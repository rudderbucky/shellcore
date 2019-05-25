using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DWSelectionDisplayHandler : MonoBehaviour
{
    public Image shell;
    public Image core;
    public GameObject partPrefab;
    public void AssignDisplay(EntityBlueprint blueprint) {
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if(shell.sprite) {
            shell.enabled = true;
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 50;
        } else {
            shell.enabled = false;
        }
        core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        if(core.sprite) {
            core.enabled = true;
            core.rectTransform.sizeDelta = core.sprite.bounds.size * 50;
        } else {
            core.enabled = false;
        }
    }
}
