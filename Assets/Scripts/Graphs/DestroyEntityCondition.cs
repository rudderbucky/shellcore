using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEntityCondition : TaskCondition
{
    public delegate void UnitDestryedDelegate(Entity entity);
    public static UnitDestryedDelegate OnUnitDestroyed;

    readonly EntityBlueprint.IntendedType entityType;
    int killCount;
    int targetCount;
    int targetFaction;

    DestroyEntityCondition(EntityBlueprint.IntendedType type, int count = 0, int faction = 1)
    {
        entityType = type;
        targetCount = count;
        targetFaction = faction;
        killCount = 0;

        //Check if the task is already completed
        var sm = GameObject.Find("SectorManager").GetComponent<SectorManager>();
        if (sm.current.sectorName == sectorName)
        {
            int fullAmount = 0;
            // Get what's loaded from sector data
            // Get what exists
            // Substract
            // How to recognize units that came in from other sectors? FACTION!

            for (int i = 0; i < sm.current.entities.Length; i++)
            {
                if (sm.current.entities[i].faction == targetFaction)
                {
                    //Get asset

                }
            }
        }
    }

    public override void Init(NodeEditorFramework.Standard.StartTaskNode node)
    {
        Node = node;
        OnUnitDestroyed += updateState;
    }

    public override void DeInit()
    {
        OnUnitDestroyed -= updateState;
    }

    void updateState(Entity entity)
    {
        if (entity.blueprint.intendedType == entityType)
        {
            killCount++;
            if (killCount == targetCount)
            {
                
            }
        }
    }
}