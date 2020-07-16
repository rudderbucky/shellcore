using UnityEngine;
using System.Collections;

public class Faction : ScriptableObject
{
    public int ID;
    public string factionName;
    public Color color;
    public Color shinyColor;
    public string colorName;
    public int relations = 1;
}
