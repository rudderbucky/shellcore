using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// A player ShellCore.
/// </summary>
public class PlayerCore : ShellCore
{
    public HUDScript hud;
    public InfoText alerter;
    public PlayerSave cursave;
    public bool loaded;
    private bool isInteracting;
    protected int credits;
    public int[] abilityCaps;
    public int reputation;
    public static PlayerCore Instance;
    public List<ShellPart> partsToDestroy = new List<ShellPart>();
    public Vector2 havenSpawnPoint;
    private int dimension;

    public int Dimension
    {
        get { return dimension; }
        set { dimension = value; }
    }

    private int lastDimension = 0;

    public int LastDimension
    {
        get { return lastDimension; }
        set { lastDimension = value; }
    }

    // Uses this method to generally add credits for the player.
    public void AddCredits(int amount)
    {
        credits += amount;
        credits = Mathf.Max(0, credits);
        var BZM = SectorManager.instance?.GetComponent<BattleZoneManager>();
        if (BZM != null)
        {
            BZM.CreditsCollected += amount;
        }
    }

    public int GetCredits()
    {
        return credits;
    }

    // Method only used for save loading
    public void SetCredits(int val)
    {
        this.credits = val;
    }

    protected override void FinalizeRepair()
    {
        base.FinalizeRepair();
        foreach (var part in partsToDestroy)
        {
            if (part && part.gameObject)
                Destroy(part.gameObject);
        }

        partsToDestroy.Clear();
    }

    public AbilityHandler GetAbilityHandler()
    {
        return GameObject.Find("AbilityUI").GetComponent<AbilityHandler>();
    }

    // "Interacting" means the player is in a situation where they SHOULDN'T BE MOVING
    public bool GetIsInteracting()
    {
        return isInteracting || DevConsoleScript.componentEnabled;
    }

    public void SetIsInteracting(bool val)
    {
        isInteracting = val;
    }

    public void SetPlayerSpawnPoint()
    {
        carrier = FindCarrier();
        if (carrier != null)
        {
            spawnPoint = carrier.GetSpawnPoint();
        }
        else
        {
            spawnPoint = havenSpawnPoint;
            dimension = lastDimension;
        }
        transform.position = spawnPoint; // reset position to spawn point
    }

    /// <summary>
    /// Respawns the player core, deinitializes the HUD
    /// </summary>
    public override void Respawn(bool force = false)
    {

        List<bool> weaponActivationStates = new List<bool>();
        List<bool> autoCastActivationStates = new List<bool>();
        if (abilities != null)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i] is WeaponAbility weapon)
                {
                    weaponActivationStates.Add(weapon.GetActiveTimeRemaining() == -1);
                }
            }
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i] is ActiveAbility activeAbility)
                {
                    autoCastActivationStates.Add(activeAbility.AutoCast);
                }
            }
        }
        if (hud)
        {
            hud.DeinitializeHUD(); // deinitialize HUD
        }

        SetPlayerSpawnPoint();


        base.Respawn(force); // this will reinitialize the HUD
        spawnPoint = havenSpawnPoint; // reset spawn point
        int weaponIndex = 0;
        if (abilities != null)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i] is WeaponAbility weapon)
                {
                    weapon.SetActive(weaponActivationStates[weaponIndex++]);
                }
            }
            int activeAbilityIndex = 0;
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i] is ActiveAbility activeAbility)
                {
                    activeAbility.AutoCast = autoCastActivationStates[activeAbilityIndex++];
                }
            }
        }
        
    }

    private Vector3? minimapPoint = null;

    public Vector3? GetMinimapPoint()
    {
        return minimapPoint;
    }

    /// <summary>
    /// The directional driver for the player core, returns a vector based on current inputs
    /// </summary>
    /// <returns>a directional vector based on current inputs</returns>
    public Vector2 getDirectionalInput()
    {
        if (Input.GetMouseButton(1) && !(MouseMovementVisualScript.overMinimap && Input.GetMouseButton(0)))
        {
            minimapPoint = null;
            // The angle from center of the screen to the mouse position on screen
            var delta = Input.mousePosition - new Vector3(Screen.width, Screen.height, 0) * 0.5f;
            return delta.normalized;
        }

        if (Input.GetMouseButton(0) && MouseMovementVisualScript.overMinimap && !SelectionBoxScript.GetClicking())
        {
            bool droneInteraction = false;
            var mousePosOnMinimap = MouseMovementVisualScript.GetMousePosOnMinimap();
            minimapPoint = CameraScript.instance.minimapCamera.ScreenToWorldPoint(mousePosOnMinimap);
            foreach (var ent in targeter.GetSecondaryTargets())
            {
                if (ent && ent.transform)
                {
                    droneInteraction = ReticleScript.instance.DroneCheck(ent.transform, null, minimapPoint.Value) || droneInteraction;
                }
            }

            // This orders primary target drones to move/follow accordingly.
            droneInteraction |= ReticleScript.instance.DroneCheck(targeter.GetTarget(), null, minimapPoint.Value);
            if (droneInteraction)
            {
                minimapPoint = null;
            }
            else
            {
                minimapPoint = new Vector3(minimapPoint.Value.x, minimapPoint.Value.y, 0);
                var delta = minimapPoint.Value - transform.position;
                return delta.normalized;
            }

        }

        //Sum up all inputs
        Vector2 direction = Vector2.zero;
        if (InputManager.GetKey(KeyName.Up))
        {
            direction += new Vector2(0, 1);
        }

        if (InputManager.GetKey(KeyName.Left))
        {
            direction += new Vector2(-1, 0);
        }

        if (InputManager.GetKey(KeyName.Down))
        {
            direction += new Vector2(0, -1);
        }

        if (InputManager.GetKey(KeyName.Right))
        {
            direction += new Vector2(1, 0);
        }

        if (minimapPoint != null && direction == Vector2.zero)
        {
            if (Vector3.SqrMagnitude(transform.position - minimapPoint.Value) < PathAI.minDist)
            {
                minimapPoint = null;
                return Vector2.zero;
            }
            else
            {
                return (minimapPoint.Value - transform.position).normalized;
            }
        }
        else
        {
            minimapPoint = null;
        }

        //Send unit vector
        direction.Normalize();
        return direction;
    }

    ICarrier FindCarrier()
    {
        if (SectorManager.instance.GetCurrentType() == Sector.SectorType.BattleZone)
        {
            var targets = BattleZoneManager.getTargets();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] &&
                    !targets[i].GetIsDead() &&
                    targets[i] is ICarrier carrier &&
                    targets[i].faction == faction)
                {
                    return carrier;
                }
            }
        }

        return null;
    }

    protected override void Awake()
    {
        Instance = this;
        name = entityName = "player";
        if (!initialized)
        {
            base.Awake();
        }

        ID = "player";
    }

    public void StartWrapper()
    {
        Start();
    }
    protected override void Start()
    {
        if (!SystemLoader.AllLoaded) return;

        foreach (var part in partsToDestroy)
        {
            if (part && part.gameObject)
                Destroy(part.gameObject);
        }

        partsToDestroy.Clear();

        base.Start();

        if (hud)
        {
            hud.InitializeHUD(this);
        }

        if (!loaded)
        {
            LoadSave(cursave);
            loaded = true;
        }

        // force sectors to load once positioning has been determined
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
        SectorManager.instance.AttemptSectorLoad();

        // the player needs a predictable name for task interactions, so its object will always be called this
        name = entityName = "player";
    }

    public override void Rebuild()
    {
        if (!initialized)
        {
            Awake();
        }

        initialized = true;
        hud.DeinitializeHUD();
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[i].gameObject.name != "Shell Sprite")
            {
                parts[i].GetComponentInChildren<Ability>()?.SetDestroyed(true);
                Destroy(parts[i].gameObject);
            }
        }

        // UnityEditor.AssetDatabase.CreateAsset(blueprint, "Assets/Core Upgrades.asset");
        BuildEntity();
        // the player needs a predictable name for task interactions, so its object will always be called this
        name = entityName = "player";
        hud.InitializeHUD(this);
    }

    public void LoadSave(PlayerSave save)
    {
        if (save.timePlayed != 0)
        {
            if (save.characters != null && save.characters.Length != 0)
            {
                // use the save's characters combined with any new characters in the Sector Manager character set
                var sectoManagerChars = new List<WorldData.CharacterData>(SectorManager.instance.characters);
                var newChars = new List<WorldData.CharacterData>(save.characters);
                foreach (var ch in sectoManagerChars)
                {
                    if (newChars.TrueForAll(c => c.ID != ch.ID))
                    {
                        newChars.Add(ch);
                    }
                }

                SectorManager.instance.characters = newChars.ToArray();
            }

            if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
                transform.position = havenSpawnPoint = save.position;
        }

        name = entityName = "player";
        positionBeforeOscillation = transform.position.y;
    }

    public List<EntityBlueprint.PartInfo> GetInventory()
    {
        if (cursave != null)
        {
            return cursave.partInventory;
        }
        else
        {
            return null;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!SystemLoader.AllLoaded || (NetworkManager.Singleton && NetworkManager.Singleton.IsListening && !blueprint)) return;
        // call methods
        if (group.sortingOrder < maxAirLayer) // player must always be above other entities
        {
            group.sortingOrder = ++maxAirLayer;
        }

        base.Update(); // base update
        if (!GetIsInteracting() && !DialogueSystem.isInCutscene)
        {
            if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !NetworkManager.Singleton.IsServer && networkAdapter != null && !dirty)
            {
                networkAdapter.ChangeDirectionServerRpc(getDirectionalInput());
                dirty = true;
            }

            if (!MasterNetworkAdapter.lettingServerDecide)
                MoveCraft(getDirectionalInput()); // move the craft based on the directional input
            if (networkAdapter) networkAdapter.wrapper.directionalVector = getDirectionalInput();
        
        }
    }

    public override void Warp(Vector3 point)
    {
        base.Warp(point);
        CameraScript.instance.Focus(transform.position);
        foreach (var instance in RectangleEffectScript.instances)
        {
            instance.Start();
        }

        instantiatedRespawnPrefab = Instantiate(respawnImplosionPrefab).transform;
        instantiatedRespawnPrefab.position = transform.position;
        AudioManager.PlayClipByID("clip_respawn", transform.position);
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
            SectorManager.instance.AttemptSectorLoad();
    }

    protected override void CraftMover(Vector2 directionVector)
    {
        base.CraftMover(directionVector);

        if (directionVector != Vector2.zero && (!MasterNetworkAdapter.lettingServerDecide))
        {
            CameraScript.instance.Focus(transform.position);
        }
    }

    public override float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy)
    {
        var residue = base.TakeShellDamage(amount, shellPiercingFactor, lastDamagedBy);
        if (lastDamagedBy)
        {
            HealthBarScript.instance.StartHurtHud(FactionManager.GetFactionColor(lastDamagedBy.faction));
        }

        return residue;
    }

    public static Color GetPlayerFactionColor()
    {
        return PlayerCore.Instance ? FactionManager.GetFactionColor(PlayerCore.Instance.GetFaction()) : FactionManager.GetFactionColor(0);
    }

    public int GetBuildValue()
    {
        var value = 0;
        foreach (var part in blueprint.parts)
        {
            value += EntityBlueprint.GetPartValue(part);
        }

        return value;
    }
}
