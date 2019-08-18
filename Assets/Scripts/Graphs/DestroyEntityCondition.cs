using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/DestroyEntities")]
    public class DestroyEntityCondition : Node, ICondition
    {
        public const string ID = "DestroyEntities";
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Destroy Entities"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 142); } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        public delegate void UnitDestryedDelegate(Entity entity);
        public static UnitDestryedDelegate OnUnitDestroyed;

        public bool useIDInput;
        public string targetName;
        public int targetCount = 1;
        public int targetFaction = 1;
        public ConnectionKnob IDInput;

        int killCount;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("Name Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

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

            useIDInput = RTEditorGUI.Toggle(useIDInput, "Use Name input");
            if (GUI.changed)
            {
                if (useIDInput)
                    IDInput = CreateConnectionKnob(IDInStyle);
                else
                    DeleteConnectionPort(IDInput);
            }
            if (!useIDInput)
            {
                GUILayout.Label("Target Name");
                targetName = GUILayout.TextField(targetName);
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
                    targetName = (IDInput.connections[0].body as SpawnEntityNode).entityName;
                }
                else
                {
                    Debug.LogWarning("Name Input not connected!");
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
            if (entity.name == targetName)
            {
                killCount++;
                if(targetFaction != 0)
                {
                    SectorManager.instance.player.alerter.showMessage("ENEMIES DESTROYED: " + killCount + " / " + targetCount, "clip_victory");
                }
                else
                {
                    SectorManager.instance.player.alerter.showMessage("ALLIES DEAD: " + killCount + " / " + targetCount, "clip_alert");
                }
                if (killCount == targetCount)
                {
                    state = ConditionState.Completed;
                    output.connection(0).body.Calculate();
                }
            }
        }
    }
}