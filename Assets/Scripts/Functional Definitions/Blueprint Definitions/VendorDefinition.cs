using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorDefinition : ScriptableObject
{
    public string entityBlueprint;
    public string vendingBlueprint;
}

[System.Serializable]
public class VendorList
{
    public string entityBlueprint;
    public string vendingBlueprint;
}
