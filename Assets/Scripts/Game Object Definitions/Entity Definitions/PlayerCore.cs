using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A player ShellCore.
/// </summary>
public class PlayerCore : ShellCore {

    public HUDScript hud;
    public InfoText alerter;
    public PlayerSave cursave;
    public bool loaded;
    private bool isInteracting;
    bool tagToReinitialize;

    public bool GetIsInteracting() {
        return isInteracting;
    }

    public void SetIsInteracting(bool val) {
        isInteracting = val;
    }
    /// <summary>
    /// Respawns the player core, deinitializes the HUD
    /// </summary>
    protected override void Respawn() {
        if(hud) hud.DeinitializeHUD(); // deinitialize HUD
        transform.position = spawnPoint; // reset position to spawn point
        base.Respawn(); // this will reinitialize the HUD
    }
    /// <summary>
    /// The directional driver for the player core, returns a vector based on current inputs
    /// </summary>
    /// <returns>a directional vector based on current inputs</returns>
    public static Vector2 getDirectionalInput()
    {
        //Sum up all inputs
        Vector2 direction = Vector2.zero;
        if (Input.GetKey("w"))
            direction += new Vector2(0, 1);
        if (Input.GetKey("a"))
            direction += new Vector2(-1, 0);
        if (Input.GetKey("s"))
            direction += new Vector2(0, -1);
        if (Input.GetKey("d"))
            direction += new Vector2(1, 0);

        //Send unit vector
        direction.Normalize();

        return direction; // it's not exactly like it was in the original game, but I like it more like this actually
    }

    protected override void Awake()
    {
        base.Awake();
    }
    // Use this for initialization (overrides the other start methods so is always called even by parent method calls)
    protected override void Start () {
        // initialize instance fields
        base.Start();
        // spawnPoint = transform.position = Vector3.zero; // overrides the shellcore spawn point
        if (hud) hud.InitializeHUD(this);
        else
        {
            Camera.main.GetComponent<CameraScript>().Initialize(this);
            GameObject.Find("AbilityUI").GetComponent<AbilityHandler>().Initialize(this);
        } // initialize the HUD
        if(!loaded) {
            LoadSave(cursave);
            loaded = true;
        }
	}

    public void Rebuild() {
        hud.DeinitializeHUD();
        for(int i = 0; i < parts.Count; i++) {
            if(parts[i].gameObject.name != "Shell Sprite")
                Destroy(parts[i].gameObject);
        }
        BuildEntity();
        hud.InitializeHUD(this);
    }

    public void LoadSave(PlayerSave save)
    {
        transform.position = save.position;
        positionBeforeOscillation = transform.position.y;
        if(save.currentHealths.Length < 3)
        {
            maxHealth.CopyTo(currentHealth, 0);
        }
        else
        {
            currentHealth = save.currentHealths;
        }
        for(int i = 0; i < currentHealth.Length; i++) {
            if(currentHealth[i] > maxHealth[i]) currentHealth[i] = maxHealth[i];
        }
    }
    public List<EntityBlueprint.PartInfo> GetInventory() {
        if(cursave != null) return cursave.partInventory;
        else return null; 
    }
    
	// Update is called once per frame
	protected override void Update () {
        isImmobile = isInteracting;
        // call methods
        if(group.sortingOrder < maxAirLayer) // player must always be above other entities
        {
            group.sortingOrder = ++maxAirLayer;
        }
        base.Update(); // base update
        MoveCraft(getDirectionalInput()); // move the craft based on the directional input
	}
}
