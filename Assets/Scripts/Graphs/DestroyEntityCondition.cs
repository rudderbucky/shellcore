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
        public override Vector2 DefaultSize { get { return new Vector2(200, 142); } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        public delegate void UnitDestryedDelegate(Entity entity);
        public static UnitDestryedDelegate OnUnitDestroyed;

        public bool useIDInput;
        public string targetID;
        public int targetCount = 1;
        public int targetFaction = 1;
        public ConnectionKnob IDInput;

        int killCount;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("ID Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            if(useIDInput)
            {
                if(IDInput == null)
                {
                    if (inputKnobs.Count == 0)
                        IDInput = CreateConnectionKnob(IDInStyle);
                    else
                        IDInput = inputKnobs[0];
                }
                IDInput.DisplayLayout();
            }
            output.DisplayLayout();
            GUILayout.EndHorizontal();

            useIDInput = RTEditorGUI.Toggle(useIDInput, "Use ID input");
            if (GUI.changed)
            {
                if (useIDInput)
                    IDInput = CreateConnectionKnob(IDInStyle);
                else
                    DeleteConnectionPort(IDInput);
            }
            if (!useIDInput)
            {
                GUILayout.Label("Target ID");
                targetID = GUILayout.TextField(targetID);
            }
            targetCount = RTEditorGUI.IntField("Count: ", targetCount);
            targetFaction = RTEditorGUI.IntField("Faction: ", targetFaction);
        }

        public void Init(int index)
        {
            if (useIDInput && IDInput == null)
                IDInput = inputKnobs[0];

            killCount = 0;
            OnUnitDestroyed += updateState;

            state = ConditionState.Listening;

            if(useIDInput)
            {
                if(IDInput.connected())
                {
                    targetID = (IDInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("ID Input not connected!");
                }
            }
        }

        public void DeInit()
        {
            OnUnitDestroyed -= updateState;
            state = ConditionState.Uninitialized;
        }

        void updateState(Entity entity)
        {
            if (entity.ID == targetID)
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