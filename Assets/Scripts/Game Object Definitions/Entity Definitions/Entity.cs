using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using static MasterNetworkAdapter;

/// <summary>
/// The base class of every "being" in the game.
/// </summary>
public class Entity : MonoBehaviour, IDamageable, IInteractable
{
    public delegate void EntitySpawnDelegate(Entity entity);

    public static EntitySpawnDelegate OnEntitySpawn;

    public delegate void EntityDeathDelegate(Entity entity, Entity murderer);

    public static EntityDeathDelegate OnEntityDeath;

    public ShellPart shell;
    protected static int maxAirLayer = 1; // the maximum sorting group layer of all entities
    protected static int maxGroundLayer = 1;
    protected SortingGroup group;
    protected float[] maxHealth; // maximum health of the entity (index 0 is shell, index 1 is core, index 2 is energy)

    [SerializeField]
    protected float[] regenRate; // regeneration rate of the entity (index 0 is shell, index 1 is core, index 2 is energy)

    protected List<Ability> abilities; // abilities
    public Rigidbody2D entityBody; // entity to modify with this script
    protected Collider2D hitbox; // the hitbox of the entity (excluding extra parts)
    protected TargetingSystem targeter; // the TargetingSystem of the entity
    protected ExtendedTargetingSystem extendedTargeter;
    protected bool isInCombat; // whether the entity is in combat or not
    protected bool isBusy; // whether the entity is busy or not
    [SerializeField]
    protected bool isDead; // whether the entity is currently dead or not
    protected bool isWarpUninteractable; // whether the entity is uninteractable because it recently warped
    protected float busyTimer; // the time since the entity was last set to busy
    protected float combatTimer; // the time since the entity was last set into combat
    protected float deathTimer; // the time since the entity last died;
    protected float warpUninteractableTimer; // the time since the entity last warped;
    protected GameObject explosionCirclePrefab; // prefabs for death explosion
    protected GameObject explosionLinePrefab;
    protected GameObject respawnImplosionPrefab;
    protected GameObject deathExplosionPrefab;
    protected List<ShellPart> parts; // List containing all parts of the entity
    protected float[] currentHealth; // current health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    public bool serverSyncHealthDirty = true;

    public bool husk;
    [SerializeField]
    public float[] CurrentHealth
    {
        get { return (float[])currentHealth.Clone(); }
        set
        {
            currentHealth = value;
            for (int i = 0; i < currentHealth.Length; i++)
            {
                currentHealth[i] = Mathf.Max(0, currentHealth[i]);
            }
        }
    }

    public int faction; // What side the entity belongs to (0 = green, 1 = red, 2 = olive...)
    public EntityBlueprint blueprint; // blueprint of entity containing parts
    public Vector3 spawnPoint;
    public Dialogue dialogue; // dialogue of entity
    protected Draggable draggable; // associated draggable
    protected bool initialized; // is the entity safe to call update() on?
    public EntityCategory category = EntityCategory.Unset; // these two fields will be changed via hardcoding in child class files
    public string ID; // used in tasks
    protected float[] baseMaxHealth = new float[3];
    private int controlStacks;
    private float TimeToDeath
    {
        get 
        { 
            var tier = 0;
            if (blueprint && MasterNetworkAdapter.mode != NetworkMode.Off) 
                tier = CoreUpgraderScript.GetCoreTier(blueprint.coreShellSpriteID);
            return RESPAWN_TIMES[tier]; 
        }
    }

    private static float[] RESPAWN_TIMES = new float[] {5, 7, 9, 11};

    public void CancelDeath()
    {
        deathTimer = TimeToDeath;
        isDead = false;
    }
    public void HealToMax()
    {
        CurrentHealth = GetMaxHealth().Clone() as float[];
    }

    public int ControlStacks
    {
        get { return controlStacks; }
        set
        {
            controlStacks = value;
            CalculateMaxHealth();
            CalculateDamageBoost();
        }
    }

    private int[] passiveMaxStacks = new int[3];
    public int[] PassiveMaxStacks
    {
        get { return (int[])passiveMaxStacks.Clone(); }
        set
        {
            passiveMaxStacks = value;
            CalculateMaxHealth();
        }
    }
    private int damageBoostStacks;
    public int DamageBoostStacks
    {
        get { return damageBoostStacks; }
        set
        {
            damageBoostStacks = value;
            CalculateDamageBoost();
        }
    }

    public string blueprintString  {set; private get;}

    public delegate void EntityRangeCheckDelegate(float range);
    public EntityRangeCheckDelegate RangeCheckDelegate;
    private bool rpcCalled = false;
    public void AttemptCreateNetworkObject(bool isPlayer)
    {
        if (!networkAdapter && !rpcCalled) // should only happen to players, drones, towers, tanks, or turrets
        {
            string idToGrab = null;
            if (this as Drone || this as Tower || this as Tank || this as Turret)
            {
                if (string.IsNullOrEmpty(ID)) ID = SectorManager.instance.GetFreeEntityID();
                idToGrab = ID;
            }

            if (MasterNetworkAdapter.mode != NetworkMode.Server && this as PlayerCore)
            {
                MasterNetworkAdapter.instance.CreatePlayerServerRpc(MasterNetworkAdapter.playerName, SectorManager.GetNetworkSafeBlueprintString(MasterNetworkAdapter.blueprint), faction);
            }
            else if (MasterNetworkAdapter.mode != NetworkMode.Client)
            {
                MasterNetworkAdapter.instance.CreateNetworkObjectWrapper(MasterNetworkAdapter.playerName, blueprintString, idToGrab, false, faction, Vector3.zero);
            }
            rpcCalled = true;
        }
        else if (networkAdapter && string.IsNullOrEmpty(networkAdapter.playerName))
        {
            networkAdapter.playerName = MasterNetworkAdapter.playerName;
        }
    }


    private void UpdateRenderer(Renderer renderer)
    {
        var finalAlpha = IsInvisible ? FactionManager.IsAllied(PlayerCore.Instance ? PlayerCore.Instance.faction : 0, faction) ? 0.2f : 0f : FactionManager.GetFactionColor(faction).a;
        if (renderer is SpriteRenderer spriteRenderer)
        {
            var c = spriteRenderer.color;
            c.a = finalAlpha;
            spriteRenderer.color = c;
        }
        if (renderer is LineRenderer lineRenderer)
        {
            var sc = lineRenderer.startColor;
            var ec = lineRenderer.endColor;
            sc.a = finalAlpha;
            ec.a = finalAlpha;
            lineRenderer.startColor = sc;
            lineRenderer.endColor = ec;
            var anim = renderer.GetComponentInChildren<MissileAnimationScript>();
            if (anim)
            {
                var ac = anim.lineColor;
                ac.a = finalAlpha;
                anim.lineColor = ac;
            }
        }
    }

    private void UpdateInvisibleGraphics()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            UpdateRenderer(renderers[i]);
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = !IsInvisible;
        }
    }

    public int StealthStacks
    {
        get { return stealths; }
        set
        {
            stealths = value;
            UpdateInvisibleGraphics();
        }
    }

    [SerializeField]
    private int healAuraStacks;

    public int HealAuraStacks
    {
        get { return healAuraStacks; }
        set
        {
            healAuraStacks = value;
        }
    }

    [SerializeField]
    private int speedAuraStacks;

    public int SpeedAuraStacks
    {
        get { return speedAuraStacks; }
        set
        {
            speedAuraStacks = value;
        }
    }


    [SerializeField]
    private int energyAuraStacks;

    public int EnergyAuraStacks
    {
        get { return energyAuraStacks; }
        set
        {
            energyAuraStacks = value;
        }
    }





    // Performs calculations based on current control and shell max stats to determine final health
    private void CalculateMaxHealth()
    {
        var fracs = new float[3] { currentHealth[0] / maxHealth[0], currentHealth[1] / maxHealth[1], currentHealth[2] / maxHealth[2] };
        maxHealth[0] = (baseMaxHealth[0] + passiveMaxStacks[0] * ShellMax.maxes[0]) * (1 + controlStacks * Control.baseControlFractionBoost);
        maxHealth[1] = (baseMaxHealth[1] + passiveMaxStacks[1] * ShellMax.maxes[1]);
        maxHealth[2] = (baseMaxHealth[2] + passiveMaxStacks[2] * ShellMax.maxes[2]);
        if (DevConsoleScript.godModeEnabled && this as PlayerCore) maxHealth = new float[] { 99999, 99999, 99999 };

        for (int i = 0; i < 3; i++)
        {
            fracs[i] *= maxHealth[i];
        }
        CurrentHealth = fracs;
    }

    // Performs calculations based on current damage boost and control stats to determine final damage addition
    private void CalculateDamageBoost()
    {
        damageFactor = controlStacks * Control.damageFactor + damageBoostStacks * DamageBoost.damageFactor;
    }

    private int stealths = 0;
    private bool invisible = false;

    public bool IsInvisible
    {
        get { return StealthStacks > 0 || invisible; }
        set
        {
            invisible = value;
            UpdateInvisibleGraphics();
        }
    }

    private float damageFactor = 0f;
    public float GetDamageFactor()
    {
        return damageFactor;
    }

    [HideInInspector]
    public int absorptions = 0;

    public bool isAbsorbing // if true, all incoming damage is converted to energy
    {
        get { return absorptions > 0; }
    }

    bool collidersEnabled = true;
    public bool tractorSwitched = false;

    public SectorManager sectorMngr;
    protected Entity lastDamagedBy;

    public string entityName;

    private float weaponGCD = 0.1F; // weapon global cooldown
    public float WeaponGCD {
        get { return weaponGCD; }
        set { weaponGCD = value; }
    }
    private float weaponGCDTimer;

    public float GetWeaponGCDTimer()
    {
        return weaponGCDTimer;
    }

    public void SetWeaponGCDTimer(float timer)
    {
        weaponGCDTimer = timer;
    }

    public float weight;

    public static readonly float weightMultiplier = 25;
    public static readonly float coreWeight = 40;
    private int sortingOrder;

    // terrain type of entity
    // binary flag
    public enum TerrainType
    {
        Unset,
        Ground,
        Air,
        All
    }

    public enum EntityCategory // category of entity (carriers, outposts and bunkers are stations, everything else are units)
    {
        Station,
        Unit,
        All,
        Unset
    }

    TerrainType terrain = TerrainType.Unset;

    public TerrainType Terrain
    {
        get { return terrain; }
        protected set { terrain = value; }
    }

    // boolean used to check if proximity and reticle interactions should trigger for this entity
    private bool interactible = false;

    // prevents interaction while entities are in paths
    public bool isPathing = false;
    public static readonly float DefaultPartRate = 0.1f;
    public static float partDropRate = DefaultPartRate;
    public void SetWeaponGCD(float gcd)
    {
        weaponGCD = gcd;
    }

    // Code run on reticle double-click/proximity hotkey press
    public void Interact()
    {
        if (TaskManager.interactionOverrides.ContainsKey(ID) && TaskManager.interactionOverrides[ID].Count > 0)
        {
            TaskManager.interactionOverrides[ID].Peek().action.Invoke();
        }
        else if (DialogueSystem.interactionOverrides.ContainsKey(ID) && DialogueSystem.interactionOverrides[ID].Count > 0)
        {
            DialogueSystem.interactionOverrides[ID].Peek().action.Invoke();
        }
        else
        {
            DialogueSystem.StartDialogue(dialogue, this);
        }
    }

    public void UpdateInteractible()
    {
        if (!SectorManager.instance.current) return;
        interactible = GetDialogue() && PlayerCore.Instance && FactionManager.IsAllied(PlayerCore.Instance.faction, faction);

        // These are implications, not a biconditional; interactibility is not necessarily true/false if there are no
        // task overrides or pathing set up. Hence the if statements are needed here
        if (ID != null && TaskManager.interactionOverrides.ContainsKey(ID)
                       && TaskManager.interactionOverrides[ID].Count > 0)
        {
            while (TaskManager.interactionOverrides[ID].Count > 0)
            {
                InteractAction action = TaskManager.interactionOverrides[ID].Peek();
                if ((action.traverser as MissionTraverser).taskHash == action.taskHash)
                {
                    interactible = true;
                    break;
                }
                else
                {
                    TaskManager.interactionOverrides[ID].Pop();
                }
            }
        }

        if (ID != null && DialogueSystem.interactionOverrides.ContainsKey(ID)
                       && DialogueSystem.interactionOverrides[ID].Count > 0)
        {
            interactible = true;
        }

        if (isPathing || DialogueSystem.isInCutscene)
        {
            interactible = false;
        }

        if (this.isWarpUninteractable)
        {
            interactible = false;
        }

        if (this as ShellCore && SectorManager.instance.current.type == Sector.SectorType.BattleZone)
        {
            interactible = false;
        }

        if (isDead)
        {
            interactible = false;
        }
    }

    public bool GetInteractible()
    {
        return interactible;
    }

    Dictionary<int, bool> weaponActivationStates = new Dictionary<int, bool>();

    public void RememberWeaponActivationStates()
    {
        RememberWeaponActivationStates(weaponActivationStates);
        if (networkAdapter && networkAdapter.weaponActivationStates != null)
        {
            RememberWeaponActivationStates(networkAdapter.weaponActivationStates);
        }
    }

    protected void RememberWeaponActivationStates(Dictionary<int, bool> weaponActivationStates)
    {
        weaponActivationStates.Clear();
        if (abilities == null) return;
        foreach (var ability in abilities)
        {
            if (!ability) continue;
            if (!(ability is WeaponAbility weapon)) continue;
            if (weaponActivationStates.ContainsKey(weapon.GetID()))
            {
                continue;
            }
            weaponActivationStates.Add(weapon.GetID(), weapon.isEnabled);
        }
    }

    protected void AttemptAddComponents()
    {
        if (!GetComponent<SortingGroup>())
        {
            group = gameObject.AddComponent<SortingGroup>();
            if (this as AirCraft || this as AirConstruct)
            {
                group.sortingLayerName = "Air Entities";
                group.sortingOrder = ++maxAirLayer;
            }
            else
            {
                group.sortingLayerName = "Ground Entities";
                group.sortingOrder = ++maxGroundLayer;
            }
        }

        if (!transform.Find("Shell Sprite")) // no shell in hierarchy yet? no problem
        {
            GameObject childObject = new GameObject("Shell Sprite"); // create the child gameobject
            childObject.transform.SetParent(transform, false); // set to child
            PolygonCollider2D collider = childObject.AddComponent<PolygonCollider2D>(); // add collider
            collider.isTrigger = true; // do not allow "actual" collisions
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>(); // add renderer
            if (blueprint)
            {
                // check if it contains a blueprint (it should)
                renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
            }
            else
            {
                renderer.sprite = ResourceManager.GetAsset<Sprite>("core1_shell"); // set to default shellcore sprite
            }

            ShellPart part = childObject.AddComponent<ShellPart>();
            part.detachible = false;
            shell = part;
            renderer.sortingLayerName = "Default";
            renderer.color = FactionManager.GetFactionColor(faction); // needed to reset outpost colors
        }
        else
        {
            var renderer = transform.Find("Shell Sprite").GetComponent<SpriteRenderer>();
            transform.Find("Shell Sprite").GetComponent<ShellPart>().SetFaction(faction);
            renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
            renderer.color = FactionManager.GetFactionColor(faction); // needed to reset outpost colors
            renderer.sortingLayerName = "Default";
        }

        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            explosionCirclePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionCirclePrefab.AddComponent<DrawCircleScript>().SetStartColor(FactionManager.GetFactionColor(faction));
            explosionCirclePrefab.SetActive(false);
        }

        if (!explosionLinePrefab)
        {
            explosionLinePrefab = new GameObject("Explosion Line");
            explosionLinePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionLinePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionLinePrefab.AddComponent<DrawLineScript>().SetStartColor(FactionManager.GetFactionColor(faction));
            explosionLinePrefab.SetActive(false);
        }

        if (!deathExplosionPrefab)
        {
            deathExplosionPrefab = ResourceManager.GetAsset<GameObject>("death_explosion");
        }

        if (!respawnImplosionPrefab)
        {
            respawnImplosionPrefab = ResourceManager.GetAsset<GameObject>("respawn_implosion");
        }

        if (!GetComponent<SpriteRenderer>())
        {
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.material = ResourceManager.GetAsset<Material>("material_color_swap");
            renderer.color = FactionManager.GetFactionColor(faction);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = FactionManager.GetFactionColor(faction); // needed to reset outpost colors
        }

        if (!GetComponent<Rigidbody2D>())
        {
            entityBody = gameObject.AddComponent<Rigidbody2D>();
            entityBody.gravityScale = 0;
            entityBody.drag = 0;
            entityBody.angularDrag = 100;
        }

        if (!transform.Find("Minimap Image"))
        {
            GameObject childObject = new GameObject("Minimap Image");
            childObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            childObject.AddComponent<MinimapLockRotationScript>();
        }

        if (!GetComponent<Draggable>())
        {
            draggable = gameObject.AddComponent<Draggable>();
        }
        else if (GetComponent<Draggable>() && !draggable)
        {
            Debug.LogWarning("Draggable was added to an entity manually, " +
                             "it should be added automatically by setting isDraggable to true!");
        }
    }

    /// <summary>
    /// Reconstruct the entity, public method for use via node traversal
    /// </summary>
    public virtual void Rebuild()
    {
        if (!initialized)
        {
            Awake();
        }

        initialized = true;

        // destroy existing parts except the shell and rebuild
        for (int i = 0; i < parts.Count; i++)
        {
            if (parts[i] && parts[i].gameObject.name != "Shell Sprite")
            {
                Destroy(parts[i].gameObject);
            }
        }

        stealths = 0;
        absorptions = 0;

        BuildEntity();
        GetComponentInChildren<MinimapLockRotationScript>().Initialize(); // initialize the minimap dot
    }

    // guarantees abilities deactivate on stack frame
    protected void DestroyOldParts()
    {
        // Remove possible old parts from list
        foreach (var part in parts)
        {
            if (part && part.gameObject && part.gameObject.name != "Shell Sprite")
            {
                part.GetComponentInChildren<Ability>()?.SetDestroyed(true);
                Destroy(part.gameObject);
            }
        }
    }

    /// <summary>
    /// Generate shell parts in the blueprint, change ship stats accordingly
    /// </summary>
    protected virtual void BuildEntity()
    {
        // all created entities should have blueprints!
        if (!blueprint)
        {
            Debug.LogError(this + " does not have a blueprint! EVERY constructed entity should have one!");
        }

        DestroyOldParts();

        parts.Clear();

        if (blueprint)
        {
            this.dialogue = blueprint.dialogue;
        }

        AttemptAddComponents();
        var coreRenderer = GetComponent<SpriteRenderer>();
        if (blueprint)
        {
            // check if it contains a blueprint (it should)

            if (blueprint.coreSpriteID == "" && blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore)
            {
                Debug.Log(this + "'s blueprint does not contain a core sprite ID!");
                // check if the blueprint does not contain a core sprite ID (it should) 
            }

            coreRenderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        }
        else
        {
            coreRenderer.sprite = ResourceManager.GetAsset<Sprite>("core1_light");
        }

        var renderer = transform.Find("Minimap Image").GetComponent<SpriteRenderer>();
        if (category == EntityCategory.Station && !(this is Turret))
        {
            if (this as Outpost)
            {
                renderer.sprite = ResourceManager.GetAsset<Sprite>("outpost_minimap_sprite");
            }
            else if (this as Bunker)
            {
                renderer.sprite = ResourceManager.GetAsset<Sprite>("bunker_minimap_sprite");
            }
            else
            {
                renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
            }

            renderer.transform.localScale = new Vector3(3.5F, 3.5F, 3.5F);
        }
        else
        {
            renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
        }


        abilities = new List<Ability>();

        entityName = blueprint.entityName;
        name = blueprint.entityName;
        GetComponent<Rigidbody2D>().mass = 1; // reset mass

        var drone = this as Drone;
        ResetWeight();

        sortingOrder = 1;
        //For shellcores, create the tractor beam
        // Create shell parts
        SetUpParts(blueprint);
        
        // Drone shell and core health penalty
        if (drone)
        {
            maxHealth[0] /= 2;
            maxHealth[1] /= 4;
        }

        maxHealth.CopyTo(baseMaxHealth, 0);

        var shellRenderer = transform.Find("Shell Sprite").GetComponent<SpriteRenderer>();
        if (shellRenderer)
            shellRenderer.sortingOrder = ++sortingOrder;
        coreRenderer.sortingOrder = ++sortingOrder;
        UpdateShooterLayering();

        if (this as ShellCore)
        {
            if (!gameObject.GetComponentInChildren<MainBullet>())
            {
                MainBullet mainBullet = gameObject.AddComponent<MainBullet>();
                mainBullet.SetTier(Mathf.Min(3, 1 + CoreUpgraderScript.GetCoreTier(blueprint.coreShellSpriteID)));
                mainBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
                mainBullet.terrain = TerrainType.Air;
                mainBullet.SetActive(true);
                abilities.Insert(0, mainBullet);
            }
            else
            {
                MainBullet mainBullet = gameObject.GetComponentInChildren<MainBullet>();
                mainBullet.SetTier(Mathf.Min(3, 1 + CoreUpgraderScript.GetCoreTier(blueprint.coreShellSpriteID)));
                mainBullet.SetDestroyed(false);
                abilities.Insert(0, mainBullet);
            }

            ReflectAbilityActivation(gameObject.GetComponentInChildren<MainBullet>());
        }

        // unique abilities for mini drones here
        if (drone)
        {
            switch (drone.type)
            {
                case DroneType.Mini:
                    var shellObj = transform.Find("Shell Sprite").gameObject;
                    Ability ab = AbilityUtilities.AddAbilityToGameObjectByID(shellObj, 6, null, 1);
                    var shooter = new GameObject("Shooter");
                    shooter.transform.SetParent(shellObj.transform);
                    shooter.transform.localPosition = Vector3.zero;
                    var shooterSprite = shooter.AddComponent<SpriteRenderer>();
                    shooterSprite.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(6));
                    shooterSprite.sortingOrder = ++sortingOrder;
                    shellObj.GetComponent<ShellPart>().shooter = shooter;
                    shellObj.GetComponent<ShellPart>().weapon = ab as WeaponAbility;
                    (ab as WeaponAbility).terrain = TerrainType.Air;
                    abilities.Insert(0, ab);
                    break;
                default:
                    break;
            }
        }

        IsInvisible = false;

        // check to see if the entity is interactible
        if (dialogue && FactionManager.IsAllied(0, faction))
        {
            interactible = true;
        }

        Transform shellSprite = shell.transform;
        if (shellSprite)
        {
            parts.Add(shellSprite.GetComponent<ShellPart>());
        }

        ConnectedTreeCreator();

        maxHealth.CopyTo(currentHealth, 0);
        ActivatePassives(); // activate passive abilities here to avoid race condition BS

        if (OnEntitySpawn != null)
        {
            OnEntitySpawn.Invoke(this);
        }
    }

    // wrapper for weight; hard-sets weight to 25 for drones
    protected void ResetWeight()
    {
        var drone = this as Drone;
        weight = drone ? 25 : coreWeight;
    }

    // Wrapper for assembling core
    protected void SetUpParts(EntityBlueprint blueprint)
    {   
        if (blueprint != null && blueprint.parts != null)
        {
            ResetHealths();
            for (int i = 0; i < blueprint.parts.Count; i++)
            {
                SetUpPart(blueprint.parts[i]);
            }
        }
    }

    public void AttachRandomPart() 
    {
        EntityBlueprint.PartInfo info = ResourceManager.Instance.GetRandomPart();
        ShellPart part = parts[Random.Range(0, parts.Count)];
        info.location = part.info.location;
        info.location += new Vector2(Random.Range(-0.1F, 0.1F), Random.Range(-0.1F, 0.1F));
        SetUpPart(info);
    }



    protected IEnumerator AddRandomParts()
    {
        while (true) 
        {
            while (GetIsDead()) yield return null;
            AttachRandomPart();
            yield return new WaitForSeconds(2F);
        }
    }

    protected void ResetHealths()
    {
        blueprint.shellHealth.CopyTo(maxHealth, 0);
        blueprint.baseRegen.CopyTo(regenRate, 0);
    }

    private void ReflectAbilityActivation (WeaponAbility ab)
    {
        if (weaponActivationStates.ContainsKey(ab.GetID()))
        {
            ab.isEnabled = weaponActivationStates[ab.GetID()];
        }

        if (networkAdapter && networkAdapter.weaponActivationStates.ContainsKey(ab.GetID()))
        {
            ab.isEnabled = networkAdapter.weaponActivationStates[ab.GetID()];
        }
    }

    protected ShellPart SetUpPart(EntityBlueprint.PartInfo part)
    {
        var drone = this as Drone;
        var isLightDrone = drone && drone.type == DroneType.Light; // used for light drone weight reduction
        PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(part.partID);

        GameObject partObject = ShellPart.BuildPart(partBlueprint);
        ShellPart shellPart = partObject.GetComponent<ShellPart>();
        shellPart.info = part;
        partObject.transform.SetParent(transform, false);
        partObject.transform.SetAsFirstSibling();

        //Add an ability to the part:

        WeaponAbility ab = AbilityUtilities.AddAbilityToGameObjectByID(partObject, part.abilityID, part.secondaryData, part.tier) as WeaponAbility;
        if (ab)
        {
            // add weapon diversity
            ab.type = DroneUtilities.GetDiversityTypeByEntity(this);
            ReflectAbilityActivation(ab);
        }

        partObject.transform.localEulerAngles = new Vector3(0, 0, part.rotation);
        partObject.transform.localPosition = new Vector3(part.location.x, part.location.y, 0);
        SpriteRenderer sr = partObject.GetComponent<SpriteRenderer>();
        var tmp = partObject.transform.localScale;
        tmp.x = part.mirrored ? -1 : 1;
        partObject.transform.localScale = tmp;
        sr.sortingOrder = ++sortingOrder;
        var partWeight = isLightDrone ? partBlueprint.mass * 0.6F * weightMultiplier : partBlueprint.mass * weightMultiplier;
        weight += partWeight;
        maxHealth[0] += partBlueprint.health / 2;
        maxHealth[1] += partBlueprint.health / 4;

        string shooterID = AbilityUtilities.GetShooterByID(part.abilityID, part.secondaryData);
        // Add shooter
        if (shooterID != null)
        {
            var shooter = new GameObject("Shooter");
            shooter.transform.SetParent(partObject.transform);
            shooter.transform.localPosition = Vector3.zero;
            shooter.transform.localRotation = Quaternion.identity;
            var shooterSprite = shooter.AddComponent<SpriteRenderer>();
            shooterSprite.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
            shellPart.shooter = shooter;
            if (AbilityUtilities.GetAbilityTypeByID(part.abilityID) == AbilityHandler.AbilityTypes.Weapons)
            {
                shellPart.weapon = true;
            }
        }

        var weaponAbility = partObject.GetComponent<WeaponAbility>();
        if (weaponAbility)
        {
            // if the terrain and category wasn't preset set to the enitity's properties

            if (weaponAbility.terrain == TerrainType.Unset)
            {
                weaponAbility.terrain = Terrain;
            }

            if (weaponAbility.category == EntityCategory.Unset)
            {
                weaponAbility.category = category;
            }
        }

        var shellRenderer = transform.Find("Shell Sprite").GetComponent<SpriteRenderer>();
        if (shellRenderer)
            shellRenderer.sortingOrder = ++sortingOrder;

        var coreRenderer = GetComponent<SpriteRenderer>();
        if (coreRenderer) coreRenderer.sortingOrder = ++sortingOrder;

        parts.Add(shellPart);
        if (partObject.GetComponent<Ability>())
        {
            abilities.Insert(0, partObject.GetComponent<Ability>());
        }

        // Disable collider if no sprite
        if (!(partObject.GetComponent<SpriteRenderer>() && partObject.GetComponent<SpriteRenderer>().sprite)
            && partObject.GetComponent<Collider2D>() && !partObject.GetComponent<Harvester>())
        {
            partObject.GetComponent<Collider2D>().enabled = false;
        }

        return shellPart;
    }


    // adjust all shooter sprites to be higher than the shell
    protected void UpdateShooterLayering()
    {
        // adjust all shooter sprites to be higher than the shell
        var shellRenderer = transform.Find("Shell Sprite").GetComponent<SpriteRenderer>();
        if (shellRenderer)
        {
            parts.ForEach(p =>
            {
                var spriteRenderer = p?.transform.Find("Shooter")?.GetComponent<SpriteRenderer>();
                if (spriteRenderer)
                {
                    spriteRenderer.sortingOrder = shellRenderer.sortingOrder + 1;
                }
            });
        }
    }

    public bool GetIsDead()
    {
        return isDead; // is dead
    }

    /// <summary>
    /// Helper method for death animation and state changing
    /// </summary>
    protected virtual void OnDeath()
    {
        // set death, interactibility and immobility
        IsInvisible = false;
        serverSyncHealthDirty = true;
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }

        RememberWeaponActivationStates();
        foreach (var ability in abilities)
        {
            if (ability)
            {
                ability.SetDestroyed(true);
            }
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton.IsServer && networkAdapter)
        {
            networkAdapter.serverReady.Value = false;
        }
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && MasterNetworkAdapter.lettingServerDecide && networkAdapter)
        {
            networkAdapter.clientReady = false;
        }

        interactible = false;
        isDead = true;
        SetIntoCombat();
        deathTimer = 0; // reset death timer
        transform.Find("Minimap Image").GetComponent<SpriteRenderer>().enabled = false; // remove from minimap

        AudioManager.PlayClipByID("clip_explosion1", transform.position);

        // Roll on each part
        if (!FactionManager.IsAllied(0, faction) && !(this as PlayerCore) && this is ShellCore shellCore &&
            shellCore.GetCarrier() == null)
        {
            // extract non-shell parts
            var selectedParts = parts.FindAll(p => p != shell);
            if (selectedParts.Count > 0  && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
            {
                foreach (var part in selectedParts)
                {
                    if (Random.value < partDropRate)
                    {
                        part.SetCollectible(true);
                        if (sectorMngr)
                        {
                            AIData.strayParts.Add(part);
                        }
                    }
                }
            }
        }

        DetachAllParts();
        entityBody.velocity = Vector2.zero;

        var BZM = SectorManager.instance?.GetComponent<BattleZoneManager>();

        if (lastDamagedBy is PlayerCore player)
        {
            player.AddCredits(Random.Range(1, 5));

            if (this as ShellCore && !FactionManager.IsAllied(0, faction))
            {
                foreach (var part in blueprint.parts)
                {
                    player.cursave.partsSeen.Add(PartIndexScript.CullToPartIndexValues(part));
                }
            }
        }

        if (OnEntityDeath != null)
        {
            OnEntityDeath.Invoke(this, lastDamagedBy);
        }

        if (BZM != null && BZM.IsTarget(this))
        {
            BZM.UpdateCounters();
        }

        GameObject deathExplosion = Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
    }

    protected void DetachAllParts()
    {
        for (int i = 0; i < parts.Count; i++)
        {
            parts[i].Detach();
        }
    }

    protected virtual void PostDeath()
    {
        Destroy(gameObject);
    }

    virtual protected void Awake()
    {
        // initialize instance fields
        currentHealth = new float[3];
        maxHealth = new float[3];
        regenRate = new float[3];
        parts = new List<ShellPart>();
        isInCombat = false;

        AttemptAddComponents();

        if (!AIData.entities.Contains(this))
        {
            AIData.entities.Add(this);
        }

        if (!AIData.interactables.Contains(this))
        {
            AIData.interactables.Add(this);
        }

        if (this is IVendor vendor)
        {
            AIData.vendors.Add(vendor);
        }
    }

    protected virtual void OnDestroy()
    {
        if (AIData.entities.Contains(this))
        {
            AIData.entities.Remove(this);
        }

        if (AIData.interactables.Contains(this))
        {
            AIData.interactables.Remove(this);
        }

        if (this is IVendor vendor)
        {
            AIData.vendors.Remove(vendor);
        }

        if (SectorManager.instance)
            SectorManager.instance.RemoveObject(ID, gameObject);

        RememberWeaponActivationStates();
        if (MasterNetworkAdapter.mode != NetworkMode.Client && networkAdapter && !networkAdapter.isPlayer.Value)
        {
            if(networkAdapter.GetComponent<NetworkObject>().IsSpawned)
                networkAdapter.GetComponent<NetworkObject>().Despawn();
            Destroy(networkAdapter.gameObject);
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
        {
            if (networkAdapter) networkAdapter.playerNameAdded = false;
            ProximityInteractScript.instance.RemovePlayerName(this);
        }
        
    }


    virtual protected void Start()
    {
        BuildEntity(); // Generate shell parts around the entity
        transform.position = spawnPoint;
        GetComponentInChildren<MinimapLockRotationScript>().Initialize(); // initialize the minimap dot
        targeter = new TargetingSystem(transform); // create the associated targeting system for this craft
        extendedTargeter = new ExtendedTargetingSystem(transform);
        targeter.SetTarget(null);
        //transform.rotation = Quaternion.identity; // reset rotation
        GetComponent<SpriteRenderer>().enabled = true; // enable sprite renderer
        busyTimer = 0; // reset busy timer
        if (SectorManager.instance && SectorManager.instance.current &&
        SectorManager.instance.current.type == Sector.SectorType.BattleZone && ((new List<string>(SectorManager.instance.current.targets)).Contains(ID) || (networkAdapter != null && networkAdapter.isPlayer.Value)) )
        {
            SectorManager.instance.AddTarget(this);
        }
        initialized = true;
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide && networkAdapter) networkAdapter.serverReady.Value = true;
    }

    protected void ActivatePassives()
    {
        foreach (var ability in abilities)
        {
            if (ability is PassiveAbility passive)
            {
                passive.Activate();
            }
        }
    }

    protected virtual void Update()
    {
        if (MasterNetworkAdapter.mode != NetworkMode.Off && MasterNetworkAdapter.mode != NetworkMode.Client)
            AttemptCreateNetworkObject(this as PlayerCore);
        if (!initialized) return;
        TickState();
    }

    /// <summary>
    /// Helper method for Tick() that automatically adds health based on passed regen rates
    /// </summary>
    /// <param name="currentHealth">current health</param>
    /// <param name="regenRate">regen rate</param>
    /// <param name="maxHealth">the maximum value this health can have</param>
    protected void RegenHealth(ref float currentHealth, float regenRate, float maxHealth)
    {
        var oldCurrentHealth = currentHealth;
        if (currentHealth + (regenRate * Time.deltaTime) > maxHealth) // if it would overheal
        {
            currentHealth = maxHealth; // set current health to max health
        }
        else
        {
            currentHealth += regenRate * Time.deltaTime; // add regenerated health
        }

        if (oldCurrentHealth != currentHealth) serverSyncHealthDirty = true;
    }

    /// <summary>
    /// Handles death and used for overriding
    /// </summary>
    virtual protected void DeathHandler()
    {
        if (currentHealth[1] <= 0 && !isDead)
        {
            // craft has been killed
            OnDeath(); // call death helper method
        }
    }

    protected void UpdateAuras()
    {
        bool speedAuraAtZero = SpeedAuraStacks == 0;
        healAuraStacks = 0;
        energyAuraStacks = 0;
        SpeedAuraStacks = 0;

        foreach (var aura in AIData.auras)
        {
            if (aura.Core.faction != faction) continue;
            if (Vector2.Distance(aura.transform.position, transform.position) > aura.GetRange()) continue;
            switch (aura.type)
            {
                case TowerAura.AuraType.Heal:
                    healAuraStacks++;
                    break;
                case TowerAura.AuraType.Speed:
                    SpeedAuraStacks++;
                    if (speedAuraAtZero && this as Craft)
                    {
                        (this as Craft).CalculatePhysicsConstants();
                    }
                    break;
                case TowerAura.AuraType.Energy:
                    energyAuraStacks++;
                    break;
            }
        }

        if (SpeedAuraStacks == 0 && !speedAuraAtZero && this as Craft)
        {
            (this as Craft).CalculatePhysicsConstants();
        }
    }

    public bool dirty;
    public EntityNetworkAdapter networkAdapter;

    public List<ShellPart> NetworkGetParts()
    {
        return parts;
    }

    /// <summary>
    /// Used to update the state of the craft- regeneration, timers, etc
    /// </summary>
    protected void TickState()
    {
        if (!MasterNetworkAdapter.lettingServerDecide || (!networkAdapter || networkAdapter.clientReady))
            DeathHandler();
        UpdateInteractible();
        UpdateAuras();
        if (isDead) // if the craft is dead
        {
            GetComponent<SpriteRenderer>().enabled = false; // disable craft sprite
            deathTimer += Time.deltaTime; // add time since last frame
            if (deathTimer >= 0.5F)
            {
                if (this is PlayerCore player && (deathTimer > 2))
                {
                    player.alerter.showMessage($"Respawning in {(TimeToDeath - (int)deathTimer)} second"
                                               + ((TimeToDeath - deathTimer) > 1 ? "s." : "."));
                }
            }

            if (deathTimer >= TimeToDeath)
            {
                if (this is PlayerCore player)
                {
                    player.alerter.showMessage("");
                }

                var BZM = SectorManager.instance?.GetComponent<BattleZoneManager>();
                if ((MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off || (!MasterNetworkAdapter.lettingServerDecide && (!BZM || BZM.playing)) || (!networkAdapter || networkAdapter.serverReady.Value)))
                    PostDeath();
            }
        }
        else
        {
            // not dead, continue normal state changing
            // regenerate
            if (!lettingServerDecide && blueprint.intendedType != EntityBlueprint.IntendedType.Tower)
            {
                RegenHealth(ref currentHealth[0], HealAuraStacks > 0 ? regenRate[0] * 20F : regenRate[0], maxHealth[0]);
                RegenHealth(ref currentHealth[1], HealAuraStacks > 0 ? regenRate[1] * 20F : regenRate[1], maxHealth[1]);
                RegenHealth(ref currentHealth[2], EnergyAuraStacks > 0 ? regenRate[2] * 50F : regenRate[2], maxHealth[2]);

                if (weaponGCDTimer < weaponGCD)
                {
                    weaponGCDTimer += Time.deltaTime; // tick GCD timer
                }
            }

            // check if busy state changing is due
            if (busyTimer > 5)
            {
                isBusy = false; // change state if it is
            }
            else
            {
                busyTimer += Time.deltaTime; // otherwise continue ticking timer
            }

            // check if combat state changing is due
            if (combatTimer > 5)
            {
                isInCombat = false; // change state if it is
            }
            else
            {
                combatTimer += Time.deltaTime; // otherwise continue ticking timer
            }

            // check if uninteractable state changing is due
            if (warpUninteractableTimer > 3)
            {
                isWarpUninteractable = false; // change state if it is
            }
            else
            {
                warpUninteractableTimer += Time.deltaTime; // otherwise continue ticking timer
            }

            if (RangeCheckDelegate != null && PlayerCore.Instance)
            {
                RangeCheckDelegate.Invoke(Vector2.SqrMagnitude(PlayerCore.Instance.transform.position - transform.position));
            }
        }

        if (serverSyncHealthDirty && !MasterNetworkAdapter.lettingServerDecide && networkAdapter)
        {
            if (!GetIsDead()) serverSyncHealthDirty = false;
            networkAdapter.UpdateHealthClientRpc(currentHealth[0], currentHealth[1], currentHealth[2]);
        }
    }

    /// <summary>
    /// Request weapon global cooldown (used by weapon abilities)
    /// </summary>
    public bool RequestGCD()
    {
        if (DialogueSystem.isInCutscene)
        {
            return false; // Entities should be controlled entirely by the cutscene, i.e. no siccing!
        }

        if (weaponGCDTimer >= weaponGCD)
        {
            weaponGCDTimer = 0;
            return true;
        }

        return false;
    }

    public virtual void RemovePart(ShellPart part)
    {
        if (!part) return;
        var lettingServerDecide = MasterNetworkAdapter.lettingServerDecide;
        if (part.GetComponent<Ability>())
        {
            part.GetComponent<Ability>().SetDestroyed(true);
        }

        entityBody.mass -= part.partMass;
        weight -= part.partMass * weightMultiplier;
        if (this is Craft craft)
        {
            craft.CalculatePhysicsConstants();
        }

        if (!lettingServerDecide)
        {
            Domino(part);
        }

        part.Detach();
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer && networkAdapter)
        {
            networkAdapter.DetachPartClientRpc(part.info.location);
        }
        parts.Remove(part);
    }


    /// <summary>
    /// Set the craft into combat
    /// </summary>
    public void SetIntoCombat()
    {
        isInCombat = true;
        combatTimer = 0;
    }

    /// <summary>
    /// Get whether the craft is in combat or not
    /// </summary>
    /// <returns>true if the craft is in combat, false otherwise</returns>
    public bool GetIsInCombat()
    {
        return isInCombat;
    }

    /// <summary>
    /// Set the craft into uninteractable state because it warped
    /// </summary>
    public void SetWarpUninteractable() {
        isWarpUninteractable = true;
        warpUninteractableTimer = 0;// reset timer
    }

    /// <summary>
    /// Get all the abilities of the craft by searching through all the parts
    /// </summary>
    /// <returns>All the abilities attached to the craft</returns>
    public Ability[] GetAbilities()
    {
        return abilities.ToArray();
        // create this array during start since it's likely that we'll be calling this multiple times
    }

    /// <summary>
    /// Get the targeting system of this craft
    /// </summary>
    /// <returns>the targeting system of the craft</returns>
    public TargetingSystem GetTargetingSystem()
    {
        return targeter;
    }
    public ExtendedTargetingSystem GetExtendedTargetingSystem()
    {
        return extendedTargeter;
    }

    /// <summary>
    /// Get the current health array of the craft
    /// </summary>
    /// <returns>the current health array of the craft</returns>
    public float[] GetHealth()
    {
        return currentHealth;
    }

    public void SyncHealth(float shell, float core, float energy)
    {
        currentHealth[0] = shell;
        currentHealth[1] = core;
        currentHealth[2] = energy;
    }

    /// <summary>
    /// Get the maximum health array of the craft
    /// </summary>
    /// <returns>the maximum health array of the craft</returns>
    public float[] GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Take shell damage, return residual damage to apply to core or parts
    /// </summary>
    public virtual float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy)
    {
        serverSyncHealthDirty = true;
        if (amount != 0 && ReticleScript.instance && ReticleScript.instance.DebugMode)
        {
            Debug.Log($"Damage: {amount} (f {lastDamagedBy?.faction} -> {faction})");
        }

        if (isAbsorbing && amount > 0f)
        {
            TakeEnergy(-amount);
            return 0f;
        }

        // counter drone fighting another drone, multiply damage accordingly
        if (this as Drone && lastDamagedBy is Drone drone && drone.type == DroneType.Counter)
        {
            amount *= 1.75F;
        }

        if (lastDamagedBy != this && amount > 0)
        {
            this.lastDamagedBy = lastDamagedBy; // heals require this check
        }

        if (amount > 0)
        {
            SetIntoCombat();
        }

        // pierce now goes directly to core first
        TakeCoreDamage(shellPiercingFactor * amount);
        float residue = 0; // get initial residual damage
        currentHealth[0] -= amount * (1 - shellPiercingFactor); // subtract amount from shell
        if (currentHealth[0] < 0)
        {
            // if shell has dipped below 0
            residue -= currentHealth[0]; // add residue
            currentHealth[0] = 0; // set shell to zero
        }

        currentHealth[0] = currentHealth[0] > maxHealth[0] ? maxHealth[0] : currentHealth[0];
        // reset health if beyond max
        return residue;
    }

    /// <summary>
    /// Take core damage.
    /// </summary>
    public virtual void TakeCoreDamage(float amount)
    {
        if (isAbsorbing && amount > 0f)
        {
            TakeEnergy(-amount);
            return;
        }

        currentHealth[1] -= amount;
        if (currentHealth[1] < 0)
        {
            currentHealth[1] = 0;
        }

        currentHealth[1] = currentHealth[1] > maxHealth[1] ? maxHealth[1] : currentHealth[1];
    }

    /// <summary>
    /// Removes energy from the craft
    /// </summary>
    /// <param name="amount">The amount of energy to remove</param>
    public void TakeEnergy(float amount)
    {
        currentHealth[2] -= amount; // remove energy
        currentHealth[2] = currentHealth[2] > maxHealth[2] ? maxHealth[2] : currentHealth[2];
    }

    ///
    /// Recursive method that sets up a directed acyclic graph describing part connections.
    /// When parts are detached from a ship, all their children are too.
    ///
    private void ConnectedTreeCreator()
    {
        shell.children.Clear();
        foreach (ShellPart part in parts)
        {
            if (part == shell)
            {
                continue;
            }

            part.children.Clear();

            // attach all core-connected parts to the shell as well
            if (part.IsAdjacent(shell))
            {
                part.parent = shell;
                shell.children.Add(part);
            }
        }

        ConnectedTreeHelper(shell);
    }

    private void ConnectedTreeHelper(ShellPart parent)
    {
        if (parent != shell)
        {
            foreach (ShellPart part in parts)
            {
                if (part.parent || part == parent || part == shell)
                {
                    continue;
                }

                if (part.IsAdjacent(parent))
                {
                    part.parent = parent;
                    parent.children.Add(part);
                }
            }
        }

        foreach (ShellPart part in parent.children)
        {
            ConnectedTreeHelper(part);
        }
    }

    private void DominoHelper(ShellPart parent)
    {
        foreach (ShellPart part in parent.children.ToArray())
        {
            if (part)
            {
                RemovePart(part);
            }
        }
    }

    private void Domino(ShellPart part)
    {
        if (part.parent)
        {
            part.parent.children.Remove(part);
        }

        DominoHelper(part);
    }

    public float[] GetRegens()
    {
        return regenRate;
    }

    public void SetRegens(float[] newRegen)
    {
        regenRate = newRegen;
    }

    public void SetMaxHealth(float[] maxHealths, bool healToMaxHealth)
    {
        maxHealth = maxHealths;
        if (healToMaxHealth)
        {
            maxHealth.CopyTo(currentHealth, 0);
        }
    }

    protected virtual void FixedUpdate()
    {
    }

    public string GetID()
    {
        return ID;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Dialogue GetDialogue()
    {
        return dialogue;
    }

    public string GetName()
    {
        return name;
    }

    public int GetFaction()
    {
        return faction;
    }

    public TerrainType GetTerrain()
    {
        return terrain;
    }

    public EntityCategory GetCategory()
    {
        return category;
    }

    public bool GetInvisible()
    {
        return IsInvisible;
    }

    public void ToggleColliders(bool enable)
    {
        if (enable != collidersEnabled)
        {
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
            {
                if (c.gameObject.name != "Shell Sprite")
                {
                    c.enabled = enable;
                }
            }

            collidersEnabled = enable;
        }
    }

    // Used by "dumb" stations, that just use their abilities whenever possible
    protected void TickAbilitiesAsStation()
    {
        if (MasterNetworkAdapter.mode == NetworkMode.Client) return;

        var enemyTargetFound = SectorManager.instance?.current?.type != Sector.SectorType.BattleZone;
        if (!enemyTargetFound && BattleZoneManager.getTargets() != null && BattleZoneManager.getTargets().Length > 0)
        {
            foreach (var target in BattleZoneManager.getTargets())
            {
                if (!FactionManager.IsAllied(target.faction, faction) && !target.GetIsDead())
                {
                    enemyTargetFound = true;
                    break;
                }
            }
        }

        foreach (ActiveAbility active in GetComponentsInChildren<ActiveAbility>())
        {
            if (!(active is SpawnDrone) || enemyTargetFound)
            {
                active.Tick();
                active.Activate();
            }
        }
    }
}
