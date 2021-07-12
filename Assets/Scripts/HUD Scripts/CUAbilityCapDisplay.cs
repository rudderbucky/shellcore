using UnityEngine;
using UnityEngine.UI;

public class CUAbilityCapDisplay : MonoBehaviour
{
    public Transform[] slotHolders;
    public GameObject[] upgradeButtonHolders;
    public GameObject slotPrefab;
    public int[] caps;
    private static CUAbilityCapDisplay instance;

    private string coreID;

    void Awake()
    {
        instance = this;
    }

    public static void Initialize(int[] caps, string coreID)
    {
        instance.initialize(caps, coreID);
    }

    private void initialize(int[] caps, string coreID)
    {
        this.caps = caps;
        this.coreID = coreID;

        for (int i = 0; i < 4; i++)
        {
            DrawSlots(i);
            var x = i;
            var button = slotHolders[i].GetChild(0).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (CoreUpgraderScript.GetUpgradeCost(x) <= CoreUpgraderScript.GetShards())
                {
                    CoreUpgraderScript.IncrementAbilityCap(x);
                }

                DrawSlots(x);
            });
        }
    }

    void DrawSlots(int type)
    {
        DestroySlots(type);
        int extras = CoreUpgraderScript.GetExtraAbilities(coreID)[type];
        for (int i = 0; i < extras; i++)
        {
            var slot = Instantiate(slotPrefab, slotHolders[type], false);
            slot.transform.SetSiblingIndex(i + 1);
            slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(-840 + 40 * i, 0);
            slot.GetComponent<Image>().color = Color.yellow;
        }

        for (int i = extras; i < CoreUpgraderScript.maxAbilityCap[type] + extras; i++)
        {
            var slot = Instantiate(slotPrefab, slotHolders[type], false);
            slot.transform.SetSiblingIndex(i + 1);
            slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(-840 + 40 * i, 0);
        }

        var text = slotHolders[type].GetComponentInChildren<Text>();
        text.text = $"{(AbilityHandler.AbilityTypes)type}: {caps[type] + extras}";
        for (int i = extras + 1; i < caps[type] + extras + 1; i++)
        {
            slotHolders[type].GetChild(i).GetComponent<Image>().color = Color.green;
        }
    }

    void DestroySlots(int type)
    {
        for (int i = 1; i < slotHolders[type].childCount; i++)
        {
            Destroy(slotHolders[type].GetChild(i).gameObject);
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < 4; i++)
        {
            DestroySlots(i);
        }
    }
}
