using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Crafts that are air-borne are known as Aircrafts. These crafts bob up and down in the air when they are not moving, and can't normally attack
/// ground-based targets.
/// </summary>
public abstract class AirCraft : Craft
{
    protected float timePassed; // float that stores the time passed since the last aircraft movement, used for idle oscillation and perhaps more down the line
    protected Vector2 oscillatorVector; // vector used to oscillate the aircraft during idle time
    protected float positionBeforeOscillation; // used for oscillation y-coordination resetting
    protected Vector2 storedPos; // position of aircraft before it stopped, used to reset the aircraft's position after oscillation
    protected AirCraftAI ai; // AI agent that controls this aircraft
    private bool oscillating;
    public GameObject energySpherePrefab;

    // This method returns the last position the player was not oscillating from.
    public Vector2 GetSectorPosition()
    {
        if (oscillating)
        {
            var vec = entityBody.position;
            vec.y = positionBeforeOscillation;
            return vec;
        }
        else return transform.position;
    }

    protected override void FixedUpdate()
    {
        if (!SystemLoader.AllLoaded) return;
        base.FixedUpdate();
        Oscillator();
    }

    protected override void Start()
    {
        // initialize instance fields
        storedPos = spawnPoint;
        positionBeforeOscillation = storedPos.y;
        Terrain = TerrainType.Air;
        energySpherePrefab = ResourceManager.GetAsset<GameObject>("energy_sphere");
        base.Start(); // base start
    }

    protected override void OnDeath()
    {
        var lettingServerDecide = MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
        if (!FactionManager.IsAllied(0, faction.factionID) && Random.Range(0, 1F) <= 0.2F && !lettingServerDecide && !DialogueSystem.isInCutscene)
        {
            var x = Instantiate(energySpherePrefab, transform.position, Quaternion.identity);

            float dir = Random.Range(0f, 2 * Mathf.PI);
            x.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sin(dir), Mathf.Cos(dir)) * Random.Range(180f, 240f));
        }

        base.OnDeath();
    }

    /// <summary>
    /// Helper for oscillator
    /// </summary>
    protected void Oscillator()
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton.IsClient)
        {
            oscillating = false;
            return;
        }
        if (isDead)
        {
            // if aircraft is dead
            // reset all fields
            storedPos = spawnPoint;
            timePassed = 0;
            positionBeforeOscillation = spawnPoint.y;
            oscillating = false;
            return;
        }

        if (IsMoving() || restAccel != Vector2.zero) // if core is supposed to be moving 
        {
            if (timePassed != 0)
            {
                // need to reset the position due to the oscillator
                storedPos = entityBody.position;
                storedPos.y = positionBeforeOscillation;
                entityBody.position = storedPos;
            }

            timePassed = 0; // reset time passed
            oscillating = false;
        }
        else if ((!draggable || !draggable.Dragging) && entityBody.velocity.y == 0)
        {
            // idle oscillation time
            if (!oscillating)
            {
                positionBeforeOscillation = entityBody.position.y;
            }
            oscillating = true;
            OscillatorFunction();
        }
    }

    /// <summary>
    /// Idle oscillation animation
    /// </summary>
    private void OscillatorFunction()
    {
        oscillatorVector = entityBody.position; // get the current aircraft position
        oscillatorVector.y = Mathf.Min(oscillatorVector.y - 0.005F * Mathf.Sin(timePassed + entityBody.position.x), positionBeforeOscillation); // cool math stuff
        oscillatorVector.y = Mathf.Max(oscillatorVector.y, positionBeforeOscillation - 1); // more cool math stuff
        entityBody.position = oscillatorVector; // set the aircraft position
        timePassed = timePassed + Time.deltaTime; // add to time so sin oscillates (this will start at zero the moment this loop begins)
    }

    public AirCraftAI GetAI()
    {
        if (ai == null && !(this is PlayerCore))
        {
            ai = gameObject.AddComponent<AirCraftAI>();
            ai.Init(this);
        }

        return ai;
    }

    // NEVER directly use the transform to teleport AirCraft! Use this method instead.
    public virtual void Warp(Vector3 point, bool setWarpUninteractable = true)
    {
        transform.position = point;
        if (!(this as PlayerCore)) spawnPoint = point;
        oscillatorVector = point;
        storedPos = point;
        positionBeforeOscillation = point.y;
        if (ai && ai.movement != null)
        {
            ai.movement.SetMoveTarget(null);
        }
        if (setWarpUninteractable)
            SetWarpUninteractable();
    }

    protected override void CraftMover(Vector2 directionVector)
    {
        base.CraftMover(directionVector);

        if (directionVector != Vector2.zero) // if core is supposed to be moving 
        {
            if (timePassed != 0)
            {
                // need to reset the position due to the oscillator
                storedPos = entityBody.position;
                storedPos.y = positionBeforeOscillation;
                entityBody.position = storedPos;
            }

            timePassed = 0; // reset time passed
        }
    }

    public bool GetIsOscillating()
    {
        return oscillating;
    }
}
