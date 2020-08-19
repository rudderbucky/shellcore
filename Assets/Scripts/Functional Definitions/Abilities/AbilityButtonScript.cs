using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.

public class AbilityButtonScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool clicked;
    public GameObject tooltipPrefab;
    public string abilityInfo;
    private GameObject tooltip;

    private void Update()
    {
        if(tooltip)
        {
            tooltip.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //create tooltip
        tooltip = Instantiate(tooltipPrefab);
        RectTransform rect = tooltip.GetComponent<RectTransform>();
        rect.position = eventData.position;
        rect.SetParent(transform.parent, true);
        rect.SetAsLastSibling();

        Text text = tooltip.transform.Find("Text").GetComponent<Text>();
        text.text = abilityInfo;

        rect.sizeDelta = new Vector2(text.preferredWidth + 16f, text.preferredHeight + 16);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //delete tooltip
        if (tooltip)
            Destroy(tooltip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clicked = true;
    }

    public void OnDisable() {
        OnPointerExit(null);
    }
}
