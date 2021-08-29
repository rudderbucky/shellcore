using UnityEngine;
using UnityEngine.UI;

public class ShpBuilderSearch : MonoBehaviour
{
    public GameObject builderInterfaceContainer;
    IBuilderInterface builder;
    public InputField input;

    void Start()
    {
        builder = builderInterfaceContainer.GetComponent<IBuilderInterface>();
        input.onValueChanged.AddListener(delegate { builder.SetSearcherString(input.text); });
    }

    void OnDisable()
    {
        input.text = "";
    }
}
