using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartPropertyButtonScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ShipBuilderCursorScript cursor;

    public enum ButtonType
    {
        Flip,
        Rotate
    }

    public ButtonType type;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (type == ButtonType.Flip)
        {
            cursor.FlipLastPart();
        }

        if (type == ButtonType.Rotate)
        {
            cursor.rotateMode = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (type == ButtonType.Rotate)
        {
            cursor.rotateMode = false;
        }
    }

    void Update()
    {
        if (cursor.GetLastInfo() != null)
        {
            GetComponent<Image>().enabled = true;
            var tmp = ((EntityBlueprint.PartInfo)cursor.GetLastInfo()).location * 100;
            tmp.x += (type == ButtonType.Rotate ? 25 : -25);
            tmp.y += 100;
            ((RectTransform)transform).anchoredPosition = tmp;
        }
        else
        {
            GetComponent<Image>().enabled = false;
        }
    }
}
