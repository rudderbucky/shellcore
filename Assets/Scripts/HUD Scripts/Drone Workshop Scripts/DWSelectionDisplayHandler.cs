using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DWSelectionDisplayHandler : MonoBehaviour
{
    public Image shell;
    public Image core;
    public Image miniDroneShooter;
    public GameObject partPrefab;
    void Awake() {
        ClearDisplay();
    }
    public void AssignDisplay(EntityBlueprint blueprint, DroneSpawnData data) {
        ClearDisplay();
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if(shell.sprite) {
            shell.enabled = true;
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
            shell.color = FactionColors.colors[0];
        } else {
            shell.enabled = false;
        }
        core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        if(core.sprite) {
            core.enabled = true;
            core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
            core.type = Image.Type.Sliced;
        } else {
            core.enabled = false;
        }
        if(data.type == DroneType.Mini) {
            miniDroneShooter.enabled = true;
            miniDroneShooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(6));
            miniDroneShooter.color = FactionColors.colors[0];
            miniDroneShooter.rectTransform.sizeDelta = miniDroneShooter.sprite.bounds.size * 100;
            miniDroneShooter.type = Image.Type.Sliced;
        } else {
            miniDroneShooter.enabled = false;
        }
        foreach(EntityBlueprint.PartInfo part in blueprint.parts) {
            DisplayPart basePart = Instantiate(partPrefab, transform, false).GetComponent<DisplayPart>();
            basePart.info = part;
        }
    }

    public void ClearDisplay() {
        shell.enabled = false;
        core.enabled = false;
        miniDroneShooter.enabled = false;
        for(int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
