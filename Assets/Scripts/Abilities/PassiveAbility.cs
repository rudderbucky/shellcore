using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAbility : Ability {

    private bool initialized = false;

    protected override void Execute()
    {
        
    }

    public override void Tick(string key)
    {
        if (!initialized)
        {
            Execute();
            initialized = true;
        }
    }
}
