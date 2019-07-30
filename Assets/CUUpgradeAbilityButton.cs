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
        text.text = CoreUpgraderScript.GetUpgradeCost(type) + " Shards";
        text.color = CoreUpgraderScript.GetUpgradeCost(type) - CoreUpgraderScript.GetShards() <= 0 ? Color.green : Color.red;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.text = CoreUpgraderScript.GetUpgradeCost(type) + " Shards";
        text.color = CoreUpgraderScript.GetUpgradeCost(type) - CoreUpgraderScript.GetShards() <= 0 ? Color.green : Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.text = "Upgrade";
        text.color = Color.white;
    }

}
