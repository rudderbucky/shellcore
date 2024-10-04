using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Entity;

public class AirCraftAI : MonoBehaviour
{
    public enum AIMode
    {
        Follow,
        Path,
        Battle,
        Inactive,
        Tractor
    }

    enum AIState
    {
        Inactive,
        Active,
        Retreating
    }

    public enum AIAggression
    {
        FollowInRange,
        StopToAttack,
        KeepMoving
    }

    [SerializeField]
    private AIMode mode = AIMode.Inactive;
    private AIState state;
    public AIAggression aggression;
    public Craft craft;
    public IOwner owner;
    private AIModule module;
    private Entity aggroTarget; // Overrides current module's movement if aggression level allowes that
    float aggroSearchTimer = 0f;

    public bool allowRetreat;
    const float retreatTreshold = 0.15f;
    float retreatSearchTimer = 0f;
    bool retreatTargetFound = false;
    Vector2 retreatTarget;

    public AIMovement movement;
    public AIAbilityController abilityControl;
    public Dictionary<VendingBlueprint.Item.AIEquivalent, bool> vendingItemEnabled = new Dictionary<VendingBlueprint.Item.AIEquivalent, bool>();

    //public static List<Entity> entities = new List<Entity>();

    public void OnEntityDeath()
    {
        if (module is BattleAI battleAI)
        {
            battleAI.OnEntityDeath();
        }
    }

    public void setMode(AIMode mode)
    {
        // Debug.Log("Mode (" + mode + ") set! (try to reduce these, the AI is initialized each time)");
        if (mode == this.mode)
        {
            return;
        }

        this.mode = mode;

        switch (mode)
        {
            case AIMode.Follow:
                module = new FollowAI();
                break;
            case AIMode.Path:
                module = new PathAI();
                break;
            case AIMode.Battle:
                module = new BattleAI();
                break;
            case AIMode.Inactive:
                if (module is TractorAI tractorAI)
                {
                    tractorAI.StopFollowing();
                    movement.StopMoving();
                }

                module = null;
                break;
            case AIMode.Tractor:
                module = new TractorAI();
                break;
            default:
                break;
        }

        if (module != null)
        {
            module.craft = craft;
            module.owner = owner;
            module.ai = this;
            module.Init();
        }
    }

    public void follow(Transform t = null)
    {
        if (craft is Drone drone && drone.type == DroneType.Worker)
        {
            if (module is TractorAI tai)
                tai.Follow(t);
        }
        else
        {
            setMode(AIMode.Follow);
            (module as FollowAI).followTarget = t;
            module.Init();
        }
    }

    public void ChatOrderStateChange(BattleAI.BattleState state)
    {
        if (module is BattleAI mod)
        {
            mod.OrderModeChange(state);
        }
    }

    // used for party HUD visual
    public string GetPartyBattleStateString()
    {
        if (module is BattleAI battleAI)
        {
            switch (battleAI.GetState())
            {
                case BattleAI.BattleState.Attack:
                    return "ATTACKING";
                case BattleAI.BattleState.Defend:
                    return "DEFENDING";
                case BattleAI.BattleState.Collect:
                    return "COLLECTING POWER";
                case BattleAI.BattleState.Fortify:
                    return "BUILDING TURRETS";
            }
        }
        else if (module is FollowAI)
        {
            return "FOLLOWING";
        }

        return "IDLE";
    }

    public AIMode getMode()
    {
        return mode;
    }

    public void moveToPosition(Vector2 pos)
    {
        if (craft is Drone drone && drone.type == DroneType.Worker && module != null)
        {
            (module as TractorAI).GoTo(pos);
        }
        else
        {
            movement.SetMoveTarget(pos, 4f);
            setMode(AIMode.Inactive);
            //(module as PathAI).MoveToPosition(pos);
        }
    }

    public void setPath(NodeEditorFramework.Standard.PathData data, UnityAction OnPathEnd = null, bool patrolling = false)
    {

        if (data == null || data.waypoints == null || data.waypoints.Count == 0)
        {
            craft.isPathing = false;
            return;
        }


        Path path = ScriptableObject.CreateInstance<Path>();
        path.waypoints = new List<Path.Node>();

        craft.isPathing = true;
        for (int i = 0; i < data.waypoints.Count; i++)
        {
            path.waypoints.Add(new Path.Node()
            {
                ID = data.waypoints[i].ID,
                position = data.waypoints[i].position,
                children = data.waypoints[i].children
            });
        }

        setMode(AIMode.Path);
        (module as PathAI).setPath(path, patrolling);
        if (OnPathEnd == null)
        {
            OnPathEnd = new UnityAction(() => craft.isPathing = false);
        }
        else
        {
            var OldOnPathEnd = OnPathEnd;
            OnPathEnd = new UnityAction(() =>
            {
                OldOnPathEnd.Invoke();
                craft.isPathing = false;
            });
        }

        (module as PathAI).OnPathEnd = OnPathEnd;
    }

    public void setPath(Path path)
    {
        setMode(AIMode.Path);
        (module as PathAI).setPath(path);
        if (module != null)
        {
            module.Init();
        }
    }

    private void Start()
    {
        state = AIState.Active;
    }

    public void Init(Craft craft, IOwner owner = null)
    {
        this.owner = owner;
        this.craft = craft;
        movement = new AIMovement(this);
        movement.SetMoveTarget(craft.transform.position);
        abilityControl = new AIAbilityController(this);
    }

    public void RotateTo(Vector2 targetVector, UnityAction OnRotateEnd = null)
    {
        this.targetVector = targetVector;
        StartCoroutine(RotateCoroutine(OnRotateEnd));
    }

    private Vector2 targetVector;

    IEnumerator RotateCoroutine(UnityAction OnEnd)
    {
        Vector2 normalizedTarget = targetVector.normalized;
        float delta = Mathf.Abs(Vector2.Dot(craft.transform.up, normalizedTarget) - 1f);
        float lastDelta = 0;
        while (delta > 0.0001F)
        {
            craft.RotateCraft(targetVector);
            delta = Mathf.Abs(Vector2.Dot(craft.transform.up, normalizedTarget) - 1f);
            // give up if the rotate didn't do anything
            if (lastDelta == delta) break;
            lastDelta = delta;
            yield return null;
        }

        targetVector = Vector2.zero;

        if (OnEnd != null)
        {
            OnEnd.Invoke();
        }
    }

    float timer = 0.3F;

    private void Update()
    {
        if (!craft.GetIsDead())
        {
            // find target
            Entity target = getNearestEntity<Entity>(craft, true);
            craft.GetTargetingSystem().SetTarget(target ? target.transform : null);

            if (craft.canUseAbilities)
                foreach (Ability a in craft.GetAbilities())
                {
                    if (a)
                    {
                        a.Tick();
                    }
                }

            // shouldn't tick if dead or in cutscene, give control to the cutscene
            if (aggression != AIAggression.KeepMoving && aggroSearchTimer < Time.time && !DialogueSystem.isInCutscene)
            {
                // stop or follow target, give up if it's outside range
                if (target && (target.transform.position - craft.transform.position).sqrMagnitude < 800f)
                {
                    aggroTarget = target;
                    //Debug.Log("AggroTarget: " + aggroTarget.name + " Factions: " + aggroTarget.faction + " - " + craft.faction);
                }

                aggroSearchTimer = Time.time + 1f;
            }

            // shouldn't tick if dead or in cutscene, give control to the cutscene
            if (aggroTarget && !FactionManager.IsAllied(aggroTarget.faction, craft.faction) && !DialogueSystem.isInCutscene)
            {
                if (aggroTarget.GetIsDead() || aggroTarget.IsInvisible ||
                    aggroTarget.GetTerrain() == Entity.TerrainType.Ground && !(craft is Drone drone && drone.type == DroneType.Torpedo))
                {
                    aggroTarget = null;
                    aggroSearchTimer = 0f;
                }
                else
                {
                    // if(craft as Drone && Input.GetKey(KeyCode.D)) Debug.Log(aggroTarget);
                    switch (aggression)
                    {
                        case AIAggression.FollowInRange:
                            var aggroPos = aggroTarget.transform.position;
                            if (timer >= 0.3F)
                            {
                                timer = 0;
                                movement.SetMoveTarget(aggroPos + new Vector3(Random.Range(-4, 4), Random.Range(-4, 4)), 9);
                            }
                            else
                            {
                                timer += Time.deltaTime;
                            }

                            float dist = movement.DistanceToTarget;
                            if (dist > 1000f)
                            {
                                aggroTarget = null;
                            }

                            if (module is FollowAI followAI && followAI.followTarget)
                            {
                                if ((followAI.followTarget.position - craft.transform.position).sqrMagnitude > 150f)
                                {
                                    aggroTarget = null;
                                }
                            }

                            //// Stealth if module is not BattleAI to not interfere with it
                            //if (!(module is BattleAI) && craft.GetHealth()[0] < craft.GetMaxHealth()[0] * 0.25f)
                            //{
                            //    var abilities = craft.GetAbilities();
                            //    if(abilities != null)
                            //    {
                            //        var stealths = abilities.Where((x) => { return (x != null) && x.GetID() == 24; });
                            //        foreach (var stealth in stealths)
                            //        {
                            //            stealth.Tick("activate");
                            //            if (stealth.GetActiveTimeRemaining() > 0)
                            //                break;
                            //        }
                            //    }
                            //}
                            break;
                        case AIAggression.StopToAttack:
                            movement.SetMoveTarget(craft.transform.position);
                            break;
                        case AIAggression.KeepMoving:
                            // Back to module's movement
                            aggroTarget = null;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                aggroTarget = null;
                aggroSearchTimer = 0f;
            }

            if (module != null)
            {
                if (state == AIState.Active)
                {
                    module.StateTick();

                    if (aggroTarget == null)
                    {
                        module.ActionTick();
                    }

                    if (allowRetreat && !(module is FollowAI))
                    {
                        // check if retreat necessary
                        if (craft.GetHealth()[0] < retreatTreshold * craft.GetMaxHealth()[0] 
                            && SectorManager.instance.GetCurrentType() != Sector.SectorType.BattleZone
                            && SectorManager.instance.GetCurrentType() != Sector.SectorType.SiegeZone)
                        {
                            state = AIState.Retreating;
                            //Debug.Log(craft.name + "[ " + craft.faction + " ] retreating!");
                        }
                    }
                }
                else if (state == AIState.Retreating)
                {
                    if ((!retreatTargetFound && retreatSearchTimer < Time.time) || retreatSearchTimer < Time.time)
                    {
                        Entity enemy = getNearestEntity<Entity>(craft, true);
                        if (enemy && (enemy.transform.position - craft.transform.position).sqrMagnitude < 1600f)
                        {
                            retreatTarget = craft.transform.position + (craft.transform.position - enemy.transform.position).normalized * 20f;

                            //Debug.Log(craft.name + "[ " + craft.faction + " ] Initial retreat target : " + retreatTarget);

                            IntRect bounds = SectorManager.instance.current.bounds;
                            if (!bounds.contains(retreatTarget))
                            {
                                retreatTarget = craft.spawnPoint;
                                //Debug.Log(craft.name + "[ " + craft.faction + " ] Updated to Spawn point : " + retreatTarget);
                            }

                            // stay inside sector (just in case the spawn point is outside)
                            if (retreatTarget.x > bounds.x + bounds.w)
                            {
                                retreatTarget = new Vector2(bounds.x + bounds.w, retreatTarget.y);
                            }

                            if (retreatTarget.x < bounds.x)
                            {
                                retreatTarget = new Vector2(bounds.x, retreatTarget.y);
                            }

                            if (retreatTarget.y < bounds.y - bounds.h)
                            {
                                retreatTarget = new Vector2(retreatTarget.x, bounds.y - bounds.h);
                            }

                            if (retreatTarget.y > bounds.y)
                            {
                                retreatTarget = new Vector2(retreatTarget.x, bounds.y);
                            }

                            retreatTargetFound = true;
                            //Debug.Log(craft.name + "[ " + craft.faction + " ] Sector bounds: " + bounds.x + ", " + bounds.y + ", " + bounds.w + ", " + bounds.h);
                            //Debug.Log(craft.name + "[ " + craft.faction + " ] Found a retreat target! : " + retreatTarget);
                        }
                        else
                        {
                            retreatTarget = craft.spawnPoint;
                            retreatTargetFound = true;
                        }

                        retreatSearchTimer = Time.time + 3.0f;
                    }
                    else if (retreatTargetFound)
                    {
                        movement.SetMoveTarget(retreatTarget);
                        if (movement.targetIsInRange() && retreatTarget != (Vector2)craft.spawnPoint)
                        {
                            retreatSearchTimer = 0f;
                        }
                    }
                    else
                    {
                        movement.SetMoveTarget(craft.spawnPoint);
                    }

                    // check if retreating is still necessary
                    if (craft.GetHealth()[0] > retreatTreshold * craft.GetMaxHealth()[0])
                    {
                        state = AIState.Active;
                        //Debug.Log(craft.name + "[ " + craft.faction + " ] stopped retreating!");
                    }
                }
                else
                {
                    if (state == AIState.Inactive)
                    {
                        module.Init();
                        state = AIState.Active;
                    }
                }
            }

            //else
            //{
            //    state = AIState.Inactive;
            //}
            movement.Update();
            abilityControl.Update();
        }
    }

    public static T getNearestEntity<T>(Entity craft, bool enemy = true) where T : Entity
    {
        EntityFaction faction = craft.faction;
        Entity.TerrainType terrainType = craft.Terrain;
        if (craft is Drone drone)
        {
            var type = drone.type;
            if (type == DroneType.Torpedo)
            {
                terrainType = Entity.TerrainType.Ground;
            }
        }

        float minD = float.MaxValue;
        T nearest = null;
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i] == craft)
            {
                continue;
            }

            if (AIData.entities[i].GetIsDead())
            {
                continue;
            }

            if (terrainType != Entity.TerrainType.All && AIData.entities[i].Terrain != terrainType)
            {
                continue;
            }

            if (AIData.entities[i].IsInvisible)
            {
                continue;
            }

            if (AIData.entities[i] is T)
            {
                if ((FactionManager.IsAllied(AIData.entities[i].faction, faction) ^ !enemy) && faction.factionID != -1)
                {
                    continue;
                }

                float d = (craft.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                if (d < minD)
                {
                    minD = d;
                    nearest = AIData.entities[i] as T;
                }
            }
        }

        return nearest;
    }

    public static int getEnemyCountInRange(Vector3 position, float range, int faction)
    {
        int count = 0;
        float sqrRange = range * range;
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (AIData.entities[i].GetIsDead())
            {
                continue;
            }

            if ((position - AIData.entities[i].transform.position).sqrMagnitude < sqrRange)
            {
                count++;
            }
        }

        return count;
    }
}
