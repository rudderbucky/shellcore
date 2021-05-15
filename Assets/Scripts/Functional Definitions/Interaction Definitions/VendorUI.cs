using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IVendor
{
    VendingBlueprint GetVendingBlueprint();
    int GetFaction();
    Vector3 GetPosition();
}

public class VendorUI : MonoBehaviour, IDialogueable, IWindow
{
    public VendingBlueprint blueprint;
    public GameObject UIPrefab;
    public GameObject buttonPrefab;
    public PlayerCore player;
    private IVendor vendor;
    private GameObject UI;
    private Transform background;
    private bool opened;
    private GameObject[] buttons;
    private Text costInfo;
    private Text nameInfo;
    public int range;
    public GameObject tooltipPrefab;
    
    public bool GetActive() {
		return UI && UI.activeSelf;
	}
    public void openUI()
    {
        if(opened) 
        {
            var vendor = this.vendor;
            var player = this.player;
            CloseUI();
            SetVendor(vendor, player);
        }
        
        if (!blueprint)
        {
            Debug.Log("No blueprint!");
            return;
        }
        if (!UIPrefab)
        {
            UIPrefab = ResourceManager.GetAsset<GameObject>("vendor_ui");
        }
        if (!buttonPrefab)
        {
            buttonPrefab = ResourceManager.GetAsset<GameObject>("vendor_button");
        }
        UI = Instantiate(UIPrefab);
        UI.GetComponentInChildren<GUIWindowScripts>().Activate();

        background = UI.transform.Find("Container").Find("Background");
        Button close = background.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(CloseUI);
        costInfo = background.transform.Find("Cost").GetComponent<Text>();
        nameInfo = background.transform.Find("Name").GetComponent<Text>();
        costInfo.text = "";
        range = blueprint.range;


        buttons = new GameObject[blueprint.items.Count];
        for (int i = 0; i < blueprint.items.Count; i++)
        {
            int index = i;
            buttons[i] = Instantiate(buttonPrefab);

            RectTransform rt = buttons[i].GetComponent<RectTransform>();
            rt.SetParent(background, false);
            rt.anchoredPosition = new Vector2(185 + (i % 5) * 64, -40 - (i > 4 ? 64 : 0));

            Button button = buttons[i].GetComponent<Button>();
            button.onClick.AddListener(() => { onButtonPressed(index); });

            VendorUIButton vendorUIButton = buttons[i].GetComponent<VendorUIButton>();

            if(player.GetPower() < blueprint.items[i].cost) 
                buttons[i].GetComponent<Image>().color = new Color(0,0,0.4F);

            vendorUIButton.blueprint = blueprint.items[i].entityBlueprint;
            vendorUIButton.costText = "POWER COST: <color=cyan>" + blueprint.items[i].cost + "</color>";
            vendorUIButton.descriptionText = blueprint.items[i].description;
            vendorUIButton.tooltipPrefab = tooltipPrefab;
            vendorUIButton.costInfo = costInfo;
            vendorUIButton.nameInfo = nameInfo;
            vendorUIButton.handler = UI.GetComponentInChildren<SelectionDisplayHandler>();

            Image sr = buttons[i].transform.Find("Icon").GetComponent<Image>();
            sr.sprite = blueprint.items[i].icon;

            Text[] texts = buttons[i].GetComponentsInChildren<Text>();
            texts[0].text = i + 1 + "";
            texts[1].text = blueprint.items[i].cost + "";
            texts[1].color = Color.cyan;
        }
        opened = true;
    }

    public void SetVendor(IVendor vendor, PlayerCore player) {
        this.player = player;
        this.vendor = vendor;
        blueprint = vendor.GetVendingBlueprint();
    }

    private void ClearVendor() {
        this.player = null;
        this.vendor = null;
        this.blueprint = null;
    }

    private void Update()
    {
        if (opened)
        {
            if(vendor == null) 
            {
                Debug.LogWarning("No vendor!");
                return;
            }

            if(player == null) 
            {
                Debug.Log("No player set! Using default player.");
                player = PlayerCore.Instance;
            }

            if(player && vendor.GetFaction() != player.faction) {
                Debug.Log("Vendor faction changed");
                CloseUI();
                return;
            }
            if((vendor.GetPosition() - player.transform.position).sqrMagnitude > range)
            {
                Debug.Log("Player moved out of the vendor range");
                CloseUI();
                return;
            }
            for (int i = 0; i < blueprint.items.Count; i++)
            {
                if(player.GetPower() < blueprint.items[i].cost)
                {
                    buttons[i].GetComponent<Image>().color = new Color(0, 0, 0.4F);
                } else buttons[i].GetComponent<Image>().color = Color.white;
            }
        } 
    }

    public void CloseUI()
    { 
        UI.GetComponentInChildren<GUIWindowScripts>().CloseUI();
        opened = false;
        Destroy(UI);
        ClearVendor();
    }

    public void onButtonPressed(int index)
    {
        // TODO: this is invalid for non ownable items, so must be changed later on
        if (player.GetPower() >= blueprint.items[index].cost && player.faction == vendor.GetFaction()
            && player.unitsCommanding.Count < player.GetTotalCommandLimit())
        {
            GameObject creation = new GameObject();
            switch(blueprint.items[index].entityBlueprint.intendedType)
            {
                case EntityBlueprint.IntendedType.Turret:
                    Turret tur = creation.AddComponent<Turret>();
                    tur.blueprint = blueprint.items[index].entityBlueprint;
                    tur.SetOwner(player);
                    break;
                case EntityBlueprint.IntendedType.Tank:
                    Tank tank = creation.AddComponent<Tank>();
                    tank.blueprint = blueprint.items[index].entityBlueprint;
                    tank.SetOwner(player);
                    break;
                default:
                    break;
            }
            creation.name = blueprint.items[index].entityBlueprint.name;
            player.sectorMngr.InsertPersistentObject(blueprint.items[index].entityBlueprint.name, creation);
            creation.transform.position = vendor.GetPosition();
            creation.GetComponent<Entity>().spawnPoint = vendor.GetPosition();
            if (blueprint.items[index].entityBlueprint.intendedType != EntityBlueprint.IntendedType.Tank)
                player.SetTractorTarget(creation.GetComponent<Draggable>());
            player.AddPower(-blueprint.items[index].cost);
            if(GetActive()) CloseUI();
            ClearVendor();
        } else if(player as PlayerCore && player.GetUnitsCommanding().Count >= player.GetTotalCommandLimit())
        {
            player.alerter.showMessage("Unit limit reached!", "clip_alert");
        }
    }

    public UnityEvent GetOnCancelled()
    {
        return null;
    }
}
