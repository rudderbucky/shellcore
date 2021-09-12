using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using System.Linq;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Check Entity Existence")]
    public class CheckEntityCondition : Node, ICondition
    {
        public const string ID = "CheckEntityCondition";

        public override string GetName
        {
            get { return ID; }
        }
        public override bool AutoLayout
        {
            get { return true; }
        }


        public override string Title
        {
            get { return "Check Entity Existence"; }
        }


        public ConditionState state; // Property can't be serialized -> field

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public string entityID;
        public bool rangeCheck;
        public int distanceFromPlayer;
        public bool lessThan = true;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Entity ID:");
            entityID = RTEditorGUI.TextField(entityID);
            rangeCheck = RTEditorGUI.Toggle(rangeCheck, "Range check from player", GUILayout.Width(200f));
            if (rangeCheck)
            {
                distanceFromPlayer = RTEditorGUI.IntField("Distance: ", distanceFromPlayer);
                lessThan = GUILayout.SelectionGrid(lessThan ? 0 : 1, new string[] { "Less", "Greater" }, 1) == 0;
            }

        }

        public void DeInit()
        {
            State = ConditionState.Uninitialized;
            if (Entity.OnEntitySpawn != null)
            {
                Entity.OnEntitySpawn -= GrabEntity;
            }
            if (entity)
            {
                entity.RangeCheckDelegate -= RangeCheck;
            }
            if (flag)
            {
                flag.RangeCheckDelegate -= RangeCheck;
            }
        }

        private Entity entity;
        private Flag flag;

        public void Init(int index)
        {
            // TODO: Disambiguate name and entityName
            List<GameObject> possibleMatches = new List<GameObject>();
            AIData.entities.FindAll(ent => ent.ID == entityID && !ent.GetIsDead()).ForEach(x => possibleMatches.Add(x.gameObject));
            AIData.flags.FindAll(f => f.entityID == entityID).ForEach(f => possibleMatches.Add(f.gameObject));

            entity = AIData.entities.Find(e => e.ID == entityID);
            Entity.OnEntitySpawn += GrabEntity;
            if (entity)
                entity.RangeCheckDelegate += RangeCheck;
            flag = AIData.flags.Find(f => f.name == entityID);
            if (flag)
            {
                flag.RangeCheckDelegate += RangeCheck;
            }

        }

        private void GrabEntity(Entity entity)
        {
            if (!entity && entity.ID == entityID)
            {
                this.entity = entity;
                entity.RangeCheckDelegate += RangeCheck;
            }
        }

        private void RangeCheck(float range)
        {
            if (rangeCheck)
            {
                var player = AIData.entities.Find(ent => ent.ID == "player");
                var diff = range - distanceFromPlayer * distanceFromPlayer;
                if ((lessThan && diff <= 0) || (!lessThan && diff > 0))
                {
                    State = ConditionState.Completed;
                    connectionKnobs[0].connection(0).body.Calculate();
                }
            }
        }
    }
}
