using UnityEngine;

public class Faction : ScriptableObject, IBaseProperty
{
    public int ID;
    public string factionName;
    public Color color;
    public Color shinyColor;
    public string colorName;
    public int relations = 1;
    public bool growRandomParts;

    public string GetName()
    {
        return factionName;
    }
}
