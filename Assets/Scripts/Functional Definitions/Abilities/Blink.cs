using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : Ability
{
     public void Initialize()
    {
        
        abilityName = "Blink";
    }

    private GameObject blinkPrefab;
    

    protected override void Awake()
    {
        base.Awake();

        energyCost = 35;

        ID = AbilityID.Blink;
        cooldownDuration = 2;
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

    /// <summary>
    /// Heals the shell of the core (doesn't heal and refunds the energy used if it would overheal)
    /// </summary>
    protected override void Execute()
    {
        if (ExtraCriteriaToActivate())
        {
            ActivationCosmetic(transform.position);
            base.Execute();
            var pos = Input.mousePosition;
            pos.z = CameraScript.zLevel;
            var worldPos = Camera.main.ScreenToWorldPoint(pos);
            worldPos.z = 0;
            if (Core is AirCraft airCraft) airCraft.Warp(worldPos);
        }
        else
        {
            Core.TakeEnergy(-energyCost); // refund energy
            startTime = Time.time - cooldownDuration;
        }
    }
}
