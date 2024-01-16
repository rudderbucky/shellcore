using System.Collections.Generic;
using UnityEngine;

public class ChainBeam : Beam
{

    protected override void Awake()
    {
        base.Awake();
        ID = AbilityID.ChainBeam;
        abilityName = "Chain Beam";
        description = $"Instant attack that deals {damage} damage to multiple targets.";
        range = 15;
        cooldownDuration = 4f;
        energyCost = 225;
        MAX_BOUNCES = 3;
    }

    protected override bool Execute(Vector3 victimPos)
    {
        base.Execute(victimPos);
        return true;
    }
}
