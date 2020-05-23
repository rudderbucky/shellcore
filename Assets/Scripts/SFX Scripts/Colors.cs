using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FactionColors
{
    public static Color[] colors = new Color[]
    {
        new Color(0f, 1f, 0f, 1F),
        new Color(1f, 0, 0, 1F),
        new Color32(218, 165, 32, 255),
    };
}

public struct ShinyFactionColors
{
    public static Color[] colors = new Color[]
    {
        new Color(0f, 1f, 0f, 1F) + new Color32(0, 0, 150, 0),
        new Color(1f, 0, 0, 1F) + new Color32(0, 0, 150, 0),
        new Color32(218, 165, 182, 255),
    };
}

public struct SectorColors
{
    public static Color[] colors = new Color[]
    {
        new Color(0, 0.3F, 0.3F),
        new Color(0, 0.4F, 0),
        new Color(0.4F, 0, 0),
        new Color(0.65F, 0, 0),
        new Color(0, 0.8F, 0),
        new Color(0.15F, 0.15F, 0.15F),
        new Color(0.15F, 0.15F, 0.15F),
    };
}