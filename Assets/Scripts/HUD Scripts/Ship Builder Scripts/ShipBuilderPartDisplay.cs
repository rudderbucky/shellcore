using UnityEngine;
using UnityEngine.UI;

public class ShipBuilderPartDisplay : PartDisplayBase
{
    public ShipBuilder builder;
    public ShipBuilderCursorScript cursorScript;
    public RectTransform builderBG;
    bool initialized = false;

    public void Initialize(ShipBuilder inter)
    {
        builder = inter;
        initialized = true;
        image.type = Image.Type.Sliced;
        abilityImage.type = Image.Type.Sliced;
        abilityTier.type = Image.Type.Sliced;
        SetInactive();
    }


    // Update is called once per frame
    void Update()
    {
        // Gets the part to diplsay using the method GetButtonPartCursorIsOn() in IBuilderInterface

        if (initialized)
        {
            EntityBlueprint.PartInfo? part = null;
            if (cursorScript.GetPartCursorIsOn() != null)
            {
                part = cursorScript.GetPartCursorIsOn();
            }

            if (part != null)
            {
                EntityBlueprint.PartInfo info = (EntityBlueprint.PartInfo)part;
                DisplayPartInfo(info);
                emptyInfoMarker.SetActive(false);
            }
            else
            {
                SetInactive();
            }
        }
    }
}
