using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAI : AIModule
{

    enum BattleState
    {
        Attack,
        Defend,
        Collect,
        Fortify
    }

    BattleState state = BattleState.Attack;

    AirCarrier carrier;
    ShellCore shellcore;

    float nextSearchTime; //timer to reduce the frequency of heavy search functions
    float nextStateCheckTime; //timer to reduce the frequency of heavy search functions

    Construct primaryTarget;

    List<Outpost> outposts;
    Outpost collectTarget = null;
    bool findNewTarget = false;

    public override void Init()
    {
        AirCarrier[] carriers = Object.FindObjectsOfType<AirCarrier>();

        for(int i = 0; i < carriers.Length; i++)
        {
            if (carriers[i].faction == craft.faction)
                carrier = carriers[i];
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

            if (carrier && carrier.GetHealth()[0] < carrier.GetMaxHealth()[0] * 0.3f)
            {
                // if base under attack -> defend
                if (AirCraftAI.getEnemyCountInRange(carrier.transform.position, 45f, craft.faction) > 0)
                {
                    if ((carrier.transform.position - craft.transform.position).sqrMagnitude > 1500f)
                    {
                        ai.aggression = AirCraftAI.AIAggression.KeepMoving; // Get to the home base asap
                    }

                    state = BattleState.Defend;
                }
                else
                {
                    state = BattleState.Fortify;
                    primaryTarget = carrier;
                }
            }
            // if population is nearly capped, attack
            else if (shellcore.GetTotalCommandLimit() > shellcore.GetUnitsCommanding().Count - 2) // TODO: OR if enemy base is weak
            {
                state = BattleState.Attack;
            }
            // if there's no need for more population space, try to create turrets to protect owned outposts and stations
            else if (shellcore.GetPower() < 100f)
            {
                if(state != BattleState.Collect)
                {
                    collectTarget = null;
                }
                state = BattleState.Collect;
            }
            else
            {
                state = BattleState.Fortify;
            }

            nextStateCheckTime = Time.time + 1f;
        }
        if(pState != state)
        {
            Debug.LogFormat("Faction {0} Shellcore changed state to: {1}", craft.faction, state);
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

                    if(primaryTarget)
                        Debug.Log("AggroTarget: " + primaryTarget.name + " Factions: " + primaryTarget.faction + " - " + craft.faction);
                }
                if(primaryTarget != null)
                {
                    craft.MoveCraft((primaryTarget.transform.position - craft.transform.position).normalized);
                }
                //TODO: AI Attack:
                // create tanks, if bunkers or enemy ground base exists
                // create torpedo turrets if no bunker is under control
                // ground attack location = own bunker location (or tank drop location?)
                // what if there are multiple land masses? how to attack?
                // how to react to different pressures on land and air?
                // keep an offensive turret in tracktor beam, if possible
                break;
            case BattleState.Defend:
                // destroy enemy units around base, ignore everything outside siege range
                if(carrier)
                    craft.MoveCraft((carrier.transform.position - craft.transform.position).normalized);

                // buy a turret matching the biggest threat's element, if possible
                break;
            case BattleState.Collect:
                // go from outpost to outpost, (also less fortified enemy outposts [count enemy units nearby {TODO}]) and collect energy
                if(outposts == null)
                {
                    outposts = new List<Outpost>();
                    foreach (var entity in AirCraftAI.entities)
                    {
                        if (entity is Outpost)
                            outposts.Add(entity as Outpost);
                    }
                }

                if(findNewTarget || collectTarget == null)
                {
                    // Find new target
                    if (collectTarget != null)
                        collectTarget = AirCraftAI.getNearestEntity<Outpost>(carrier != null ? carrier.transform.position : craft.transform.position, craft.faction, false);
                    else
                    {
                        float minD = float.MaxValue;
                        Outpost nearest = null;
                        for (int i = 0; i < outposts.Count; i++)
                        {
                            if (outposts[i].faction != craft.faction && AirCraftAI.getEnemyCountInRange(outposts[i].transform.position, 15f, craft.faction) < 3)
                                continue;

                            float d = (craft.transform.position - outposts[i].transform.position).sqrMagnitude;
                            if (d < minD && outposts[i] != collectTarget)
                            {
                                minD = d;
                                nearest = outposts[i];
                            }
                        }
                        collectTarget = nearest;
                    }

                    if(collectTarget != null)
                        findNewTarget = false;
                }

                if(collectTarget != null)
                {
                    Vector2 delta = collectTarget.transform.position - craft.transform.position;
                    if ((delta).sqrMagnitude < 400f)
                    {
                        craft.MoveCraft(delta.normalized);
                    }
                    else
                    {
                        findNewTarget = true;
                    }
                }
                break;
            case BattleState.Fortify:
                // move between own outposts and buy more turrets
                

                break;
            default:
                break;
        }
    }
}
