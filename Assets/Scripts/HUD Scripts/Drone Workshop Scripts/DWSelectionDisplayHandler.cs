using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DWSelectionDisplayHandler : MonoBehaviour
{
    public Image shell;
    public Image core;
    public GameObject partPrefab;
    void Awake() {
        ClearDisplay();
    }
    public void AssignDisplay(EntityBlueprint blueprint) {
        ClearDisplay();
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if(shell.sprite) {
            shell.enabled = true;
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
        } else {
            shell.enabled = false;
        }
        core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        if(core.sprite) {
            core.enabled = true;
            core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
        } else {
            core.enabled = false;
        }
        foreach(EntityBlueprint.PartInfo part in blueprint.parts) {
            DisplayPart basePart = Instantiate(partPrefab, transform, false).GetComponent<DisplayPart>();
            basePart.info = part;
        }
    }

    public void ClearDisplay() {
        shell.enabled = false;
        core.enabled = false;
        for(int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
