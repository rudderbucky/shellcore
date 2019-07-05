using NodeEditorFramework.Utilities;
using UnityEngine;

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

        public bool useID;
        public string EntityID;
        public string tag;
        public int targetCount = 1;
        public int targetFaction = 1;


        int killCount;

        [ConnectionKnob("ID Input", Direction.In, "EntityID", NodeSide.Left)]
        public ConnectionKnob IDInput;
        //ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("ID Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            output.DisplayLayout();
            if(useID)
            {
                IDInput.DisplayLayout();
            }
            GUILayout.EndHorizontal();

            useID = RTEditorGUI.Toggle(useID, "Use ID input");

            

        }

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
            if (entity.tag == tag)
            {
                killCount++;
                if (killCount == targetCount)
                {
                    state = ConditionState.Completed;
                    output.connection(0).body.Calculate();
                }
            }
        }
    }
}