using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class AbilityHandlerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public AbilityHandler.AbilityTypes type;
    public AbilityHandler handler;
    public GameObject tooltipPrefab;
    private RectTransform tooltipTransform;

    public void OnPointerClick(PointerEventData eventData)
    {
        handler.SetCurrentVisible(type);
    }

    void Update()
    {
        if (handler.currentVisibles != type)
        {
            GetComponent<Image>().color = Color.black;
        }
        else
        {
            GetComponent<Image>().color = new Color32((byte)33, (byte)33, (byte)33, (byte)255);
        }

        if (tooltipTransform)
        {
            tooltipTransform.position = Input.mousePosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTooltip("You can press Z, X, C, or V to\nquickly switch your ability bar.");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipTransform)
        {
            Destroy(tooltipTransform.gameObject);
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
