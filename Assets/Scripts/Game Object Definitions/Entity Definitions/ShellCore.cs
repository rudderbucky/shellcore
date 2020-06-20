using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft, IHarvester, IOwner {

    protected ICarrier carrier;
    protected float totalPower;
    protected GameObject bulletPrefab; // prefab for main bullet
    public int intrinsicCommandLimit;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    private TractorBeam tractor;

    public int GetTotalCommandLimit()
    {
        if (sectorMngr)
        {
            return intrinsicCommandLimit + sectorMngr.GetExtraCommandUnits(faction);
        }
        else return intrinsicCommandLimit;
    }

    public void SetCarrier(ICarrier carrier)
    {
        this.carrier = carrier;
    }

    public ICarrier GetCarrier()
    {
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

        if(!(this as PlayerCore) && !ai)
        {
            ai = gameObject.AddComponent<AirCraftAI>();
        }
        if(ai && ai.getMode() == AirCraftAI.AIMode.Inactive)
        {
            ai.Init(this);
            if(sectorMngr.current.type == Sector.SectorType.BattleZone)
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

    protected override void Respawn()
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
}
