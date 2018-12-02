using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every entity that can move is a craft. This includes drones and ShellCores.
/// </summary>
public abstract class Craft : MonoBehaviour
{
    protected float[] currentHealth; // current health of the craft (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] maxHealth; // maximum health of the craft (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] regenRate; // regeneration rate of the craft (index 0 is shell, index 1 is core, index 2 is energy)
    protected Ability[] abilities; // abilities
    public int enginePower; // craft's engine power, determines how fast it goes
    public Rigidbody2D craftBody; // craft to modify with this script
    public Collider2D hitbox; // the hitbox of the craft (excluding extra parts)
    protected TargetingSystem targeter; // the TargetingSystem of the craft
    protected bool isInCombat; // whether the craft is in combat or not
    protected bool isBusy; // whether the craft is busy or not
    protected bool isDead; // whether the craft is currently dead or not
    protected bool isImmobile; // whether the craft is immobile or not
    protected bool respawns; // whether the craft respawns or not
    protected float busyTimer; // the time since the craft was last set to busy
    protected float combatTimer; // the time since the craft was last set into combat
    protected float deathTimer; // the time since the craft last died;
    protected Vector3 spawnPoint; // the spawn point of the craft
    public GameObject explosionCirclePrefab; // prefabs for death explosion
    public GameObject explosionLinePrefab;
    public List<ShellPart> parts; // List containing all parts of the craft
    public CraftBlueprint blueprint; // Default shell configuration of the craft
    public int faction; // What side the craft belongs to (0 = green, 1 = red, 2 = blue...) //TODO: use this to set colors of all parts

    /// <summary>
    /// Get if the craft is dead
    /// </summary>
    /// <returns>true if the craft is dead, false otherwise</returns>
    public bool GetIsDead() {
        return isDead; // is dead
    }

    /// <summary>
    /// Helper method for death animation and state changing
    /// </summary>
    protected void OnDeath() {
        // set death and immobility
        isDead = true;
        isImmobile = true;
        MakeBusy(); // make busy
        deathTimer = 0; // reset death timer
        transform.Find("Minimap Image").GetComponent<SpriteRenderer>().enabled = false; // remove from minimap

        for(int i = 0; i < parts.Count; i++)
        {
            parts[i].SetCollectible((parts[i].name != "Shell Sprite") && Random.Range(0F,5) > 2.5F);
            parts[i].Detach();
        }

        GameObject tmp = Instantiate(explosionCirclePrefab); // instantiate circle explosion
        tmp.transform.SetParent(transform, false);
        Destroy(tmp, 2); // destroy explosions after 2 seconds
        for (int i = 0; i < 3; i++) { // instantiate line explosions
            tmp = Instantiate(explosionLinePrefab);
            tmp.transform.SetParent(transform, false);
            Destroy(tmp, 2); // destroy explosions after 2 seconds
        }
    }

    virtual protected void Awake() {
        // initialize instance fields
        currentHealth = new float[3];
        maxHealth = new float[3];
        regenRate = new float[3];
        isBusy = false;
        respawns = false;
        targeter = new TargetingSystem(transform); // create the associated targeting system for this craft
        isInCombat = false;
    }

    virtual protected void Start()
    {
        GetComponentInChildren<MinimapLockRotationScript>().Initialize(); // initialize the minimap dot
        //transform.rotation = Quaternion.identity; // reset rotation
        GetComponent<SpriteRenderer>().enabled = true; // enable sprite renderer
        busyTimer = 0; // reset busy timer
        BuildShip(); // Generate shell parts around the core
    }

    protected virtual void Update() {
        TickState(); // tick state
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
    /// Called to respawn this craft to its spawn point
    /// </summary>
    protected virtual void Respawn() {
        // no longer dead, busy or immobile
        isDead = false; 
        isBusy = false;
        isImmobile = false;
        transform.rotation = Quaternion.identity; // reset rotation so part rotation can be reset
        foreach (Transform child in transform) { // reset all the children rotations
            child.transform.rotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            var tmp = child.gameObject.GetComponent<ShellPart>(); 
            // will be changed to check for all parts instead of just shell part
            if (tmp) { // if part exists
                tmp.Start(); // initialize it
            }
        }
        Start(); // once everything else is done initialize the craft again
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
            if (deathTimer >= 2 && respawns) 
                // not all craft respawn, but if they do the duration is hardcoded right now
            {
                Respawn(); // respawn
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

    /// <summary>
    /// Generate shell parts in the blueprint, change ship stats accordingly
    /// </summary>
    protected virtual void BuildShip()
    {
        // Remove possible old parts from list
        parts.Clear();
        maxHealth = new float[3];
        // Create shell parts
        if (blueprint != null)
        {
            for (int i = 0; i < blueprint.parts.Count; i++)
            {
                CraftBlueprint.PartInfo part = blueprint.parts[i];

                GameObject obj = Instantiate(part.part);
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
        currentHealth[0] = maxHealth[0];
        currentHealth[1] = maxHealth[1];
        currentHealth[2] = maxHealth[2] = 100;
        regenRate[0] = 100;
        regenRate[2] = 100;
        // Add abilities
        abilities = GetComponentsInChildren<Ability>();
    }

    public void RemovePart(ShellPart part)
    {
        part.GetComponent<Ability>().SetDestroyed(true);
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
    /// The method that moves the craft based on the integer input it receives
    /// Movement tries to emulate original Shellcore Command movement (specifically episode 1) but is not perfect
    /// </summary>
    /// <param name="direction">integer that specifies the direction of movement</param>
    protected void MoveCraft(Vector2 direction)
    {
        if (!isImmobile) // check for immobility
        {
            CraftMover(direction); // if not immobile move craft
        }
    }

    /// <summary>
    /// Rotates the craft to the passed vector
    /// </summary>
    /// <param name="directionVector">direction vector to rotate the craft to</param>
    private void RotateCraft(Vector2 directionVector) {

        //no need to do anything if there's no movement
        if (directionVector == Vector2.zero)
            return;

        //calculate difference of angles and compare them to find the correct turning direction
        float targetAngle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
        float craftAngle = Mathf.Atan2(craftBody.transform.up.y, craftBody.transform.up.x) * Mathf.Rad2Deg;

        float delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));
        bool direction = delta < 90;

        //rotate with physics
        craftBody.transform.Rotate(0, 0, (direction ? 2 : -2) * enginePower / craftBody.mass * Time.deltaTime);

        //check if the angle has gone over the target
        craftAngle = Mathf.Atan2(craftBody.transform.up.y, craftBody.transform.up.x) * Mathf.Rad2Deg;
        delta = Mathf.Abs(Mathf.DeltaAngle(targetAngle - craftAngle, 90));

        if (direction != (delta < 90))
        {
            //if so, set the angle to be exactly the target
            craftBody.transform.eulerAngles = new Vector3(0, 0, targetAngle - 90);
        }
    }

    /// <summary>
    /// Applies a force to the craft on the vector given
    /// </summary>
    /// <param name="directionVector">vector given</param>
    private void CraftMover(Vector2 directionVector)
    {
        RotateCraft(directionVector); // rotate craft
        craftBody.AddForce(enginePower * directionVector); 
        // actual force applied to craft; independent of angle rotation
    }

    /// <summary>
    /// Check if craft is moving
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMoving() {
        return craftBody.velocity != Vector2.zero; // if there is any velocity the craft is moving
    }

    /// <summary>
    /// Removes health from the shell and/or core based on the passed piercing factor and current health
    /// </summary>
    /// <param name="amount">The amount of damage to do</param>
    /// <param name="shellPiercingFactor">The factor of damage that pierces through the shell into the core</param>
    public void TakeDamage(float amount, float shellPiercingFactor) {
        currentHealth[0] -= amount * (1 - shellPiercingFactor); // subtract amount from shell
        if (currentHealth[0] < 0) { // if shell has dipped below 0
            currentHealth[1] += currentHealth[0]; // remove excess from core
            currentHealth[0] = 0; // set shell to zero
        }
        currentHealth[1] -= amount * shellPiercingFactor; // remove the rest of the damage from the core
    }

    /// <summary>
    /// Removes energy from the craft
    /// </summary>
    /// <param name="amount">The amount of energy to remove</param>
    public void TakeEnergy(float amount) {
        currentHealth[2] -= amount; // remove energy
    }
}
