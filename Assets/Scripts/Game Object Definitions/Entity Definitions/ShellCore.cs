using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static MasterNetworkAdapter;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft, IHarvester, IOwner
{
    public delegate void PowerCollectDelegate(Entity ent, int amount);

    public static PowerCollectDelegate OnPowerCollected;

    protected ICarrier carrier;
    [SerializeField]
    protected int totalPower;
    protected GameObject bulletPrefab; // prefab for main bullet
    public int intrinsicCommandLimit;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    private TractorBeam tractor;
    public bool isYardRepairing = false;
    private float yardRepairDuration = 1f;

    private List<EntityBlueprint.PartInfo> partsToRepairAdd = new List<EntityBlueprint.PartInfo>();

    private Coroutine yardRepairCoroutine;
    private Coroutine addRandomPartsCoroutine;

    public void StartYardRepairCoroutine()
    {
        if (!initialized) return;
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
                UpdateColliders();

                UpdateShooterLayering();

                if (this as PlayerCore && HUDScript.instance && HUDScript.instance.abilityHandler)
                {
                    HUDScript.instance.abilityHandler.Deinitialize();
                    HUDScript.instance.abilityHandler.Initialize(PlayerCore.Instance);
                }
                yield return new WaitForSeconds(yardRepairDuration / blueprint.parts.Count);
            }
        }
        UpdateColliders();
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
        UpdateColliders();
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

        var facID = FactionManager.GetDistinguishingInteger(faction);
        if ((carrier == null || carrier.Equals(null) || carrier.GetIsDead()) && SectorManager.instance.carriers.ContainsKey(facID))
        {
            carrier = SectorManager.instance.carriers[facID];
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

    public int GetPower()
    {
        return totalPower;
    }

    public void AddPower(int power)
    {
        totalPower = Mathf.Min(5000, totalPower + power);
        if (power > 0 && OnPowerCollected != null)
        {
            OnPowerCollected.Invoke(this, Mathf.RoundToInt(power));
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
        if (ai) ai.OnEntityDeath();
        if (MasterNetworkAdapter.mode != NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide
            && lastDamagedBy is ShellCore core && core.networkAdapter && core.networkAdapter.isPlayer.Value)
        {
            HUDScript.AddScore(core.networkAdapter.playerName, 5);
        }
        
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

        if (FactionManager.DoesFactionGrowRandomParts(faction.factionID) && addRandomPartsCoroutine == null)
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

    public void RemoveAllParts()
    {
        while (parts.Count > 0)
        {
            if (parts[0].name == "Shell Sprite") return;
            RemovePart(parts[0]);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (AIData.shellCores.Contains(this))
        {
            AIData.shellCores.Remove(this);
        }
        if (PartyManager.instance?.partyMembers?.Contains(this) == true)
        {
            PartyManager.instance.UnassignBackend(null, this);
        }
    }

    public override void Respawn(bool force = false)
    {
        if (force || (GetCarrier() != null && !GetCarrier().Equals(null)) || (this as PlayerCore && MasterNetworkAdapter.mode == NetworkMode.Off) || (PartyManager.instance && PartyManager.instance.partyMembers.Contains(this)))
        {
            if (this as PlayerCore) PlayerCore.Instance.enabled = true;
            isYardRepairing = false;
            base.Respawn();
        }
        else if (!(this as PlayerCore))
        {
            if (!MasterNetworkAdapter.lettingServerDecide && networkAdapter) networkAdapter.safeToRespawn.Value = false;
            Destroy(gameObject);
        }
        else if (MasterNetworkAdapter.mode != NetworkMode.Client)
        {
            PlayerCore.Instance.enabled = false;
        }
    }

    public void SyncPower(int power)
    {
        this.totalPower = power;
    }

    protected override void Update()
    {
        base.Update();
        if (!SystemLoader.AllLoaded) return;
        // If got away from Yard while isYardRepairing, FinalizeRepair immediately.
        if (isYardRepairing)
        {
            bool gotAwayFromYard = true;

            foreach (Entity entity in AIData.entities)
            {
                if (!(entity as Yard))
                    continue;
                
                if (!FactionManager.IsAllied(entity.faction.factionID, faction.factionID))
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

        // tick all abilities if a server husk
        if (husk || this as PlayerCore)
        {
            foreach (Ability a in GetAbilities())
            {
                if (a)
                {
                    a.Tick();
                }
            }
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

        if (!AIData.shellCores.Contains(this))
        {
            AIData.shellCores.Add(this);
        }
        base.Awake(); // base awake
    }

    public void SetTractorTarget(Draggable newTarget, bool fromClient = false, bool fromServer = false)
    {
        tractor.SetTractorTarget(newTarget, fromClient, fromServer);
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
        serverSyncHealthDirty = true;
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
