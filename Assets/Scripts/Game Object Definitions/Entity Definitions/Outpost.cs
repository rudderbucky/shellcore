using UnityEngine;

public class Outpost : AirConstruct, IVendor
{
    public VendingBlueprint vendingBlueprint;

    BattleZoneManager BZManager;

    public EntityNetworkAdapter GetAdapter()
    {
        return networkAdapter;
    }

    public bool NeedsAlliedFaction()
    {
        return true;
    }
    
    public VendingBlueprint GetVendingBlueprint()
    {
        return vendingBlueprint;
    }

    protected override void Start()
    {
        Category = EntityCategory.Station;
        base.Start();
        BZManager = GameObject.Find("SectorManager").GetComponent<BattleZoneManager>();
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
        TickAbilitiesAsStation();
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
        targeter.SetTarget(null);
        int otherFaction = faction;
        if (sectorMngr.GetCurrentType() == Sector.SectorType.BattleZone)
        {
            BZManager.UpdateCounters();
            BZManager.AttemptAlertPlayers(otherFaction, "WARNING: Outpost lost!", "clip_stationlost");
        }
        else if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide)
        {
            if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide
                && lastDamagedBy is ShellCore core && core.networkAdapter && core.networkAdapter.isPlayer.Value)
                {
                    HUDScript.AddScore(core.networkAdapter.playerName, 1);
                }
        
        }

        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;

        faction = lastDamagedBy.faction;

        for (int i = 0; i < parts.Count; i++)
        {
            RemovePart(parts[i]);
        }

        Start();
        foreach (var part in parts)
        {
            part.Start();
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
