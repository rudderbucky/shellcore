using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft, IHarvester, IOwner {

    public delegate void PowerCollectDelegate(int faction, int amount);
    public static PowerCollectDelegate OnPowerCollected;

    protected ICarrier carrier;
    protected float totalPower;
    protected GameObject bulletPrefab; // prefab for main bullet
    public int intrinsicCommandLimit;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    private TractorBeam tractor;

    public int GetTotalCommandLimit()
    {
        return Mathf.Min(intrinsicCommandLimit + (sectorMngr ? sectorMngr.GetExtraCommandUnits(faction) : 0), 99);
    }

    public void SetCarrier(ICarrier carrier)
    {
        this.carrier = carrier;
    }

    public ICarrier GetCarrier()
    {
        if(carrier == null || carrier.Equals(null) || carrier.GetIsDead()) return null;
        return carrier;
    }

    public void ResetPower() {
        totalPower = 0;
    }

    public float GetPower()
    {
        return totalPower;
    }

    public void AddPower(float power)
    {
        totalPower += power;

        if (power > 0 && OnPowerCollected != null)
            OnPowerCollected.Invoke(faction, Mathf.RoundToInt(power));
    }

    protected override void OnDeath()
    {
        tractor.SetTractorTarget(null);
        base.OnDeath();
    }


    public SectorManager GetSectorManager() {
        return sectorMngr;
    }
    protected override void Start()
    {
        if ((carrier != null && !carrier.Equals(null)) && carrier.GetIsInitialized())
        {
            spawnPoint = carrier.GetSpawnPoint();
        }
        transform.position = spawnPoint;
        // initialize instance fields
        base.Start(); // base start

        ai = GetAI();
        if (ai && ai.getMode() == AirCraftAI.AIMode.Inactive)
        {
            if(sectorMngr.GetCurrentType() == Sector.SectorType.BattleZone)
            {
                ai.setMode(AirCraftAI.AIMode.Battle);
            }
            else
            {
                ai.setMode(AirCraftAI.AIMode.Inactive);
            }
            ai.allowRetreat = true;
        }
    }

    public override void Respawn()
    {
        if ((carrier != null && !(carrier as Entity).GetIsDead()) || this as PlayerCore)
        {
            base.Respawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected override void BuildEntity()
    {
        intrinsicCommandLimit = 0;
        if(!tractor.initialized) {
            tractor.BuildTractor();
            switch (CoreUpgraderScript.GetCoreTier(blueprint.coreShellSpriteID)) {
                case 0:
                    tractor.maxRangeSquared = 225;
                    tractor.energyPickupRangeSquared = 160;
                    tractor.maxBreakRangeSquared = 600;
                    break;
                case 1:
                    tractor.maxRangeSquared = 325;
                    tractor.energyPickupRangeSquared = 240;
                    tractor.maxBreakRangeSquared = 900;
                    break;
                case 2:
                    tractor.maxRangeSquared = 450;
                    tractor.energyPickupRangeSquared = 320;
                    tractor.maxBreakRangeSquared = 1200;
                    break;
                case 3:
                    tractor.maxRangeSquared = 550;
                    tractor.energyPickupRangeSquared = 400;
                    tractor.maxBreakRangeSquared = 1500;
                    break;
            }
        }
        base.BuildEntity();
    }

    protected override void Awake()
    {
        respawns = true;
        if(!tractor)
        {
            tractor = gameObject.AddComponent<TractorBeam>();
            tractor.owner = this;
        }
        base.Awake(); // base awake
    }

    protected override void Update() {
        base.Update(); // base update
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        tractor.SetTractorTarget(newTarget);
    }

    public Draggable GetTractorTarget()
    {
        return tractor ? tractor.GetTractorTarget() : null;
    }


    public List<IOwnable> GetUnitsCommanding()
    {
        return unitsCommanding;
    }

    public void PowerHeal()
    {
        TakeShellDamage(-0.05F * GetMaxHealth()[0], 0, null);
        TakeCoreDamage(-0.05F * GetMaxHealth()[1]);
        TakeEnergy(-0.05F * GetMaxHealth()[2]);
    }

    public int GetIntrinsicCommandLimit()
    {
        return intrinsicCommandLimit;
    }

    public void SetIntrinsicCommandLimit(int val)
    {
        intrinsicCommandLimit = val;
    }
}
