using System.Collections.Generic;
using UnityEngine;

public class ChainBeam : Beam
{
    int numShots = 0;
    private static readonly int MAX_BOUNCES = 3;

    protected override void Awake()
    {
        base.Awake();
        ID = AbilityID.ChainBeam;
        range = 15;
        abilityName = "Chain";
        description = $"Instant attack that deals {damage} damage to multiple targets."
        energyCost = 225;
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (!firing)
        {
            numShots = 0;
            return;
        }
        if (timer > 0.1 * numShots && numShots < MAX_BOUNCES)
        {
            var vec = line.GetPosition(numShots);
            var ents = GetClosestTargets(MAX_BOUNCES, vec);
            Transform closestEntity = null;
            foreach (var ent in ents)
            {
                if (targetArray.Contains(ent))
                {
                    continue;
                }
                closestEntity = ent;
                break;
            }

            if (!closestEntity)
            {
                firing = false;
            }
            else
            {
                targetArray.Add(closestEntity);
                FireBeam(closestEntity.position);
                numShots++;
            }
        }

        for (int i = 0; i < numShots; i++)
        {
            RenderBeam(i);
        }
    }


    protected override bool Execute(Vector3 victimPos)
    {
        base.Execute(victimPos);
        numShots++;
        return true;
    }

    protected override List<Transform> GetClosestTargets(int num, Vector3 pos)
    {
        var list = base.GetClosestTargets(num, pos);
        if (list.Count > 0)
        {
            foreach (var ent in list)
            {
                if (targetArray.Contains(ent)) continue;
                GetClosestPart(pos, ent.GetComponentsInChildren<ShellPart>());
                break;
            }
        }
        return list;
    }
}
