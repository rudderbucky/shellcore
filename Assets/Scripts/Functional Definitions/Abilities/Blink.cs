using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : Ability
{
    private GameObject blinkPrefab;

    protected override void Awake()
    {
        base.Awake();
        abilityName = "Blink";
        energyCost = 75;
        ID = AbilityID.Blink;
        cooldownDuration = 15;
    }

    protected override bool ExtraCriteriaToActivate()
    {
        return !Core.IsInvisible;
    }

    public override void ActivationCosmetic(Vector3 targetPos)
    {
        blinkPrefab = ResourceManager.GetAsset<GameObject>("respawn_implosion");
        Instantiate(blinkPrefab, Core.transform.position, Quaternion.identity);
        base.ActivationCosmetic(targetPos);
    }

    protected override void Execute()
    {
        if (!ExtraCriteriaToActivate())
            return;

        bool isPlayer = Core is PlayerCore ? true : false;

        if (isPlayer)
        {
            var pos = Input.mousePosition;
            pos.z = CameraScript.zLevel;
            var worldPos = Camera.main.ScreenToWorldPoint(pos);
            worldPos.z = 0;
            ActivateBlink(worldPos);
        }
        else
        {
            ActivateBlink(Core.transform.position + Quaternion.Euler(0, 0, Random.Range(0, 360)) * new Vector3(15, 0, 0));
            ActivationCosmetic(transform.position);
        }
        
        AudioManager.PlayClipByID("clip_respawn", transform.position);
        base.Execute();
    }

    private void ActivateBlink(Vector3 pos)
    {
        if (Core is AirCraft airCraft)
            airCraft.Warp(pos);
    }
}
