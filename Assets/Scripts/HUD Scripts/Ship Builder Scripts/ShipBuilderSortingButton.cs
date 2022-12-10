using UnityEngine;
using UnityEngine.UI;

public class ShipBuilderSortingButton : MonoBehaviour
{
    Button button;
    Text text;
    [SerializeField]
    ShipBuilder builder;

    void Start()
    {
        button = GetComponent<Button>();
        text = GetComponentInChildren<Text>();
        text.color = Color.green;
        DroneWorkshopModifications();
    }

    void OnEnable()
    {
        if (button)
        {
            text.color = Color.green;
        }
    }

    public void DroneWorkshopModifications()
    {
        if (builder && text && builder.mode == BuilderMode.Workshop)
        {
            if  ((builder.GetDroneWorkshopSelectPhase() && text.text != "SPAWNS")
                || (!builder.GetDroneWorkshopSelectPhase() && text.text == "SPAWNS"))
            {
                text.color = Color.gray;
            }
        }
    }

    public void ChangeColor()
    {
        text.color = text.color == Color.gray ? Color.green : Color.gray;
        if (builder && builder.mode == BuilderMode.Workshop)
        {
            if  ((builder.GetDroneWorkshopSelectPhase() && text.text != "SPAWNS")
                || (!builder.GetDroneWorkshopSelectPhase() && text.text == "SPAWNS"))
            {
                text.color = Color.gray;
            }
        }
        DroneWorkshopModifications();
    }
}
