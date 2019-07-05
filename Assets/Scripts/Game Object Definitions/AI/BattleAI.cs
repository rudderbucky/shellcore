using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAI : AIModule
{

    enum BattleState
    {
        Attack,
        Defend,
        Collect
    }

    BattleState state = BattleState.Attack;

    List<Entity> carriers;
    ShellCore shellcore;

    float nextSearchTime; //timer to reduce the frequency of heavy search functions
    float nextStateCheckTime; //timer to reduce the frequency of heavy search functions

    Entity primaryTarget;

    EnergyRock collectTarget = null;
    bool findNewTarget = false;

    Draggable waitingDraggable;

    public override void Init()
    {
        carriers = new List<Entity>();

        Entity[] targetEntities = BattleZoneManager.getTargets();
        if(targetEntities == null)
        {
            Debug.LogError("Battle zone target list not initialized");
            ai.setMode(AirCraftAI.AIMode.Inactive);
            return;
        }

        for(int i = 0; i < targetEntities.Length; i++)
        {
            if(targetEntities[i] is ICarrier)
                if (targetEntities[i].faction == craft.faction)
                    carriers.Add(targetEntities[i]);
        }

        if (craft is ShellCore)
            shellcore = craft as ShellCore;
        else
            Debug.LogError("Battle zone AI should only be used by shellcores!");

        nextSearchTime = Time.time;
        nextStateCheckTime = Time.time;
    }

    //TEMP:
    BattleState pState;

    public override void StateTick()
    {
        pState = state;
        if (nextStateCheckTime < Time.time) // Don't change state every tick
        {
            ai.aggression = AirCraftAI.AIAggression.FollowInRange; // usually, allow chasing after enemies

            for (int i = 0; i < carriers.Count; i++)
            {
                if (carriers[i] && carriers[i].GetHealth()[0] < carriers[i].GetMaxHealth()[0] * 0.3f)
                {
                    // if base under attack -> defend
                    if (AirCraftAI.getEnemyCountInRange(carriers[i].transform.position, 45f, craft.faction) > 0)
                    {
                        if ((carriers[i].transform.position - craft.transform.position).sqrMagnitude > 1500f)
                        {
                            ai.aggression = AirCraftAI.AIAggression.KeepMoving; // Get to the home base asap
                            primaryTarget = carriers[i];
                        }
                        else
                        {
                            primaryTarget = null;
                            // TODO: find the most dangerous unit in the area, set as primary target
                        }

                        state = BattleState.Defend;
                    }
                    else
                    {
                        state = BattleState.Collect;
                    }
                    break;
                }
            }

            // if population is nearly capped, attack
            if (shellcore.GetTotalCommandLimit() < shellcore.GetUnitsCommanding().Count + 1 || /*shellcore.GetTractorTarget() != null*/ shellcore.GetPower() >= 200) // TODO: OR if enemy base is weak
            {
                state = BattleState.Attack;
            }
            // if there's no need for more population space, try to create turrets to protect owned outposts and stations
            else
            {
                if(state != BattleState.Collect)
                {
                    collectTarget = null;
                }
                if (shellcore.GetTractorTarget() != null && shellcore.GetTractorTarget().GetComponent<Turret>() != null && shellcore.GetHealth()[0] > shellcore.GetMaxHealth()[0] * 0.3f)
                {
                    state = BattleState.Attack;
                }
                else
                {
                    state = BattleState.Collect;
                }
            }

            nextStateCheckTime = Time.time + 1f;
        }
        if(pState != state)
        {
            //Debug.LogFormat("Faction {0} Shellcore changed state to: {1}", craft.faction, state);
        }
    }

    public override void ActionTick()
    {
        switch (state)
        {
            case BattleState.Attack:
                // go to nearest enemy construct, attack units / turrets if in visual range
                if((primaryTarget == null && nextSearchTime < Time.time) || nextSearchTime < Time.time - 3f)
                {
                    // get nearest construct
                    primaryTarget = AirCraftAI.getNearestEntity<AirConstruct>(craft.transform.position, craft.faction, true); //TODO: Exclude turrets?
                    nextSearchTime = Time.time + 1f;

                    //if(primaryTarget)
                    //    Debug.Log("AggroTarget: " + primaryTarget.name + " Factions: " + primaryTarget.faction + " - " + craft.faction);
                }
                if(primaryTarget != null)
                {
                    craft.MoveCraft((primaryTarget.transform.position - craft.transform.position).normalized);
                }
                //TODO: AI Attack:
                // action sequences
                // Use existing turrets:
                // -Drag torpedo turrets to enemy bunkers
                // -Drag siege turrets to outposts
                // ground attack location = own bunker location
                // drag tanks from one platform to another "LandPlatformGenerator.getEnemiesOnPlatform(Vector2 platformLocation, int faction)"?
                // how to react to different pressures on land and air?
                break;
            case BattleState.Defend:
                // destroy enemy units around base, ignore everything outside siege range
                if(primaryTarget && !primaryTarget.GetIsDead())
                    craft.MoveCraft((primaryTarget.transform.position - craft.transform.position).normalized);
                // buy a turret matching the biggest threat's element, if possible
                break;
            case BattleState.Collect:
                // go from outpost to outpost, (also less fortified enemy outposts [count enemy units nearby {TODO}]) and collect energy
                if (findNewTarget || collectTarget == null)
                {
                     // Find new target
                    float minD = float.MaxValue;
                    EnergyRock nearest = null;
                    for (int i = 0; i < AIData.energyRocks.Count; i++)
                    {
                        if (AirCraftAI.getEnemyCountInRange(AIData.energyRocks[i].transform.position, 15f, craft.faction) > 3)
                            continue;

                        float d = (craft.transform.position - AIData.energyRocks[i].transform.position).sqrMagnitude;
                        if (d < minD && AIData.energyRocks[i] != collectTarget)
                        {
                            minD = d;
                            nearest = AIData.energyRocks[i];
                        }
                    }
                    collectTarget = nearest;

                    if (collectTarget != null)
                        findNewTarget = false;

                    //Debug.LogFormat("Faction {0} collect target: {1}", craft.faction, collectTarget);
                }
                if(collectTarget != null)
                {
                    Vector2 delta = collectTarget.transform.position - craft.transform.position;
                    if (delta.sqrMagnitude > 25f)
                    {
                        craft.MoveCraft(delta.normalized);
                    }
                    else
                    {
                        findNewTarget = true;
                    }
                }
                break;
            default:
                break;
        }

        // always collect energy
        if (shellcore.GetTractorTarget() != null && shellcore.GetTractorTarget().gameObject.GetComponent<EnergySphereScript>() == null)
        {
            {
                for (int i = 0; i < AIData.energySpheres.Count; i++)
                {
                    if ((AIData.energySpheres[i].transform.position - shellcore.transform.position).sqrMagnitude < 150)
                    {
                        waitingDraggable = shellcore.GetTractorTarget();
                        shellcore.SetTractorTarget(null);
                        break;
                    }
                }
            }
        }
        else if (shellcore.GetTractorTarget() == null && waitingDraggable != null)
        {
            bool energyNearby = false;
            for (int i = 0; i < AIData.energySpheres.Count; i++)
            {
                if ((AIData.energySpheres[i].transform.position - shellcore.transform.position).sqrMagnitude < 150)
                {
                    energyNearby = true;
                    break;
                }
            }
            if (!energyNearby)
            {
                shellcore.SetTractorTarget(waitingDraggable);
                waitingDraggable = null;
            }
        }

        // always buy more turrets/tanks
        if (shellcore.unitsCommanding.Count < shellcore.GetTotalCommandLimit())
        {
            for (int i = 0; i < AIData.vendors.Count; i++)
            {
                if ((AIData.vendors[i].transform.position - craft.transform.position).sqrMagnitude < 380f && AIData.vendors[i].faction == craft.faction)
                {
                    IVendor vendor = AIData.vendors[i] as IVendor;

                    if (vendor.GetVendingBlueprint() == null)
                        continue;

                    int itemIndex = -1;

                    if (state == BattleState.Attack)
                    {
                        bool ownGroundExists = false;
                        for (int j = 0; j < AIData.entities.Count; j++)
                        {
                            if (AIData.entities[j].faction == craft.faction && AIData.entities[j].Terrain == Entity.TerrainType.Ground)
                            {
                                ownGroundExists = true;
                                break;
                            }
                        }
                        if (!ownGroundExists && enemyGroundTargets(true) && shellcore.GetPower() >= 150)
                        {
                            // Attack & enemy holds all ground
                            itemIndex = vendor.GetVendingBlueprint().getItemIndex("Torpedo Turret");
                        }
                        else
                        {
                            if (shellcore.GetPower() >= 200)
                            {
                                itemIndex = vendor.GetVendingBlueprint().getItemIndex("Missile Turret");
                            }
                            else if(shellcore.GetPower() >= 100)
                            {
                                itemIndex = vendor.GetVendingBlueprint().getItemIndex("Defense Turret");
                            }
                        }
                    }
                    if (itemIndex == -1)
                    {
                        for (int j = 0; j < vendor.GetVendingBlueprint().items.Count; j++)
                        {
                            if (vendor.GetVendingBlueprint().items[j].cost <= shellcore.GetPower() && shellcore.unitsCommanding.Count < shellcore.GetTotalCommandLimit())
                            {
                                if (itemIndex != -1 && vendor.GetVendingBlueprint().items[j].cost <= vendor.GetVendingBlueprint().items[itemIndex].cost) // more expensive => better (TODO: choose based on the situation)
                                {
                                    continue;
                                }

                                if (vendor.GetVendingBlueprint().items[j].entityBlueprint.intendedType == EntityBlueprint.IntendedType.Tank && !enemyGroundTargets(true)) //TODO: get turret / tank attack category from somewhere else
                                    continue;

                                itemIndex = j;
                            }
                        }
                    }

                    if (itemIndex != -1)
                    {
                        GameObject creation = new GameObject();
                        creation.transform.position = AIData.vendors[i].transform.position;
                        switch (vendor.GetVendingBlueprint().items[itemIndex].entityBlueprint.intendedType)
                        {
                            case EntityBlueprint.IntendedType.Turret:
                                creation.name = "Turret";
                                Turret tur = creation.AddComponent<Turret>();
                                tur.blueprint = vendor.GetVendingBlueprint().items[itemIndex].entityBlueprint;
                                tur.SetOwner(shellcore);
                                tur.faction = craft.faction;
                                shellcore.SetTractorTarget(creation.GetComponent<Draggable>());
                                break;
                            case EntityBlueprint.IntendedType.Tank:
                                creation.name = "Tank";
                                Tank tank = creation.AddComponent<Tank>();
                                tank.blueprint = vendor.GetVendingBlueprint().items[itemIndex].entityBlueprint;
                                tank.enginePower = 250;
                                tank.SetOwner(shellcore);
                                tank.faction = craft.faction;
                                break;
                            default:
                                break;
                        }
                        shellcore.sectorMngr.InsertPersistentObject(vendor.GetVendingBlueprint().items[itemIndex].entityBlueprint.name, creation);
                        creation.GetComponent<Entity>().spawnPoint = AIData.vendors[i].transform.position;
                        shellcore.AddPower(-vendor.GetVendingBlueprint().items[itemIndex].cost);

                        break;
                    }
                }
            }
        }
    }

    bool enemyGroundTargets(bool allEntities)
    {
        Entity[] targets = allEntities ? AIData.entities.ToArray() : BattleZoneManager.getTargets();
        for (int i = 0; i < targets.Length; i++)
        {
            if(targets[i].faction != craft.faction && targets[i].Terrain == Entity.TerrainType.Ground)
            {
                return true;
            }
        }
        return false;
    }
}
