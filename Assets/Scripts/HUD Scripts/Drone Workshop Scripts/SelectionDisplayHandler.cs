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
    protected List<DisplayPart> parts = new List<DisplayPart>();

    void Awake()
    {
        ClearDisplay();
    }

    private void Update()
    {
        shell.color = core.color = miniDroneShooter.color = FactionManager.GetFactionColor(faction);
    }

    public virtual void AssignDisplay(EntityBlueprint blueprint, DroneSpawnData data, int faction = 0)
    {
        this.faction = faction;
        ClearDisplay();
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if (shell.sprite)
        {
            shell.enabled = true;
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
            shell.color = FactionManager.GetFactionColor(faction);
            shell.type = Image.Type.Sliced;
            shell.rectTransform.pivot = new Vector2(shell.sprite.pivot.x
                                                    / (shell.sprite.bounds.size.x * 100), shell.sprite.pivot.y / (shell.sprite.bounds.size.y * 100));
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
            core.type = Image.Type.Sliced;
            core.material = ResourceManager.GetAsset<Material>("material_color_swap");
            core.color = FactionManager.GetFactionColor(faction);
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
            miniDroneShooter.color = FactionManager.GetFactionColor(faction);
            miniDroneShooter.rectTransform.sizeDelta = miniDroneShooter.sprite.bounds.size * 100;
            miniDroneShooter.type = Image.Type.Sliced;
        }
        else if (blueprint.intendedType == EntityBlueprint.IntendedType.Turret ||
                 blueprint.intendedType == EntityBlueprint.IntendedType.Tank || blueprint.intendedType == EntityBlueprint.IntendedType.WeaponStation)
        {
            miniDroneShooter.enabled = true;
            miniDroneShooter.sprite =
                ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(blueprint.parts[0].abilityID, blueprint.parts[0].secondaryData));
            miniDroneShooter.color = FactionManager.GetFactionColor(faction);
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
                basePart.UpdateFaction(faction);
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
