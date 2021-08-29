using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShipBuilderJSONButton : MonoBehaviour, IPointerClickHandler
{
    public InputField field;
    public GUIWindowScripts builder;
    public GUIWindowScripts window;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (window)
        {
            window.ToggleActive();
            field.text = (builder as IBuilderInterface).GetCurrentJSON();
        }
    }
}
