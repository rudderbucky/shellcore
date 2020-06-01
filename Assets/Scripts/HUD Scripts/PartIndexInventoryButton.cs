using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PartIndexInventoryButton : ShipBuilderInventoryBase, IPointerEnterHandler, IPointerExitHandler
{
    public PartIndexScript.PartStatus status;
    public GameObject infoBox;
    public List<string> origins = new List<string>();
    public bool displayShiny;
    public PartDisplayBase partDisplay;

    protected override void Start()
    {
        base.Start();
        val.enabled = false;
        isShiny.enabled = displayShiny;

        switch(status)
        {
            case PartIndexScript.PartStatus.Unseen:
                image.enabled = false;
                if(shooter) shooter.enabled = false;
                break;
            case PartIndexScript.PartStatus.Seen:
                image.color = Color.gray;
                if(shooter) shooter.color = Color.gray;
                break;
            default:
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoBox.SetActive(true);
        if(status != PartIndexScript.PartStatus.Unseen)
        {
            var textComponent = infoBox.GetComponentInChildren<Text>();
            textComponent.text = null;
            foreach(var origin in origins)
            {
                if(!textComponent.text.Contains(origin)) 
                {
                    if(textComponent.text == "") textComponent.text = "Sector Origins: ";
                    textComponent.text += "\n" + origin;
                }
            }

            if(status == PartIndexScript.PartStatus.Obtained)
            {
                partDisplay.gameObject.SetActive(true);
                partDisplay.DisplayPartInfo(part);
            }
            else partDisplay.gameObject.SetActive(false);
        }
        else
        {
            partDisplay.gameObject.SetActive(false);
            infoBox.GetComponentInChildren<Text>().text = "Sector Origins: ???";
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoBox.SetActive(false);
    }
}
