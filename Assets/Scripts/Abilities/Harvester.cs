using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHarvester : ITractorer
{
    void AddPower(float power);
}

public class Harvester : WeaponAbility, IHarvester {

    public ShellCore owner;
    private TractorBeam tractor;

    protected override void Start()
    {
        ID = 16;

        if(!owner)
        {
            owner = (Core as Turret).owner as ShellCore;
        }
        if(!tractor) {
            tractor = gameObject.AddComponent<TractorBeam>();
            tractor.owner = Core;
            tractor.BuildTractor();
        }
    }

    protected override bool Execute(Vector3 victimPos)
    {
        return true;
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        tractor.SetTractorTarget(newTarget);
    }

    public void AddPower(float power)
    {
        owner.AddPower(power);
    }

    public void SetOwner(ShellCore owner)
    {
        this.owner = owner;
    }

    public Draggable GetTractorTarget()
    {
        return tractor.GetTractorTarget();
    }
}
