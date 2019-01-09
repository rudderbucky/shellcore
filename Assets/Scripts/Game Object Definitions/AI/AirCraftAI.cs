using System.Collections.Generic;
using UnityEngine;

public class AirCraftAI : MonoBehaviour
{
    public enum AIMode
    {
        Follow,
        Path,
        Battle,
        Inactive
    }

    enum AIState
    {
        Inactive,
        Active,
        Fleeing
    }

    public enum AIAggression
    {
        FollowInRange,
        StopToAttack,
        KeepMoving
    }

    private AIMode mode;
    private AIState state;

    public AIAggression aggression;
    public Craft craft;
    public IOwner owner;
    private AIModule module;

    private Entity secondaryTarget; // Overrides curren't module's movement if aggression level allowes that

    bool allowRetreat;
    bool retreating;

    public void setMode(AIMode mode)
    {
        if (mode == this.mode)
            return;

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
                module = null;
                break;
            default:
                break;
        }
        if(module != null)
        {
            module.craft = craft;
            module.owner = owner;
            module.ai = this;
            module.Init();
        }

    }

    public void follow(Transform t = null)
    {
        setMode(AIMode.Follow);
        (module as FollowAI).followTarget = t;
        module.Init();
    }

    public AIMode getMode()
    {
        return mode;
    }

    public void moveToPosition(Vector2 pos)
    {
        setMode(AIMode.Path);
        (module as PathAI).MoveToPosition(pos);
    }

    public void setPath(Path path)
    {
        setMode(AIMode.Path);
        (module as PathAI).setPath(path);
        module.Init();
    }

    private void Start()
    {
        state = AIState.Active;
    }

    public void Init(Craft craft, IOwner owner = null)
    {
        this.owner = owner;
        this.craft = craft;
    }

    private void Update()
    {
        if (!craft.GetIsDead())
        {
            foreach (Ability a in craft.GetAbilities())
            {
                if (a && a is WeaponAbility)
                {
                    (a as WeaponAbility).Tick("");
                }
            }

            if(aggression != AIAggression.KeepMoving)
            {
                //TODO: find target (secondaryTarget), stop or follow, give up if it's outside range
            }

            if (state == AIState.Active)
            {
                if(secondaryTarget == null)
                {
                    module.Tick();

                    if (allowRetreat)
                    {
                        //TODO: check if retreat necessary
                        if (craft.GetHealth()[0] < 0.1f * craft.GetMaxHealth()[0])
                        {
                            state = AIState.Fleeing;
                        }
                    }
                }
            }
            else if (state == AIState.Fleeing)
            {
                //TODO: attempth retreat (retreat module?)
            }
            else
            {
                if(state == AIState.Inactive)
                {
                    module.Init();
                    state = AIState.Active;
                }
            }
        }
        else
        {
            state = AIState.Inactive;
        }
    }
}
