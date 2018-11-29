using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponAbility : ActiveAbility {

    protected float range;
    
    protected override void Awake()
    {
        isActive = true;
    }

    public float GetRange() {
        return range;
    }

    public override float GetActiveTimeRemaining()
    {
        if (isActive) return -1;
        else return 0;
    }

    public override void Tick(string key)
    {
        if (Input.GetKeyDown(key)) {
            core.MakeBusy();
            isActive = !isActive;
        }
        if (isOnCD)
        {
            TickDown(cooldownDuration, ref CDRemaining, ref isOnCD); // tick the cooldown time
        }
        else if (isActive && core.GetHealth()[2] >= energyCost) // if energy is sufficient and key is pressed
        {
            if (core.GetTargetingSystem().GetTarget() != null) {
                core.SetIntoCombat();
                if (Vector2.Distance(core.transform.position, core.GetTargetingSystem().GetTarget().transform.position) <= GetRange())
                {
                    Execute(core.GetTargetingSystem().GetTarget().position);
                    core.TakeEnergy(energyCost); // take energy
                }
            }
            // Execute(); // activate the special effect
        }
    }

    protected override void Execute()
    {
        Debug.Log("no argument execute is called on a weapon ability!");
    }

    protected virtual void Execute(Vector3 victimPos)
    {
        isOnCD = true;
    }
}
