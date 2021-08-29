using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartIndexInventoryButton : ShipBuilderInventoryBase, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public PartIndexScript.PartStatus status;
    public GameObject infoBox;
    public List<string> origins = new List<string>();
    public bool displayShiny;
    public PartDisplayBase partDisplay;
    public static List<string> partMarkerSectorNames = new List<string>();

    protected override void Start()
    {
        base.Start();
        val.enabled = false;
        isShiny.enabled = displayShiny;

        switch (status)
        {
            case PartIndexScript.PartStatus.Unseen:
                image.enabled = false;
                if (shooter)
                {
                    shooter.enabled = false;
                }

                break;
            case PartIndexScript.PartStatus.Seen:
                image.color = Color.gray;
                if (shooter)
                {
                    shooter.color = Color.gray;
                }

                break;
            default:
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoBox.SetActive(true);
        if (status != PartIndexScript.PartStatus.Unseen)
        {
            var textComponent = infoBox.GetComponentInChildren<Text>();
            textComponent.text = "Sector Origins: (Click part to mark on map)";
            foreach (var origin in origins)
            {
                if (!textComponent.text.Contains(origin))
                {
                    textComponent.text += "\n" + origin;
                }
            }

            if (status == PartIndexScript.PartStatus.Obtained)
            {
                partDisplay.gameObject.SetActive(true);
                partDisplay.DisplayPartInfo(part);
            }
            else
            {
                partDisplay.gameObject.SetActive(false);
            }
        }
        else
        {
            partDisplay.gameObject.SetActive(false);
            infoBox.GetComponentInChildren<Text>().text = "Sector Origins: ???";
        }
    }

    ///
    /// Sets up the markers for the map and switches to it.
    ///
    public void OnPointerClick(PointerEventData eventData)
    {
        if (status != PartIndexScript.PartStatus.Unseen)
        {
            partMarkerSectorNames = origins.Distinct().ToList();
            StatusMenu.instance.SwitchSections(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoBox.SetActive(true);
        partDisplay.gameObject.SetActive(false);
        infoBox.GetComponentInChildren<Text>().text = "Hover over a part to see information here";
    }
}
