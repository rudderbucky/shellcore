using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/DestroyEntities")]
    public class DestroyEntityCondition : Node, ICondition
    {
        public const string ID = "DestroyEntities";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Destroy Entities"; } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        public delegate void UnitDestryedDelegate(Entity entity);
        public static UnitDestryedDelegate OnUnitDestroyed;

        public EntityBlueprint.IntendedType entityType;
        public int targetCount = 1;
        public int targetFaction = 1;

        int killCount;

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public void Init(int index)
        {
            killCount = 0;
            OnUnitDestroyed += updateState;

            state = ConditionState.Listening;
        }

        public void DeInit()
        {
            OnUnitDestroyed -= updateState;
            state = ConditionState.Uninitialized;
        }

        void updateState(Entity entity)
        {
            if (entity.blueprint.intendedType == entityType)
            {
                killCount++;
                if (killCount == targetCount)
                {
                    outputRight.connection(0).body.Calculate();
                }
            }
            state = ConditionState.Completed;
        }
    }
}