using UnityEngine;
using UnityEngine.EventSystems; 

public class DragDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool dragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }
}
