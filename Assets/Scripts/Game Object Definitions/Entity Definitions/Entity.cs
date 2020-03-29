﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The base class of every "being" in the game.
/// </summary>
public class Entity : MonoBehaviour, IDamageable {

    public ShellPart shell;
    protected static int maxAirLayer = 1; // the maximum sorting group layer of all entities
    protected static int maxGroundLayer = 1;
    protected SortingGroup group;
    protected float[] maxHealth; // maximum health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] regenRate; // regeneration rate of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected List<Ability> abilities; // abilities
    protected Rigidbody2D entityBody; // entity to modify with this script
    protected Collider2D hitbox; // the hitbox of the entity (excluding extra parts)
    protected TargetingSystem targeter; // the TargetingSystem of the entity
    protected bool isInCombat; // whether the entity is in combat or not
    protected bool isBusy; // whether the entity is busy or not
    protected bool isDead; // whether the entity is currently dead or not
    protected float busyTimer; // the time since the entity was last set to busy
    protected float combatTimer; // the time since the entity was last set into combat
    protected float deathTimer; // the time since the entity last died;
    protected GameObject explosionCirclePrefab; // prefabs for death explosion
    protected GameObject explosionLinePrefab;
    protected GameObject respawnImplosionPrefab;
    protected GameObject deathExplosionPrefab;
    protected List<ShellPart> parts; // List containing all parts of the entity
    public float[] currentHealth; // current health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    public int faction; // What side the entity belongs to (0 = green, 1 = red, 2 = blue...) //TODO: get this from a file?
    public EntityBlueprint blueprint; // blueprint of entity containing parts
    public Vector3 spawnPoint;
    public Dialogue dialogue; // dialogue of entity
    protected bool isDraggable; // is the entity draggable?
    protected Draggable draggable; // associated draggable
    protected bool initialized; // is the entity safe to call update() on?
    public EntityCategory category = EntityCategory.Unset; // these two fields will be changed via hardcoding in child class files
    public string ID; // used in tasks

    public SectorManager sectorMngr;
    protected Entity lastDamagedBy;

    public string entityName;

    private float weaponGCD = 0.1F; // weapon global cooldown
    private float weaponGCDTimer;

    public enum TerrainType // terrain type of entity
    {
        Ground,
        Air,
        All,
        Unset
    }

    public enum EntityCategory // category of entity (carriers, outposts and bunkers are stations, everything else are units)
    {
        Station,
        Unit,
        All,
        Unset
    }

    TerrainType terrain = TerrainType.Unset;
    public TerrainType Terrain { get { return terrain; } protected set { terrain = value; } }

    // boolean used to check if proximity and reticle interactions should trigger for this entity
    private bool interactible = false;
    
    // prevents interaction while entities are in paths
    public bool isPathing = false;
    public void UpdateInteractible()
    {
        interactible = GetDialogue() && (faction == 0); 

        // These are implications, not a biconditional; interactibility is not necessarily true/false if there are no
        // task overrides or pathing set up. Hence the if statements are needed here
        if(ID != null && TaskManager.interactionOverrides.ContainsKey(ID) 
            && TaskManager.interactionOverrides[ID].Count > 0) interactible = true;

        if(ID != null && DialogueSystem.interactionOverrides.ContainsKey(ID) 
            && DialogueSystem.interactionOverrides[ID].Count > 0) interactible = true;

        if(isPathing || DialogueSystem.isInCutscene) interactible = false;
    }

    public bool GetInteractible()
    {
        return interactible;
    }

    /// <summary>
    /// Generate shell parts in the blueprint, change ship stats accordingly
    /// </summary>
    protected virtual void BuildEntity()
    {
        // all created entities should have blueprints!
        if (!blueprint) Debug.Log(this + " does not have a blueprint! EVERY constructed entity should have one!");

        // Remove possible old parts from list
        foreach(var part in parts)
        {
            if(part && part.gameObject && part.gameObject.name != "Shell Sprite")
                Destroy(part.gameObject);
        }
        parts.Clear();
        blueprint.shellHealth.CopyTo(maxHealth, 0);
        blueprint.baseRegen.CopyTo(regenRate, 0);

        if(blueprint) this.dialogue = blueprint.dialogue;

        if (!GetComponent<SortingGroup>())
        {
            group = gameObject.AddComponent<SortingGroup>();
            if(this as AirCraft || this as AirConstruct) {
                group.sortingLayerName = "Air Entities";
                group.sortingOrder = ++maxAirLayer;
            } else 
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
            renderer.sortingOrder = 100; // hardcoded max shell sprite value TODO: change this to being dynamic with the other parts
            if (blueprint)
            { // check if it contains a blueprint (it should)
                renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
            }
            else renderer.sprite = ResourceManager.GetAsset<Sprite>("core1_shell"); // set to default shellcore sprite
            ShellPart part = childObject.AddComponent<ShellPart>();
            part.detachible = false;
            shell = part;
        } else {
            var renderer = transform.Find("Shell Sprite").GetComponent<SpriteRenderer>();
            renderer.color = FactionColors.colors[faction]; // needed to reset outpost colors
            renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        }

        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            explosionCirclePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionCirclePrefab.AddComponent<DrawCircleScript>().SetStartColor(FactionColors.colors[faction]);
            explosionCirclePrefab.SetActive(false);
        }
        if (!explosionLinePrefab)
        {
            explosionLinePrefab = new GameObject("Explosion Line");
            explosionLinePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionLinePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionLinePrefab.AddComponent<DrawLineScript>().SetStartColor(FactionColors.colors[faction]);
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
            renderer.color = FactionColors.colors[faction];
            if (blueprint)
            { // check if it contains a blueprint (it should)
                if (blueprint.coreSpriteID == "" && blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore)
                {
                    Debug.Log(this + "'s blueprint does not contain a core sprite ID!"); 
                    // check if the blueprint does not contain a core sprite ID (it should) 
                }
                renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
            }
            else renderer.sprite = ResourceManager.GetAsset<Sprite>("core1_light");
            renderer.sortingOrder = 101;
        } else GetComponent<SpriteRenderer>().color = FactionColors.colors[faction]; // needed to reset outpost colors
        if (!GetComponent<Rigidbody2D>())
        {
            entityBody = gameObject.AddComponent<Rigidbody2D>();
            entityBody.gravityScale = 0;
            entityBody.drag = 10;
            entityBody.angularDrag = 100;
        }
        /*
        if (!GetComponent<Collider2D>())
        {
            hitbox = gameObject.AddComponent<PolygonCollider2D>();
            hitbox.isTrigger = true;

            // Disable collider if no sprite
            if(!GetComponent<SpriteRenderer>().sprite) hitbox.enabled = false; 
        }
        // Another collider disable check
        else if(!GetComponent<SpriteRenderer>().sprite) GetComponent<Collider2D>().enabled = false;
        */
        if(!transform.Find("Minimap Image"))
        {
            GameObject childObject = new GameObject("Minimap Image");
            childObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            if(category == EntityCategory.Station && !(this is Turret))
            {
                if(this as Outpost)
                {
                    renderer.sprite = ResourceManager.GetAsset<Sprite>("outpost_minimap_sprite");
                }
                else if(this as Bunker)
                {
                    renderer.sprite = ResourceManager.GetAsset<Sprite>("bunker_minimap_sprite");                   
                }
                else
                {
                    renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
                } 
                renderer.transform.localScale = new Vector3(3.5F, 3.5F, 3.5F);
            }
            else renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
            childObject.AddComponent<MinimapLockRotationScript>();
        }

        abilities = new List<Ability>();

        entityName = blueprint.entityName;
        name = blueprint.entityName;
        GetComponent<Rigidbody2D>().mass = 1; // reset mass

        var isLightDrone = this as Drone && (this as Drone).type == DroneType.Light; // used for light drone weight reduction
        //For shellcores, create the tractor beam
        // Create shell parts
        if (blueprint != null)
        {
            for (int i = 0; i < blueprint.parts.Count; i++)
            {

                EntityBlueprint.PartInfo part = blueprint.parts[i];
                PartBlueprint partBlueprint = ResourceManager.GetAsset<PartBlueprint>(part.partID);

                GameObject partObject = ShellPart.BuildPart(partBlueprint);
                ShellPart shellPart = partObject.GetComponent<ShellPart>();
                shellPart.info = part;

                //Add an ability to the part:

                WeaponAbility ab = AbilityUtilities.AddAbilityToGameObjectByID(partObject, part.abilityID, part.secondaryData, part.tier) as WeaponAbility;
                if(ab) { // add weapon diversity
                    ab.type = DroneUtilities.GetDiversityTypeByEntity(this);
                }
                partObject.transform.SetParent(transform, false);
                partObject.transform.SetAsFirstSibling();
                partObject.transform.localEulerAngles = new Vector3(0, 0, part.rotation);
                partObject.transform.localPosition = new Vector3(part.location.x, part.location.y, 0);
                SpriteRenderer sr = partObject.GetComponent<SpriteRenderer>();
                // sr.flipX = part.mirrored; this doesn't work, it does not flip the collider hitbox
                var tmp = partObject.transform.localScale;
                tmp.x = part.mirrored ? -1 : 1;
                partObject.transform.localScale = tmp;
                sr.sortingOrder = i + 2;
                entityBody.mass += (isLightDrone ? partBlueprint.mass * 0.6F : partBlueprint.mass);
                maxHealth[0] += partBlueprint.health / 2;
                maxHealth[1] += partBlueprint.health / 4;

                string shooterID = AbilityUtilities.GetShooterByID(part.abilityID, part.secondaryData);
                // Add shooter
                if (shooterID != null)
                {
                    var shooter = new GameObject("Shooter");
                    shooter.transform.SetParent(partObject.transform);
                    shooter.transform.localPosition = Vector3.zero;
                    var shooterSprite = shooter.AddComponent<SpriteRenderer>();
                    shooterSprite.sprite = ResourceManager.GetAsset<Sprite>(shooterID);
                    if(blueprint.parts.Count < 2) shooterSprite.sortingOrder = 500;
                    else shooterSprite.sortingOrder = sr.sortingOrder + 1;
                    shellPart.shooter = shooter;
                }

                var weaponAbility = partObject.GetComponent<WeaponAbility>();
                if (weaponAbility)
                {

                    // if the terrain and category wasn't preset set to the enitity's properties

                    if(weaponAbility.terrain == TerrainType.Unset)
                        weaponAbility.terrain = Terrain;
                    if(weaponAbility.category == EntityCategory.Unset)
                        weaponAbility.category = category;
                }
                    

                parts.Add(partObject.GetComponent<ShellPart>());
                if(partObject.GetComponent<Ability>()) abilities.Insert(0, partObject.GetComponent<Ability>());

                // Disable collider if no sprite
                if(!(partObject.GetComponent<SpriteRenderer>() && partObject.GetComponent<SpriteRenderer>().sprite)
                    && partObject.GetComponent<Collider2D>() && !partObject.GetComponent<Harvester>()) 
                    partObject.GetComponent<Collider2D>().enabled = false;
            }
        }

        if (this as ShellCore && !gameObject.GetComponentInChildren<MainBullet>())
        {
            MainBullet mainBullet = gameObject.AddComponent<MainBullet>();
            mainBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
            mainBullet.terrain = TerrainType.Air;
            mainBullet.SetActive(true);
            abilities.Insert(0, mainBullet);
        } else abilities.Insert(0, gameObject.GetComponentInChildren<MainBullet>());

        // unique abilities for mini and worker drones here
        if(this as Drone) {
            Drone drone = this as Drone;
            switch(drone.type) {
                case DroneType.Mini:
                    var shellObj = transform.Find("Shell Sprite").gameObject;
                    Ability ab = AbilityUtilities.AddAbilityToGameObjectByID(shellObj, 6, null, 1);
                    var shooter = new GameObject("Shooter");
                    shooter.transform.SetParent(shellObj.transform);
                    shooter.transform.localPosition = Vector3.zero;
                    var shooterSprite = shooter.AddComponent<SpriteRenderer>();
                    shooterSprite.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(6));
                    shooterSprite.sortingOrder = 500;
                    shellObj.GetComponent<ShellPart>().shooter = shooter;
                    (ab as WeaponAbility).terrain = TerrainType.Air;
                    abilities.Insert(0, ab);
                    break;
                default:
                    break;
            }
        }

        // check to see if the entity is interactible
        if(dialogue && faction == 0) interactible = true;

        Transform shellSprite = shell.transform;
        if(shellSprite)
        {
            parts.Add(shellSprite.GetComponent<ShellPart>());
        }
        ConnectedTreeCreator();

        maxHealth.CopyTo(currentHealth, 0);
        // Add abilities
    }
   
     public bool GetIsDead() {
        return isDead; // is dead
    }

    /// <summary>
    /// Helper method for death animation and state changing
    /// </summary>
    protected virtual void OnDeath() {
        // set death, interactibility and immobility
        interactible = false;
        isDead = true;
        SetIntoCombat();
        deathTimer = 0; // reset death timer
        transform.Find("Minimap Image").GetComponent<SpriteRenderer>().enabled = false; // remove from minimap

        AudioManager.PlayClipByID("clip_explosion1", transform.position);

        for(int i = 0; i < parts.Count; i++)
        {
            if(faction != 0 && (parts[i] != shell) && Random.value < 0.08f && !(this as PlayerCore) && this as ShellCore && 
                ((this as ShellCore).GetCarrier() == null)) {
                parts[i].SetCollectible(true);
                if(sectorMngr) AIData.strayParts.Add(parts[i]);
            }
            parts[i].Detach();
        }

        if(lastDamagedBy as PlayerCore) (lastDamagedBy as PlayerCore).credits += 5;

        GameObject deathExplosion = Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
        /*
        GameObject tmp = Instantiate(explosionCirclePrefab); // instantiate circle explosion
        tmp.SetActive(true);
        tmp.transform.SetParent(transform, false);
        tmp.GetComponent<DrawCircleScript>().Initialize();
        Destroy(tmp, 2); // destroy explosions after 2 seconds
        for (int i = 0; i < 3; i++) { // instantiate line explosions
            tmp = Instantiate(explosionLinePrefab);
            tmp.SetActive(true);
            tmp.transform.SetParent(transform, false);
            tmp.GetComponent<DrawLineScript>().Initialize();
            Destroy(tmp, 2); // destroy explosions after 2 seconds
        }
        */

        if(GameObject.Find("SectorManager")) 
            GameObject.Find("SectorManager").GetComponent<BattleZoneManager>().UpdateCounters();

        if(NodeEditorFramework.Standard.DestroyEntityCondition.OnUnitDestroyed != null)
            NodeEditorFramework.Standard.DestroyEntityCondition.OnUnitDestroyed.Invoke(this);
    }

    protected virtual void PostDeath() 
    {
        Destroy(gameObject);
    }

    virtual protected void Awake() {
        // initialize instance fields
        currentHealth = new float[3];
        maxHealth = new float[3];
        regenRate = new float[3];
        parts = new List<ShellPart>();
        isBusy = false;
        targeter = new TargetingSystem(transform); // create the associated targeting system for this craft
        isInCombat = false;

        if (!GetComponent<Draggable>() && (this as Drone || this as Tank || this as Turret))
        {
            draggable = gameObject.AddComponent<Draggable>();
        }
        else if (GetComponent<Draggable>() && !draggable)
        {
            Debug.Log("Draggable was added to an entity manually, " +
                "it should be added automatically by setting isDraggable to true!");
        }

        if(!AIData.entities.Contains(this)) 
        {
            AIData.entities.Add(this);
        }
           
        if(this is IVendor)
            AIData.vendors.Add(this);
    }

    protected virtual void OnDestroy()
    {
        if(AIData.entities.Contains(this))
            AIData.entities.Remove(this);
        if (this is IVendor)
            AIData.vendors.Remove(this);
        SectorManager.instance.RemoveObject(name);
    }

    virtual protected void Start()
    {
        BuildEntity(); // Generate shell parts around the entity
        transform.position = spawnPoint;
        GetComponentInChildren<MinimapLockRotationScript>().Initialize(); // initialize the minimap dot
        targeter.SetTarget(null);
        //transform.rotation = Quaternion.identity; // reset rotation
        GetComponent<SpriteRenderer>().enabled = true; // enable sprite renderer
        busyTimer = 0; // reset busy timer
        initialized = true;
    }

    protected virtual void Update() {
        if(initialized) TickState(); // tick state
    }

    /// <summary>
    /// Helper method for Tick() that automatically adds health based on passed regen rates
    /// </summary>
    /// <param name="currentHealth">current health</param>
    /// <param name="regenRate">regen rate</param>
    /// <param name="maxHealth">the maximum value this health can have</param>
    protected void RegenHealth(ref float currentHealth, float regenRate, float maxHealth) {
        if (currentHealth + (regenRate * Time.deltaTime) > maxHealth) // if it would overheal
        {
            currentHealth = maxHealth; // set current health to max health
        }
        else
        {
            currentHealth += regenRate * Time.deltaTime; // add regenerated health
        }
    }

    /// <summary>
    /// Handles death and used for overriding
    /// </summary>
    virtual protected void DeathHandler()
    {
        if (currentHealth[1] <= 0 && !isDead)
        { // craft has been killed
            OnDeath(); // call death helper method
        }
    }
    /// <summary>
    /// Used to update the state of the craft- regeneration, timers, etc
    /// </summary>
    protected void TickState() {
        DeathHandler();
        UpdateInteractible();
        if (isDead) // if the craft is dead
        {
            GetComponent<SpriteRenderer>().enabled = false; // disable craft sprite
            deathTimer += Time.deltaTime; // add time since last frame
            if (deathTimer >= 0.5F) 
            {
                if(this as PlayerCore && (deathTimer > 2)) {
                    ((PlayerCore)this).alerter.showMessage("Respawning in " + (5 - (int)deathTimer) + " second"
                    + ((5 - deathTimer) > 1 ? "s." : "."));
                }
            }
            if (deathTimer >= 5F)
            {
                if(this as PlayerCore) ((PlayerCore)this).alerter.showMessage("");
                PostDeath();
            }
        }
        else { // not dead, continue normal state changing
            // regenerate
            RegenHealth(ref currentHealth[0], regenRate[0], maxHealth[0]); 
            RegenHealth(ref currentHealth[1], regenRate[1], maxHealth[1]);
            RegenHealth(ref currentHealth[2], regenRate[2], maxHealth[2]);

            if(weaponGCDTimer < weaponGCD) {
                weaponGCDTimer += Time.deltaTime; // tick GCD timer
            }
            // check if busy state changing is due
            if (busyTimer > 5)
            {
                isBusy = false; // change state if it is
            }
            else busyTimer += Time.deltaTime; // otherwise continue ticking timer

            // check if combat state changing is due
            if (combatTimer > 5)
            {
                isInCombat = false; // change state if it is
            }
            else combatTimer += Time.deltaTime; // otherwise continue ticking timer
        }
    }

    /// <summary>
    /// Request weapon global cooldown (used by weapon abilities)
    /// </summary>
    public bool RequestGCD() {
        if(DialogueSystem.isInCutscene) return false; // Entities should be controlled entirely by the cutscene, i.e. no siccing!
        if(weaponGCDTimer >= weaponGCD) {
            weaponGCDTimer = 0;
            return true;
        }
        return false;
    }

    public virtual void RemovePart(ShellPart part)
    {
        if(part.GetComponent<Ability>())
        {
            part.GetComponent<Ability>().SetDestroyed(true);
        }
        entityBody.mass -= part.partMass;
        Domino(part);
        part.Detach();
        parts.Remove(part);
    }

    /// <summary>
    /// Make the craft busy
    /// </summary>
    public void MakeBusy() {
        isBusy = true; // set to true
        busyTimer = 0; // reset timer
    }

    /// <summary>
    /// Get whether the craft is busy or not
    /// </summary>
    /// <returns>true if the craft is busy, false otherwise</returns>
    public bool GetIsBusy() {
        return isBusy; // is busy
    }

    /// <summary>
    /// Set the craft into combat
    /// </summary>
    public void SetIntoCombat() {
        isInCombat = true; // set these to true
        isBusy = true;
        busyTimer = 0; // reset timers
        combatTimer = 0;
    }

    /// <summary>
    /// Get whether the craft is in combat or not
    /// </summary>
    /// <returns>true if the craft is in combat, false otherwise</returns>
    public bool GetIsInCombat()
    {
        return isInCombat; // is in combat
    }

    /// <summary>
    /// Get all the abilities of the craft by searching through all the parts
    /// </summary>
    /// <returns>All the abilities attached to the craft</returns>
    public Ability[] GetAbilities() {
        return abilities.ToArray(); 
        // create this array during start since it's likely that we'll be calling this multiple times
    }

    /// <summary>
    /// Get the targeting system of this craft
    /// </summary>
    /// <returns>the targeting system of the craft</returns>
    public TargetingSystem GetTargetingSystem() {
        return targeter; // get targeting system
    }

    /// <summary>
    /// Get the current health array of the craft
    /// </summary>
    /// <returns>the current health array of the craft</returns>
    public float[] GetHealth() {
        return currentHealth; // get current health
    }

    /// <summary>
    /// Get the maximum health array of the craft
    /// </summary>
    /// <returns>the maximum health array of the craft</returns>
    public float[] GetMaxHealth() {
        return maxHealth; // get max health
    }

    /// <summary>
    /// Take shell damage, return residual damage to apply to core or parts
    /// </summary>
    public float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy) {

        // counter drone fighting another drone, multiply damage accordingly
        if(this as Drone && lastDamagedBy as Drone && (lastDamagedBy as Drone).type == DroneType.Counter)
            amount *= 1.75F;
        if(lastDamagedBy != this && amount > 0) this.lastDamagedBy = lastDamagedBy; // heals require this check
        if (amount > 0) SetIntoCombat();
        float residue = shellPiercingFactor * amount; // get initial residual damage
        currentHealth[0] -= amount * (1 - shellPiercingFactor); // subtract amount from shell
        if (currentHealth[0] < 0) { // if shell has dipped below 0
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
    public void TakeCoreDamage(float amount) {
        currentHealth[1] -= amount;
        if (currentHealth[1] < 0) currentHealth[1] = 0;
        currentHealth[1] = currentHealth[1] > maxHealth[1] ? maxHealth[1] : currentHealth[1];
    }

    /// <summary>
    /// Removes energy from the craft
    /// </summary>
    /// <param name="amount">The amount of energy to remove</param>
    public void TakeEnergy(float amount) {
        currentHealth[2] -= amount; // remove energy
        currentHealth[2] = currentHealth[2] > maxHealth[2] ? maxHealth[2] : currentHealth[2];
    }

    private void ConnectedTreeCreator() {
        shell.children.Clear();
        foreach(ShellPart part in parts) 
        {
            if(part == shell) continue;
            part.children.Clear();

            // attach all core-connected parts to the shell as well
            if(part.IsAdjacent(shell)) {
                part.parent = shell;
                shell.children.Add(part);
            }
        }
        ConnectedTreeHelper(shell);
    }

    private void ConnectedTreeHelper(ShellPart parent) {
        if(parent != shell)
            foreach(ShellPart part in parts) 
            {
                if(part.parent || part == parent || part == shell) continue;
                if(part.IsAdjacent(parent)) {
                    part.parent = parent;
                    parent.children.Add(part);
                }
            }
        foreach(ShellPart part in parent.children) {
            ConnectedTreeHelper(part);
        }
    }
    private void DominoHelper(ShellPart parent) {
        foreach(ShellPart part in parent.children.ToArray()) {
            if(part) 
            {
                RemovePart(part);
            }
        }
    }

    private void Domino(ShellPart part) {
        if(part.parent) {
            part.parent.children.Remove(part);
        }
        DominoHelper(part);
    }

    public float[] GetRegens() {
        return regenRate;
    }

    public void SetRegens(float[] newRegen) {
        regenRate = newRegen;
    }

    public void SetMaxHealth(float[] maxHealths, bool healToMaxHealth) {
        maxHealth = maxHealths;
        if(healToMaxHealth) maxHealth.CopyTo(currentHealth, 0);
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

    public string GetName() {
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
}
