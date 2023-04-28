using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShipBuilderJSONButton : MonoBehaviour, IPointerClickHandler
{
    public InputField field;
    public GUIWindowScripts builder;
    public GameObject window;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (window)
        {
            window.SetActive(true);
            field.text = (builder as ShipBuilder).GetCurrentJSON();
        }
    }
}
