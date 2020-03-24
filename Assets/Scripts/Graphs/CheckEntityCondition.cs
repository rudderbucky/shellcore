using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Check Entity")]
    public class CheckEntityCondition : Node, ICondition
    {
        public const string ID = "CheckEntityCondition";
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Check Entity"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 120); } }

        public ConditionState state; // Property can't be serialized -> field
        public ConditionState State { get { return state; } set { state = value; } }

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public string entityID;
        bool rangeCheck;
        int distanceFromPlayer;


         public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Entity ID:");
            entityID = RTEditorGUI.TextField(entityID);
            rangeCheck = RTEditorGUI.Toggle(rangeCheck, "Range check", GUILayout.Width(200f));
            if(rangeCheck)
            {
                distanceFromPlayer = RTEditorGUI.IntField("Distance (squared): ", distanceFromPlayer);
            }
        }

        public void DeInit()
        {
            State = ConditionState.Uninitialized;
        }

        public void Init(int index)
        {
            // TODO: Disambiguate name and entityName
            var possibleMatches = AIData.entities.FindAll(ent => ent.ID == entityID && !ent.GetIsDead());
            foreach(var match in possibleMatches)
            {
                if(rangeCheck)
                {
                    var player = AIData.entities.Find(ent => ent.ID == "player");
                    if((player.transform.position - match.transform.position).sqrMagnitude > distanceFromPlayer) continue;
                }
                State = ConditionState.Completed;
                connectionKnobs[0].connection(0).body.Calculate();
                break;
            } // TODO: Implement failure when I understand how it works or when Ormanus adds failure if it isn't added
        }


    }
}

