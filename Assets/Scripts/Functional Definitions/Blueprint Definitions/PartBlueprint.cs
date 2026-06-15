using UnityEngine;

[CreateAssetMenu(fileName = "Part", menuName = "ShellCore/Part", order = 2)]
public class PartBlueprint : ScriptableObject
{
    public string spriteID;
    public float health;
    public float mass;
    public int size;
    public bool detachible = true;
    public PartSymmetry symmetry;
}

public enum PartSymmetry
{
    None,
    MirrorXAxis,        // shape mirrored horizontally (up to down)
    MirrorYAxis,        // shape mirrored vertically (left to right)
    MirrorBothAxes      // all four corners are mirrored (180 degrees)
}