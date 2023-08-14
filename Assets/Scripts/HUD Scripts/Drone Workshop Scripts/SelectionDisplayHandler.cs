using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionDisplayHandler : MonoBehaviour
{
    public Image shell;
    public Image core;
    public Image miniDroneShooter;
    public GameObject partPrefab;
    int faction = 0;
    float opacity = 1f;
    protected List<DisplayPart> parts = new List<DisplayPart>();

    void Awake()
    {
        ClearDisplay();
    }

    private void Update()
    {
        shell.color = core.color = miniDroneShooter.color = AdjustColorOpacity(FactionManager.GetFactionColor(faction), opacity);
    }

    public static Color AdjustColorOpacity(Color color, float opacity)
    {
        color.a *= opacity;
        return color;
    }

    public virtual void AssignDisplay(EntityBlueprint blueprint, DroneSpawnData data, int faction = 0)
    {
        this.faction = faction;
        ClearDisplay();
        if (blueprint.parts == null) 
        {
            Debug.LogWarning("No parts passed for selection display handler.");
            return;
        }
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if (shell.sprite)
        {
            shell.enabled = true;
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
            shell.color = AdjustColorOpacity(FactionManager.GetFactionColor(faction), opacity);
            shell.type = Image.Type.Sliced;
            shell.rectTransform.anchoredPosition = -shell.sprite.pivot + shell.rectTransform.sizeDelta / 2;
        }
        else
        {
            shell.enabled = false;
        }

        core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        if (core.sprite)
        {
            core.enabled = true;
            core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
            core.rectTransform.anchoredPosition = -shell.rectTransform.anchoredPosition;
            core.type = Image.Type.Sliced;
            if (blueprint.intendedType != EntityBlueprint.IntendedType.Tower)
                core.material = ResourceManager.GetAsset<Material>("material_color_swap");
            core.color = AdjustColorOpacity(FactionManager.GetFactionColor(faction), opacity);
            // orient core image so relative center stays the same regardless of shell tier
        }
        else
        {
            core.enabled = false;
        }

        if (data != null && data.type == DroneType.Mini)
        {
            miniDroneShooter.enabled = true;
            miniDroneShooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(6));
            miniDroneShooter.color = AdjustColorOpacity(FactionManager.GetFactionColor(faction), opacity);
            miniDroneShooter.rectTransform.sizeDelta = miniDroneShooter.sprite.bounds.size * 100;
            miniDroneShooter.type = Image.Type.Sliced;
        }
        else if (blueprint.intendedType == EntityBlueprint.IntendedType.Turret ||
                 blueprint.intendedType == EntityBlueprint.IntendedType.Tank || blueprint.intendedType == EntityBlueprint.IntendedType.WeaponStation)
        {
            miniDroneShooter.enabled = true;
            miniDroneShooter.sprite =
                ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(blueprint.parts[0].abilityID, blueprint.parts[0].secondaryData));
            miniDroneShooter.color = AdjustColorOpacity(FactionManager.GetFactionColor(faction), opacity);
            miniDroneShooter.rectTransform.sizeDelta = miniDroneShooter.sprite.bounds.size * 100;
            miniDroneShooter.type = Image.Type.Sliced;
        }
        else
        {
            miniDroneShooter.enabled = false;
        }

        if (blueprint.intendedType != EntityBlueprint.IntendedType.Turret
            && blueprint.intendedType != EntityBlueprint.IntendedType.Tank)
        {
            foreach (EntityBlueprint.PartInfo part in blueprint.parts)
            {
                DisplayPart basePart = Instantiate(partPrefab, transform, false).GetComponent<DisplayPart>();
                basePart.UpdateProperties(faction, opacity);
                parts.Add(basePart);
                basePart.info = part;
            }
        }
    }

    public virtual void ClearDisplay()
    {
        parts.Clear();
        shell.enabled = false;
        core.enabled = false;
        miniDroneShooter.enabled = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
