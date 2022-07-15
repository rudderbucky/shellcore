using UnityEngine;
using UnityEngine.EventSystems;

public class PointerOverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool isPointerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

}
