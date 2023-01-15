using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MasterNetworkAdapter;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft, IHarvester, IOwner
{
    public delegate void PowerCollectDelegate(int faction, int amount);

    public static PowerCollectDelegate OnPowerCollected;

    protected ICarrier carrier;
    protected float totalPower;
    protected GameObject bulletPrefab; // prefab for main bullet
    public int intrinsicCommandLimit;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    private TractorBeam tractor;
    public bool isYardRepairing = false;
    private float yardRepairDuration = 1f;

    private List<EntityBlueprint.PartInfo> partsToRepairAdd = new List<EntityBlueprint.PartInfo>();

    private Coroutine yardRepairCoroutine;
    private Coroutine addRandomPartsCoroutine;
    public bool husk;

    public void StartYardRepairCoroutine()
    {
        yardRepairCoroutine = StartCoroutine(StartYardRepair());
    }

    public void StopYardRepairCoroutine()
    {
        if (yardRepairCoroutine != null)
        {
            StopCoroutine(yardRepairCoroutine);
        }
    }

    private IEnumerator StartYardRepair()
    {
        isYardRepairing = true;
        foreach (var part in parts)
        {
            if (part.name != "Shell Sprite")
            {
                part.Detach();
                part.GetComponentInChildren<Ability>()?.SetDestroyed(true);
                Destroy(part.gameObject, 3);
            }
        }
        parts.RemoveAll(p => p.name != "Shell Sprite");
        ResetWeight();
        ResetHealths();
        blueprint.parts.ForEach(p => partsToRepairAdd.Add(p));
        foreach (var part in blueprint.parts)
        {
            if (GetIsDead())
            {
                break;
            }
            partsToRepairAdd.Remove(part);
            if (!parts.Exists(p => p.info.Equals(part)))
            {
                SetUpPart(part);

                UpdateShooterLayering();

                if (this as PlayerCore && HUDScript.instance && HUDScript.instance.abilityHandler)
                {
                    HUDScript.instance.abilityHandler.Deinitialize();
                    HUDScript.instance.abilityHandler.Initialize(PlayerCore.Instance);
                }
                yield return new WaitForSeconds(yardRepairDuration / blueprint.parts.Count);
            }
        }
        if (!GetIsDead()) FinalizeRepair();
    }

    protected virtual void FinalizeRepair()
    {
        if (!isYardRepairing)
            return;
        foreach (var part in partsToRepairAdd)
        {
            if (!parts.Exists(p => p.info.Equals(part)))
            {
                SetUpPart(part);
            }
        }
        UpdateShooterLayering();

        if (this as PlayerCore && HUDScript.instance && HUDScript.instance.abilityHandler)
        {
            HUDScript.instance.abilityHandler.Deinitialize();
            HUDScript.instance.abilityHandler.Initialize(PlayerCore.Instance);
        }
        partsToRepairAdd.Clear();
        maxHealth.CopyTo(baseMaxHealth, 0);
        maxHealth.CopyTo(currentHealth, 0);
        ActivatePassives();
        HealToMax();
        isYardRepairing = false;
    }

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
        if (!SectorManager.instance || SectorManager.instance.current.type != Sector.SectorType.BattleZone)
        {
            return null;
        }

        if ((carrier == null || carrier.Equals(null) || carrier.GetIsDead()) && SectorManager.instance.carriers.ContainsKey(faction))
        {
            carrier = SectorManager.instance.carriers[faction];
            if (carrier == null || carrier.Equals(null) || carrier.GetIsDead())
            {
                carrier = null;
            }
        }

        return carrier;
    }

    public void ResetPower()
    {
        totalPower = 0;
    }

    public float GetPower()
    {
        return totalPower;
    }

    public void AddPower(float power)
    {
        totalPower = Mathf.Min(5000, totalPower + power);
        if (power > 0 && OnPowerCollected != null)
        {
            OnPowerCollected.Invoke(faction, Mathf.RoundToInt(power));
        }
    }

    public void KillShellCore() 
    {
        OnDeath();
    }

    protected override void OnDeath()
    {
        tractor.SetTractorTarget(null);
        StopYardRepairCoroutine();
        base.OnDeath();
    }

    public SectorManager GetSectorManager()
    {
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

        if (!husk)
            InitAI();

        if (FactionManager.DoesFactionGrowRandomParts(faction) && addRandomPartsCoroutine == null)
        {
            addRandomPartsCoroutine = StartCoroutine(AddRandomParts());
        }
    }

    private void InitAI()
    {
        ai = GetAI();
        if (!ai || ai.getMode() != AirCraftAI.AIMode.Inactive) { return; }
        if (sectorMngr.GetCurrentType() == Sector.SectorType.BattleZone)
        {
            ai.setMode(AirCraftAI.AIMode.Battle);
        }
        else
        {
            ai.setMode(AirCraftAI.AIMode.Inactive);
        }

        ai.allowRetreat = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PartyManager.instance?.partyMembers?.Contains(this) == true)
        {
            PartyManager.instance.UnassignBackend(null, this);
        }
    }

    public override void Respawn()
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off || 
            (carrier is Entity entity && !entity.GetIsDead()) || this as PlayerCore || PartyManager.instance.partyMembers.Contains(this))
        {
            isYardRepairing = false;
            base.Respawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string networkPlayerName = "Test name";
    private bool rpcCalled = false;
    protected override void Update()
    {
        base.Update();

        // If got away from Yard while isYardRepairing, FinalizeRepair immediately.
        if (isYardRepairing)
        {
            bool gotAwayFromYard = true;

            foreach (Entity entity in AIData.entities)
            {
                if (!(entity as Yard))
                    continue;
                
                if (!FactionManager.IsAllied(entity.faction, faction))
                    continue;
                
                if ((entity.transform.position - transform.position).sqrMagnitude > Yard.YardProximitySquared)
                    continue;
                
                gotAwayFromYard = false;
            
                break;
            }
        
            if (gotAwayFromYard)
            {
                StopYardRepairCoroutine();
                FinalizeRepair();
            }
        }

        if (MasterNetworkAdapter.mode == NetworkMode.Off) return;
        if (!networkAdapter && !rpcCalled)
        {
            MasterNetworkAdapter.instance.CreateNetworkObjectServerRpc(networkPlayerName, "");
            rpcCalled = true;
        }
        else if (networkAdapter && string.IsNullOrEmpty(networkAdapter.playerName))
        {
            networkAdapter.playerName = networkPlayerName;
        }


    }

    protected override void BuildEntity()
    {
        intrinsicCommandLimit = 0;
        if (!tractor.initialized)
        {
            tractor.BuildTractor();
            switch (CoreUpgraderScript.GetCoreTier(blueprint.coreShellSpriteID))
            {
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
        if (!tractor)
        {
            tractor = gameObject.AddComponent<TractorBeam>();
            tractor.owner = this;
        }

        base.Awake(); // base awake
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

    public bool HasShellOrCoreDamaged()
    {
        // If has damaged shell
        if (GetHealth()[0] < GetMaxHealth()[0])
            return true;

        // if has damaged core
        if (GetHealth()[1] < GetMaxHealth()[1])
            return true;
        
        return false;
    }

    public bool HasPartsDamagedOrDestroyed()
    {
        // Cheks if has damaged parts
        if (parts.Exists(p => p && p.name != "Shell Sprite" && p.IsDamaged()))
            return true;

        // Check if has parts destroyed
        if (blueprint.parts.Exists(p => !parts.Exists(part => part && part.info.Equals(p))))
            return true;

        return false;
    }

}
