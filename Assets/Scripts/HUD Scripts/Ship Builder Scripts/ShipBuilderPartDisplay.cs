using UnityEngine.UI;

public class ShipBuilderPartDisplay : PartDisplayBase
{
    public void Awake()
    {
        image.type = Image.Type.Sliced;
        abilityImage.type = Image.Type.Sliced;
        abilityTier.type = Image.Type.Sliced;
        SetInactive();
    }
}
