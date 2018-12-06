using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : WeaponAbility {

    public LineRenderer line;
    public Material material;
    private bool firing;
    private float timer;
    private Vector3 victimPos;

    protected override void Awake()
    {
        base.Awake();
        line = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
        line.sortingOrder = 10;
        line.material = material;
        line.startWidth = line.endWidth = 0.2F;
        cooldownDuration = CDRemaining = 5;
        energyCost = 20;
        ID = 4;
        range = 25;
    }

    void Update()
    {
        if (firing && timer < 0.2F)
        {
            line.SetPosition(0, line.transform.position);
            line.SetPosition(1, victimPos);
            timer += Time.deltaTime;
        }
        else
        {
            
            firing = false;
            line.positionCount = 0;
        }
    }

    protected override bool Execute(Vector3 victimPos)
    {
        Core.GetTargetingSystem().GetTarget().GetComponent<Entity>().TakeDamage(1000, 0);
        line.positionCount = 2;
        this.victimPos = victimPos;
        timer = 0;
        isOnCD = true;
        firing = true;
        return true;
    }
}
