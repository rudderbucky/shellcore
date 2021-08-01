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

        public override string Title
        {
            get { return "Check Entity Existence"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(240, 130); }
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

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Entity ID:");
            entityID = RTEditorGUI.TextField(entityID);
            rangeCheck = RTEditorGUI.Toggle(rangeCheck, "Range check", GUILayout.Width(200f));
            if (rangeCheck)
            {
                distanceFromPlayer = RTEditorGUI.IntField("Distance from player: ", distanceFromPlayer);
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
            foreach (var match in possibleMatches)
            {
                if (rangeCheck)
                {
                    var player = AIData.entities.Find(ent => ent.ID == "player");
                    if ((player.transform.position - match.transform.position).sqrMagnitude > distanceFromPlayer * distanceFromPlayer)
                    {
                        continue;
                    }
                }

                State = ConditionState.Completed;
                connectionKnobs[0].connection(0).body.Calculate();
                return;
            } // TODO: Implement failure when I understand how it works or when Ormanus adds failure if it isn't added

            entity = AIData.entities.Find(e => e.ID == entityID);
            Entity.OnEntitySpawn += GrabEntity;
            if (entity)
                entity.RangeCheckDelegate += RangeCheck;
            flag = AIData.flags.Find(f => f.entityID == entityID);
            if (flag)
                flag.RangeCheckDelegate += RangeCheck;
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
                if (range <= distanceFromPlayer * distanceFromPlayer)
                {
                    State = ConditionState.Completed;
                    connectionKnobs[0].connection(0).body.Calculate();
                }
            }
        }
    }
}
