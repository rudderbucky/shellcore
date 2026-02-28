using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WCBetterBarHandler : MonoBehaviour
{
    public GameObject betterBarButtonPrefab;
    public Transform betterBarContents;
    public ItemHandler itemHandler;
    public Sprite betterButtonActive;
    public Sprite betterButtonInactive;
    public int currentActiveButton;
    public Text itemName;
    public List<Image> images = new List<Image>();
    public int minButton = 0;
    public int maxButton = 7;
    public int padding = 1;
    public WorldCreatorCursor cursor;
    public GameObject optionButtonPrefab;
    public Transform optionGridTransform;
    private List<OptionButton> activeOptionButtons = new List<OptionButton>();
    public GameObject tooltipPrefab;
    private RectTransform tooltipTransform;
    public WCWorldIO WCWorldIO;
    public Sprite playButtonImage;
    public static WCBetterBarHandler instance;
    public GameObject barContainer;
    public Image modeGobj;

    /// <summary>
    /// Option buttons for the World Creator
    /// </summary>
    [System.Serializable]
    public class OptionButton
    {
        public string tooltip;
        public Sprite sprite;
        public Image imgRef;
        public WorldCreatorCursor.WCCursorMode mode;
        public Button.ButtonClickedEvent action;
    }

    public List<OptionButton> globalButtons;
    public List<OptionButton> extraButtons;

    public static void UpdateActiveButtons()
    {
        instance.updateActiveButtons();
    }

    // Activates/deactivates extra buttons according to the current cursor mode
    void updateActiveButtons()
    {
        foreach (var extraButton in extraButtons)
        {
            extraButton.imgRef.gameObject.SetActive(extraButton.mode == cursor.GetMode());
            // extraButton.imgRef.GetComponentsInChildren<Image>()[1].color = cursor.modeColors[(int)cursor.GetMode()] + Color.gray;
        }
    }

    void AddOptionButton(int i, List<OptionButton> buttonList)
    {
        var gObj = Instantiate(optionButtonPrefab, optionGridTransform);
        var button = buttonList[i];
        button.imgRef = gObj.GetComponent<Image>();
        if (button.sprite)
        {
            var headerImg = gObj.GetComponentsInChildren<Image>()[1];
            headerImg.sprite = button.sprite;
            headerImg.rectTransform.sizeDelta = button.sprite.bounds.size * 100;
        }
        else
        {
            gObj.GetComponentsInChildren<Image>()[1].enabled = false;
        }

        var buttonScript = button.imgRef.gameObject.AddComponent<Button>();
        buttonScript.onClick = button.action;
        activeOptionButtons.Add(button);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < itemHandler.itemPack.items.Count; i++)
        {
            var buttonObj = Instantiate(betterBarButtonPrefab, betterBarContents, false);
            var x = i;
            buttonObj.AddComponent<Button>().onClick.AddListener(new UnityAction(() =>
            {
                currentActiveButton = x;
                cursor.SetCurrent(x);
            }));
            var img = buttonObj.GetComponent<Image>();
            images.Add(img);
            if (itemHandler.itemPack.items[i].assetID != "")
            {
                if (ResourceManager.Instance.resourceExists(itemHandler.itemPack.items[i].assetID) &&
                    ResourceManager.GetAsset<Object>(itemHandler.itemPack.items[i].assetID) as EntityBlueprint)
                {
                    var print = ResourceManager.GetAsset<EntityBlueprint>(itemHandler.itemPack.items[i].assetID);
                    buttonObj.transform.Find("MaskObj").Find("StandardImage").gameObject.SetActive(false);
                    var scaler = buttonObj.transform.Find("MaskObj").Find("EntityDisplay").transform;

                    switch (print.intendedType)
                    {
                        case EntityBlueprint.IntendedType.AirCarrier:
                        case EntityBlueprint.IntendedType.GroundCarrier:
                        case EntityBlueprint.IntendedType.Yard:
                        case EntityBlueprint.IntendedType.Trader:
                        case EntityBlueprint.IntendedType.WeaponStation:
                        case EntityBlueprint.IntendedType.CoreUpgrader:
                        case EntityBlueprint.IntendedType.DroneWorkshop:
                        case EntityBlueprint.IntendedType.FusionStation:
                            scaler.localScale = new Vector3(0.15f, 0.15f, 1);
                            break;
                        default:
                            scaler.localScale = new Vector3(0.3f, 0.3f, 1);
                            break;
                    }

                    var sdh = buttonObj.GetComponentInChildren<SelectionDisplayHandler>();
                    sdh.AssignDisplay(print, print.intendedType == EntityBlueprint.IntendedType.Drone ? DroneUtilities.GetDefaultData(print.customDroneType) : null);
                }
                else
                {
                    var obj = itemHandler.itemPack.items[i].obj;
                    SetStandardImage(obj, buttonObj, 0.5f);
                }
            }
            else
            {
                var obj = itemHandler.itemPack.items[i].obj;
                SetStandardImage(obj, buttonObj, 0.5f);
            }
        }

        for (int i = 0; i < globalButtons.Count; i++)
        {
            AddOptionButton(i, globalButtons);
        }

        for (int i = 0; i < extraButtons.Count; i++)
        {
            AddOptionButton(i, extraButtons);
        }

        UpdateActiveButtons();
    }

    void SetStandardImage(GameObject obj, GameObject buttonObj, float scale = 1)
    {
        buttonObj.transform.Find("MaskObj").Find("EntityDisplay").gameObject.SetActive(false);
        var spriteList = obj.GetComponentsInChildren<SpriteRenderer>();
        var standardImage = buttonObj.transform.Find("MaskObj").Find("StandardImage");
        standardImage.localScale = new Vector3(scale, scale, 1);
        var standardImageList = standardImage.GetComponentsInChildren<Image>();
        standardImageList[0].sprite = spriteList[0].sprite;
        SetAspectRatio(standardImageList[0]);
        standardImageList[0].color = spriteList[0].color;
        if (spriteList.Length > 1 && spriteList[1].sprite.name != "minimapsquare")
        {
            standardImageList[1].sprite = spriteList[1].sprite;
            SetAspectRatio(standardImageList[1]);
            standardImageList[0].color = spriteList[0].color;
        }
        else
        {
            standardImageList[1].enabled = false;
        }
    }

    private void SetAspectRatio(Image image)
    {
        var x = image.sprite.bounds.extents.x > image.sprite.bounds.extents.y ? 100 : (image.sprite.bounds.extents.x / image.sprite.bounds.extents.y) * 100;
        var y = image.sprite.bounds.extents.x < image.sprite.bounds.extents.y ? 100 : (image.sprite.bounds.extents.y / image.sprite.bounds.extents.x) * 100;
        image.rectTransform.sizeDelta = new Vector2(x, y);
    }

    void Update()
    {
        currentActiveButton = cursor.currentIndex;
        itemName.color = cursor.modeColors[(int)cursor.GetMode()] + Color.gray;

        if (cursor.GetMode() == WorldCreatorCursor.WCCursorMode.Item)
        {
            barContainer.SetActive(true);
            foreach (Image image in images)
            {
                image.sprite = betterButtonInactive;
            }

            var test = betterBarContents.GetChild(currentActiveButton).GetComponent<Image>();
            test.sprite = betterButtonActive;
            while ((currentActiveButton > maxButton - padding) && (currentActiveButton < images.Count - 1))
            {
                minButton++;
                maxButton++;

            }
            while (currentActiveButton < minButton + padding && (currentActiveButton > 0))
            {
                minButton--;
                maxButton--;
            }
            if (currentActiveButton == 0)
            {
                minButton = 0;
                maxButton = 7;
            }
            if (currentActiveButton == images.Count - 1)
            {
                minButton = images.Count - 1 - 7;
                maxButton = images.Count - 1;
            }
            (betterBarContents as RectTransform).anchoredPosition = new Vector2(-(maxButton - 7) * 125, 0);

            itemName.text = itemHandler.itemPack.items[currentActiveButton].name.ToUpper();
        }
        else
        {
            barContainer.SetActive(false);
        }

        // Instantiate tooltip. Destroy tooltip if mouse is not over a sector image.
        bool mouseOverSector = false;
        Vector3 pos, sizeDelta;
        Rect newRect;
        foreach (var optionButton in activeOptionButtons)
        {
            if (!optionButton.imgRef.gameObject.activeSelf)
            {
                continue;
            }

            pos = optionButton.imgRef.rectTransform.position;
            sizeDelta = optionButton.imgRef.rectTransform.sizeDelta / UIScalerScript.GetScale();
            newRect = new Rect(pos.x - sizeDelta.x / 2, pos.y - sizeDelta.y / 2, sizeDelta.x, sizeDelta.y);
            // Mouse over sector. Instantiate tooltip if necessary, move tooltip and set text up
            if (newRect.Contains(Input.mousePosition))
            {
                mouseOverSector = true;
                SetTooltip(optionButton.tooltip);
            }
        }

        pos = modeGobj.rectTransform.position;
        sizeDelta = modeGobj.rectTransform.sizeDelta / UIScalerScript.GetScale();
        newRect = new Rect(pos.x - sizeDelta.x, pos.y, sizeDelta.x, sizeDelta.y);
        if (newRect.Contains(Input.mousePosition))
        {
            mouseOverSector = true;
            SetTooltip("Click to change mode.", -1);
        }

        if (!mouseOverSector)
        {
            if (tooltipTransform)
            {
                Destroy(tooltipTransform.gameObject);
            }
        }
    }

    void SetTooltip(string displayText, int scale = 1)
    {
        if (!tooltipTransform)
        {
            tooltipTransform = Instantiate(tooltipPrefab, transform.parent).GetComponent<RectTransform>();
        }

        tooltipTransform.position = Input.mousePosition;
        var text = tooltipTransform.GetComponentInChildren<Text>();
        tooltipTransform.localScale = text.rectTransform.localScale = new Vector3(scale, 1, 1);
        text.text =
            $"{displayText}".ToUpper();
        tooltipTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(text.preferredWidth + 16, text.preferredHeight + 16);
    }
}
