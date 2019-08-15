using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CUUpgradeAbilityButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Text text;
    public int type;

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateButtonCost(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateButtonCost(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateButtonCost(false);
    }

    public void OnEnable() {
        UpdateButtonCost(false);
    }

    public void UpdateButtonCost(bool mouseOver) {
        if(CoreUpgraderScript.maxAbilityCap[type] > CoreUpgraderScript.instance.player.abilityCaps[type]) {
            if(mouseOver) {
                text.text = CoreUpgraderScript.GetUpgradeCost(type) + " Shards";
                text.color = CoreUpgraderScript.GetUpgradeCost(type) - CoreUpgraderScript.GetShards() <= 0 ? Color.green : Color.red;  
            }
            else {
                text.text = "Upgrade";
                text.color = Color.white;                   
            }
        } else {
            text.text = "MAX";
            text.color = Color.yellow;
        }
    }
}
