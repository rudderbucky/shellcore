using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bunker : GroundConstruct, IVendor {

    public VendingBlueprint vendingBlueprint;

    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
    }
    protected override void Awake()
    {
        base.Awake();
    }

    public override void RemovePart(ShellPart part)
    {
        if (part)
            if (part.gameObject.name != "Shell Sprite")
            {
                Destroy(part.gameObject);
            }
    }

    protected override void Update()
    {
        base.Update();
        targeter.GetTarget(true);
        Bullet[] bullets = GetComponentsInChildren<Bullet>();
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].Tick(null);
        }
    }
    protected override void OnDeath()
    {
        faction = faction == 1 ? 0 : 1;
        for (int i = 0; i < parts.Count; i++)
        {
            RemovePart(parts[i]);
        }
        targeter.SetTarget(null);
        Start();
    }

    public VendingBlueprint GetVendingBlueprint()
    {
        return vendingBlueprint;
    }
}
