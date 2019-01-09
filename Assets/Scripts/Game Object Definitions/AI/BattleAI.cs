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
    static Construct[] constructs; //needed by all, doesn't change much

    float nextSearchTime; //timer to reduce the frequency of heavy search functions

    Construct primaryTarget;
    Entity secondaryTarget;

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

        if(constructs == null)
            constructs = Object.FindObjectsOfType<Construct>();

        nextSearchTime = Time.time;
    }

    public override void Tick()
    {
        // else fortify

        // if base under attack -> defend
        if(carrier.GetHealth()[0] < carrier.GetMaxHealth()[0] * 0.3f)
        {
            // TODO: check if there are enemies near the base
            state = BattleState.Defend;
            // else: state = fortify
        }
        else if(shellcore.GetTotalCommandLimit() > shellcore.GetUnitsCommanding().Count - 2) // TODO: OR if enemy base is weak
        {
            state = BattleState.Attack;
        }
        else if(shellcore.GetPower() < 100f)
        {
            state = BattleState.Collect;
        }
        else
        {
            state = BattleState.Fortify;
        }

        switch (state)
        {
            case BattleState.Attack:
                // go to nearest enemy construct, attack units / turrets if in visual range
                if(primaryTarget == null && nextSearchTime < Time.time)
                {
                    // get nearest construct
                    primaryTarget = getNearestConstruct(craft.transform.position, craft.faction, true);
                }

                // Finding & moving to secodary target @ AirCraftAI.cs
                ai.aggression = AirCraftAI.AIAggression.FollowInRange;

                //TODO: movement & aggro override

                // create tanks, if bunkers or enemy ground base exists
                // create torpedo turrets if no bunker is under control
                // ground attack location = own bunker location (or tank drop location?)
                // what if there are multiple land masses? how to attack?
                // how to react to different pressures on land and air?

                break;
            case BattleState.Defend:
                // destroy enemy units around base, ignore everything outside siege range
                // buy a turret matching the biggest threat's element, if possible
                break;
            case BattleState.Collect:
                // go from outpost to outpost, (also less fortified enemy outposts [count enemy units nearby]) and collect energy
                // keep an offensive turret in tracktor beam, if possible
                break;
            case BattleState.Fortify:
                // move between own constructs and buy more turrets
                break;
            default:
                break;
        }
    }

    private Construct getNearestConstruct(Vector3 position, int faction, bool enemy)
    {
        float minD = float.MaxValue;
        Construct nearest = null;
        for (int i = 0; i < constructs.Length; i++)
        {
            if ((constructs[i].faction == faction) ^ enemy)
                continue;
            float d = (position - constructs[i].transform.position).sqrMagnitude;
            if(d < minD)
            {
                minD = d;
                nearest = constructs[i];
            }
        }
        nextSearchTime = Time.time + 1f;
        return nearest;
    }

    private Craft getNearestCraft(Vector3 position, int faction, bool enemy)
    {
        float minD = float.MaxValue;
        Craft nearest = null;
        Craft[] crafts = Object.FindObjectsOfType<Craft>();
        for (int i = 0; i < crafts.Length; i++)
        {
            if ((crafts[i].faction == faction) ^ enemy)
                continue;
            float d = (position - crafts[i].transform.position).sqrMagnitude;
            if (d < minD)
            {
                minD = d;
                nearest = crafts[i];
            }
        }
        nextSearchTime = Time.time + 1f;
        return nearest;
    }
}
