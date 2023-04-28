using UnityEngine;
using UnityEngine.UI;

public class ShpBuilderSearch : MonoBehaviour
{
    public GameObject builderInterfaceContainer;
    ShipBuilder builder;
    public InputField input;

    void Start()
    {
        builder = builderInterfaceContainer.GetComponent<ShipBuilder>();
        input.onValueChanged.AddListener(delegate { builder.SetSearcherString(input.text); });
    }

    void OnDisable()
    {
        input.text = "";
    }
}
