using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bunker : GroundConstruct, IVendor {

    public VendingBlueprint vendingBlueprint;
    BattleZoneManager BZManager;

    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
        BZManager = GameObject.Find("SectorManager").GetComponent<BattleZoneManager>();
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
        TargetManager.Enqueue(targeter);
        WeaponAbility[] bullets = GetComponentsInChildren<WeaponAbility>();
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].Tick();
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
        int otherFaction = faction;
        faction = lastDamagedBy.faction;
        for (int i = 0; i < parts.Count; i++)
        {
            RemovePart(parts[i]);
        }
        targeter.SetTarget(null);
        if(sectorMngr.GetCurrentType() == Sector.SectorType.BattleZone)
        {
            BZManager.UpdateCounters();
            BZManager.AlertPlayers(otherFaction, "WARNING: Bunker lost!");
        }
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

    public Vector3 GetPosition() {
        return transform.position;
    }
}
