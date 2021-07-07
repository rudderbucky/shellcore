using UnityEngine;
using UnityEngine.UI;

public class ShipBuilderPartDisplay : PartDisplayBase
{
    public IBuilderInterface builder;
    public GameObject emptyInfoMarker;
    public ShipBuilderCursorScript cursorScript;
    public RectTransform builderBG;
    bool initialized = false;

    public void Initialize(IBuilderInterface inter)
    {
        builder = inter;
        initialized = true;
        image.type = Image.Type.Sliced;
        abilityImage.type = Image.Type.Sliced;
        abilityTier.type = Image.Type.Sliced;
        SetInactive();
    }

    void SetInactive()
    {
        emptyInfoMarker.SetActive(true);
        abilityBox.gameObject.SetActive(false);
        abilityImage.gameObject.SetActive(false);
        abilityText.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
        partName.gameObject.SetActive(false);
        partStats.gameObject.SetActive(false);
        abilityTier.gameObject.SetActive(false);
    }

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
