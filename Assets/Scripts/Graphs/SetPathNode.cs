using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Set Path")]
    public class SetPathNode : Node
    {
        public override string GetName { get { return "SetPathNode"; } }
        public override string Title { get { return "Set Path"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        //public bool action; //TODO: action input
        public bool useIDInput;
        public bool useCoordinates;
        public string entityName = "";
        public PathData path = null;
        public bool pathLoop = false;

        public ConnectionKnob IDInput;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("Name Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (useIDInput)
            {
                if (IDInput == null)
                {
                    if (inputKnobs.Count == 1)
                        IDInput = CreateConnectionKnob(IDInStyle);
                    else
                        IDInput = inputKnobs[1];
                }
                IDInput.DisplayLayout();
            }
            GUILayout.EndHorizontal();

            useIDInput = RTEditorGUI.Toggle(useIDInput, "Use Name input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInput)
                    IDInput = CreateConnectionKnob(IDInStyle);
                else
                    DeleteConnectionPort(IDInput);
            }
            if (!useIDInput)
            {
                GUILayout.Label("Entity Name");
                entityName = GUILayout.TextField(entityName);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetEntityName;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            RTEditorGUI.Seperator();

            if (GUILayout.Button("Draw Path", GUILayout.ExpandWidth(false)))
            {
                if (path == null)
                {
                    path = new PathData();
                    path.waypoints = new List<PathData.Node>();
                }
                WorldCreatorCursor.finishPath += SetPath;
                WorldCreatorCursor.instance.pathDrawing(path);
            }
        }

        void SetEntityName(string newName)
        {
            Debug.Log("selected " + newName + "!");

            entityName = newName;
            WorldCreatorCursor.selectEntity -= SetEntityName;
        }

        void SetPath(PathData path)
        {
            this.path = path;
        }

        public override int Traverse()
        {
            if (useIDInput)
            {
                if (useIDInput && IDInput == null)
                    IDInput = inputKnobs[1];

                if (IDInput.connected())
                {
                    entityName = (IDInput.connections[0].body as SpawnEntityNode).entityName;
                }
                else
                {
                    Debug.LogWarning("Name Input not connected!");
                }
            }

            Debug.Log("Entity name: " + entityName);

            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i].name == entityName && AIData.entities[i] is AirCraft)
                {
                    (AIData.entities[i] as AirCraft).GetAI().setPath(path);
                }
            }
            return 0;
        }
    }
}
