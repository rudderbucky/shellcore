using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class of every "being" in the game.
/// </summary>
public class Entity : MonoBehaviour {

    /*protected float[] currentHealth; // current health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] maxHealth; // maximum health of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected float[] regenRate; // regeneration rate of the entity (index 0 is shell, index 1 is core, index 2 is energy)
    protected Ability[] abilities; // abilities
    public int enginePower; // entity's engine power, determines how fast it goes
    public Rigidbody2D entityBody; // entity to modify with this script
    public Collider2D hitbox; // the hitbox of the entity (excluding extra parts)
    protected TargetingSystem targeter; // the TargetingSystem of the entity
    protected bool isInCombat; // whether the entity is in combat or not
    protected bool isBusy; // whether the entity is busy or not
    protected bool isDead; // whether the entity is currently dead or not
    protected bool isImmobile; // whether the entity is immobile or not
    protected float busyTimer; // the time since the entity was last set to busy
    protected float combatTimer; // the time since the entity was last set into combat
    protected float deathTimer; // the time since the entity last died;
    public Vector3 spawnPoint; // the spawn point of the entity
    public GameObject explosionCirclePrefab; // prefabs for death explosion
    public GameObject explosionLinePrefab;
    public List<ShellPart> parts; // List containing all parts of the entity
    public CraftBlueprint blueprint; // Default shell configuration of the entity
    public int faction; // What side the entity belongs to (0 = green, 1 = red, 2 = blue...) //TODO: use this to set colors of all parts
                        // Use this for initialization*/
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
