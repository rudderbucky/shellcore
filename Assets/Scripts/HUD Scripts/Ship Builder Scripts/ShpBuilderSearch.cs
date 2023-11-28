using UnityEngine;
using UnityEngine.UI;

public class ShpBuilderSearch : MonoBehaviour
{
    public GameObject builderInterfaceContainer;
    ShipBuilder builder;
    public InputField input;
    void OnDisable()
    {
        input.text = "";
    }
}
