using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class of every "being" in the game.
/// </summary>
public class Entity : MonoBehaviour {

    protected float[] maxHealth; // maximum health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] regenRate; // regeneration rate of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected Ability[] abilities; // abilities
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
    protected List<ShellPart> parts; // List containing all parts of the entity
    protected Material explosionMaterial;
    protected Sprite coreSprite;
    protected Sprite shellSprite;
    protected GameObject bulletPrefab;
    protected Sprite minimapSprite;

    public float[] currentHealth; // current health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    public int faction; // What side the entity belongs to (0 = green, 1 = red, 2 = blue...) //TODO: get this from a file?
    public EntityBlueprint blueprint;
    public Vector3 spawnPoint;
    public Dialogue dialogue;
    protected bool isDraggable;
    protected Draggable draggable;
    private bool initialized;

    /// <summary>
    /// Generate shell parts in the blueprint, change ship stats accordingly
    /// </summary>
    protected virtual void BuildEntity()
    {
        // Remove possible old parts from list
        parts.Clear();
        maxHealth = new float[] { 100, 100, 100 };

        if (blueprint == null)
            return;

        if (!transform.Find("Shell Sprite"))
        {
            GameObject childObject = new GameObject("Shell Sprite");
            childObject.transform.SetParent(transform, false);
            PolygonCollider2D collider = childObject.AddComponent<PolygonCollider2D>();
            collider.isTrigger = true;
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 100;
            renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
            ShellPart part = childObject.AddComponent<ShellPart>();
            part.detachible = false;
        }
        if (!GetComponent<MainBullet>() && this as ShellCore)
        {
            MainBullet mainBullet = gameObject.AddComponent<MainBullet>();
            mainBullet.bulletPrefab = ResourceManager.GetAsset<GameObject>("bullet_prefab");
        }
        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            explosionCirclePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionCirclePrefab.AddComponent<DrawCircleScript>();
            explosionCirclePrefab.SetActive(false);
        }
        if (!explosionLinePrefab)
        {
            explosionLinePrefab = new GameObject("Explosion Line");
            explosionLinePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionLinePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            explosionLinePrefab.AddComponent<DrawLineScript>();
            explosionLinePrefab.SetActive(false);
        }
        if (!GetComponent<SpriteRenderer>())
        {
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
            renderer.sortingOrder = 101;
        }
        if (!GetComponent<Rigidbody2D>())
        {
            entityBody = gameObject.AddComponent<Rigidbody2D>();
            entityBody.gravityScale = 0;
            entityBody.drag = 10;
            entityBody.angularDrag = 100;
        }
        if (!GetComponent<Collider2D>())
        {
            hitbox = gameObject.AddComponent<PolygonCollider2D>();
            hitbox.isTrigger = true;
        }        
        if(!transform.Find("Minimap Image"))
        {
            GameObject childObject = new GameObject("Minimap Image");
            childObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
            childObject.AddComponent<MinimapLockRotationScript>();
        }
        if (!GetComponent<Draggable>())
        {
            draggable = gameObject.AddComponent<Draggable>();
        } else if(!draggable)
        {
            Debug.Log("Draggable was added to an entity manually, " +
                "it should be added automatically by setting isDraggable to true!");
        }

        GetComponent<Rigidbody2D>().mass = 1; // reset mass

        //For shellcores, create the tractor beam
        // Create shell parts
        if (blueprint != null)
        {
            for (int i = 0; i < blueprint.parts.Count; i++)
            {
                EntityBlueprint.PartInfo part = blueprint.parts[i];

                GameObject prefab = ResourceManager.GetAsset<GameObject>(part.partID);
                GameObject obj = Instantiate(prefab);
                obj.SetActive(true);
                obj.transform.SetParent(transform, false);
                obj.transform.SetAsFirstSibling();
                obj.transform.localEulerAngles = new Vector3(0, 0, part.rotation);
                obj.transform.localPosition = new Vector3(part.location.x, part.location.y, 0);
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                // sr.flipX = part.mirrored; this doesn't work, it does not flip the collider hitbox
                var tmp = obj.transform.localScale;
                tmp.x = part.mirrored ? -1 : 1;
                obj.transform.localScale = tmp;
                sr.sortingOrder = i + 2;
                ShellPart partComp = obj.GetComponent<ShellPart>();
                entityBody.mass += partComp.GetPartMass();
                maxHealth[0] += partComp.GetPartHealth() / 2;
                maxHealth[1] += partComp.GetPartHealth() / 4;

                parts.Add(obj.GetComponent<ShellPart>());
            }
        }
        Transform shellSprite = transform.Find("Shell Sprite");
        if(shellSprite)
        {
            parts.Add(shellSprite.GetComponent<ShellPart>());
        }

        explosionMaterial = ResourceManager.GetAsset<Material>("white_material");

        currentHealth[0] = maxHealth[0];
        currentHealth[1] = maxHealth[1];
        currentHealth[2] = maxHealth[2];
        regenRate[0] = 100;
        regenRate[2] = 100;
        // Add abilities
        abilities = GetComponentsInChildren<Ability>();
    }
   
     public bool GetIsDead() {
        return isDead; // is dead
    }

    /// <summary>
    /// Helper method for death animation and state changing
    /// </summary>
    protected virtual void OnDeath() {
        // set death and immobility
        isDead = true;
        SetIntoCombat();
        deathTimer = 0; // reset death timer
        transform.Find("Minimap Image").GetComponent<SpriteRenderer>().enabled = false; // remove from minimap

        for(int i = 0; i < parts.Count; i++)
        {
            parts[i].SetCollectible((parts[i].name != "Shell Sprite") && Random.Range(0F,5) > 2.5F && !(this as PlayerCore));
            parts[i].Detach();
        }

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
    /// Used to update the state of the craft- regeneration, timers, etc
    /// </summary>
    protected void TickState() {

        if (currentHealth[1] <= 0 && !isDead) { // craft has been killed
            OnDeath(); // call death helper method
        }
        if (isDead) // if the craft is dead
        {
            deathTimer += Time.deltaTime; // add time since last frame
            if (deathTimer >= 1) // hardcoded based on animation
            {
                GetComponent<SpriteRenderer>().enabled = false; // disable craft sprite
            }
            if (deathTimer >= 2)
            {
                PostDeath();
            }
        }
        else { // not dead, continue normal state changing
            // regenerate shell and energy
            RegenHealth(ref currentHealth[0], regenRate[0], maxHealth[0]); 
            RegenHealth(ref currentHealth[2], regenRate[2], maxHealth[2]);

            //if (targeter.GetTarget() != null) // locked on currently
            //{
            //    //Lock on only to enemies
            //    Craft targetCraft = targeter.GetTarget().GetComponent<Craft>();
            //    if(targetCraft && targetCraft.faction != faction)
            //    {
            //        // rotate craft to lock on
            //        RotateCraft(targeter.GetTarget().transform.position - transform.position);
            //    }
            //}

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

    public virtual void RemovePart(ShellPart part)
    {
        if(part.GetComponent<Ability>())
        {
            part.GetComponent<Ability>().SetDestroyed(true);
        }

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
        return abilities; 
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
    /// Removes health from the shell and/or core based on the passed piercing factor and current health
    /// </summary>
    /// <param name="amount">The amount of damage to do</param>
    /// <param name="shellPiercingFactor">The factor of damage that pierces through the shell into the core</param>
    public void TakeDamage(float amount, float shellPiercingFactor) {
        if (amount > 0) SetIntoCombat();
        currentHealth[0] -= amount * (1 - shellPiercingFactor); // subtract amount from shell
        if (currentHealth[0] < 0) { // if shell has dipped below 0
            currentHealth[1] += currentHealth[0]; // remove excess from core
            currentHealth[0] = 0; // set shell to zero
        }
        currentHealth[1] -= amount * shellPiercingFactor; // remove the rest of the damage from the core
        if (currentHealth[1] < 0) currentHealth[1] = 0;
    }

    /// <summary>
    /// Removes energy from the craft
    /// </summary>
    /// <param name="amount">The amount of energy to remove</param>
    public void TakeEnergy(float amount) {
        currentHealth[2] -= amount; // remove energy
    }
}
