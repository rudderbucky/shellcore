using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class VendorUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string costText;
    public string descriptionText;
    public Text nameInfo;
    public Text costInfo;
    public SelectionDisplayHandler handler;
    public EntityBlueprint blueprint;
    public GameObject tooltipPrefab;
    private GameObject tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        costInfo.text = costText;
        nameInfo.text = blueprint.entityName.ToUpper();
        handler.AssignDisplay(blueprint, null);

        // create tooltip
        tooltip = Instantiate(tooltipPrefab);
        RectTransform rect = tooltip.GetComponent<RectTransform>();
        rect.position = eventData.position;
        rect.SetParent(transform.parent, true);
        rect.SetAsLastSibling();

        Text desc = tooltip.transform.Find("Text").GetComponent<Text>();
        desc.text = descriptionText;

        rect.sizeDelta = new Vector2(desc.preferredWidth + 16f, desc.preferredHeight + 16);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        costInfo.text = nameInfo.text = "";
        handler.ClearDisplay();

        // destroy tooltip
        if (tooltip)
            Destroy(tooltip);
    }

    public void OnDisable() {
        // destroy tooltip
        if (tooltip)
            Destroy(tooltip);
    }
    private void Update()
    {
        if(tooltip)
        {
            tooltip.transform.position = Input.mousePosition;
        }
    }
}
