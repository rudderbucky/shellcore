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
        public override Vector2 DefaultSize { get { return new Vector2(200, 180); } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } }

        public bool useIDInput;
        public string targetID;
        public int targetCount = 1;
        public int targetFaction = 1;
        public ConnectionKnob IDInput;

        int killCount;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("ID Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public bool nameMode;
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

            if(nameMode = RTEditorGUI.Toggle(nameMode, "Name Mode"))
            {
                GUILayout.Label("Target Name");
                targetID = GUILayout.TextField(targetID);
            }
            else
            {
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
                    if (WorldCreatorCursor.instance != null)
                    {
                        if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                        {
                            WorldCreatorCursor.selectEntity += SetEntityID;
                            WorldCreatorCursor.instance.EntitySelection();
                        }
                    }
                }
            }
            targetCount = RTEditorGUI.IntField("Count: ", targetCount);
            targetFaction = RTEditorGUI.IntField("Faction: ", targetFaction);
        }

        void SetEntityID(string ID)
        {
            Debug.Log("selected ID " + ID + "!");

            targetID = ID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        public void Init(int index)
        {
            if (useIDInput && IDInput == null)
                IDInput = inputKnobs[0];

            killCount = 0;
            Entity.OnEntityDeath += updateState;

            state = ConditionState.Listening;

            if(useIDInput)
            {
                if(IDInput.connected())
                {
                    targetID = (IDInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Name Input not connected!");
                }
            }
        }

        // TODO: Force failed deinit if you pass through some node, this is required for the game since if Aristu dies later and this node is initialized
        // I have no idea what that does
        public void DeInit()
        {
            Entity.OnEntityDeath -= updateState;
            state = ConditionState.Uninitialized;
        }

        void updateState(Entity entity, Entity _)
        {
            if (((!nameMode && entity.ID == targetID) || (nameMode && (entity.entityName == targetID || entity.name == targetID)))
                && entity.faction == targetFaction)
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