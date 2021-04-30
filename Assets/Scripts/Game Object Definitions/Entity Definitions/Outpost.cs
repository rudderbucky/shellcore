using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : AirConstruct, IVendor {

    public VendingBlueprint vendingBlueprint;

    BattleZoneManager BZManager;

    public VendingBlueprint GetVendingBlueprint()
    {
        return vendingBlueprint;
    }

    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
        BZManager = GameObject.Find("SectorManager").GetComponent<BattleZoneManager>();
    }
    protected override void Awake()
    {
        base.Awake();
    }

    public override void RemovePart(ShellPart part) 
    {
        if (part)
        {
            if (part.gameObject.name != "Shell Sprite")
            {
                Destroy(part.gameObject);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        TargetManager.Enqueue(targeter);
        WeaponAbility[] weapons = GetComponentsInChildren<WeaponAbility>();
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].Tick();
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

        // this won't trigger PostDeath() since that only gets called if the timer ticks to a value
        // the timer doesn't tick unless isDead is set to true
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
            BZManager.AlertPlayers(otherFaction, "WARNING: Outpost lost!");
        }
        Start();
        foreach(var part in parts)
        {
            part.Start();
        }
    }

    public Vector3 GetPosition() {
        return transform.position;
    }
}
