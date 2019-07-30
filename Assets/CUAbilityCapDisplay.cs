using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CUAbilityCapDisplay : MonoBehaviour
{
    public Transform[] slotHolders;
    public GameObject[] upgradeButtonHolders;
    public GameObject slotPrefab;
    public int[] caps;
    private static CUAbilityCapDisplay instance;

    void Awake() {
        instance = this;
    }
    public static void Initialize(int[] caps) {
        instance.initialize(caps);
    }
    private void initialize(int[] caps) {
        this.caps = caps;

        for(int i = 0; i < 4; i++) {
            DrawSlots(i);
            var x = i;
            var button = slotHolders[i].GetChild(0).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { 
                if(CoreUpgraderScript.GetUpgradeCost(x) <= CoreUpgraderScript.GetShards()) {
                    CoreUpgraderScript.IncrementAbilityCap(x);
                }
                DrawSlots(x);
            });
            //int nextUpgradeCost = 1;
        }
    }

    void DrawSlots(int type) {
        DestroySlots(type);
        for(int i = 0; i < 15; i++) {
            var slot = Instantiate(slotPrefab, slotHolders[type], false);
            slot.transform.SetSiblingIndex(i + 1);
            slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(-840 + 40 * i, 0);
        }

        var text = slotHolders[type].GetComponentInChildren<Text>();
        text.text = (AbilityHandler.AbilityTypes)type + ": " + caps[type];
        for(int i = 1; i < caps[type] + 1; i++) {
            slotHolders[type].GetChild(i).GetComponent<Image>().color = Color.green;
        }
    }

    void DestroySlots(int type) {
        for(int i = 1; i < slotHolders[type].childCount; i++) {
            Destroy(slotHolders[type].GetChild(i).gameObject);
        }
    }

    void OnDisable() {
        for(int i = 0; i < 4; i++) {
            DestroySlots(i);
        }
    }
}
