using UnityEngine;
using UnityEngine.UI;

public class ShpBuilderSearch : MonoBehaviour
{
    ShipBuilder builder;
    public InputField input;
    void OnDisable()
    {
        input.text = "";
    }
}
