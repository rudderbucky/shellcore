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
        WeaponAbility[] bullets = GetComponentsInChildren<WeaponAbility>();
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].Tick(null);
        }
    }

    protected override void DeathHandler()
    {
        if (currentHealth[0] <= 0 && !isDead)
        {
            OnDeath(); // switch factions
        }
    }

    protected override void OnDeath()
    {
        int oldFaction = faction;
        faction = faction == 1 ? 0 : 1;
        for (int i = 0; i < parts.Count; i++)
        {
            RemovePart(parts[i]);
        }
        targeter.SetTarget(null);
        GameObject.Find("SectorManager").GetComponent<BattleZoneManager>().AlertPlayers(oldFaction, "WARNING: Bunker lost!");
        GameObject.Find("SectorManager").GetComponent<BattleZoneManager>().UpdateCounters();
        Start();
        foreach (var part in parts)
        {
            part.Start();
        }
    }

    public VendingBlueprint GetVendingBlueprint()
    {
        return vendingBlueprint;
    }
}
