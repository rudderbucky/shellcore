using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Set Path")]
    public class SetPathNode : Node
    {
        public override string GetName { get { return "SetPathNode"; } }
        public override string Title { get { return "Set Path"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        //public override Vector2 DefaultSize { get { return new Vector2(200, 240); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        //public bool action;
        public bool useIDInput;
        public bool useCoordinates;
        public string entityName = "";
        public List<string> flagNames;
        public List<Vector2Int> coordinates;
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
                    if (inputKnobs.Count == 0)
                        IDInput = CreateConnectionKnob(IDInStyle);
                    else
                        IDInput = inputKnobs[0];
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
            }

            RTEditorGUI.Seperator();
            pathLoop = RTEditorGUI.Toggle(pathLoop, "Loop path");
            useCoordinates = RTEditorGUI.Toggle(useCoordinates, "Use coordinates");

            if (flagNames == null)
                flagNames = new List<string>();
            if (coordinates == null)
                coordinates = new List<Vector2Int>();


            if(useCoordinates)
            {
                GUILayout.Label("Coordinates:");
                for (int i = 0; i < coordinates.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    coordinates[i] = new Vector2Int(
                        RTEditorGUI.IntField(coordinates[i].x),
                        RTEditorGUI.IntField(coordinates[i].y));

                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                    {
                        coordinates.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Waypoint names:");
                for (int i = 0; i < flagNames.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    flagNames[i] = GUILayout.TextField(flagNames[i]);

                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                    {
                        flagNames.RemoveAt(i);
                        i--;
                        GUILayout.EndHorizontal();
                        continue;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                if (useCoordinates)
                    coordinates.Add(Vector2Int.zero);
                else
                    flagNames.Add("flag");
            }
            GUILayout.EndHorizontal();

        }

        public override int Traverse()
        {
            Path path = CreateInstance<Path>();
            path.waypoints = new List<Path.Node>();
            int count = useCoordinates ? coordinates.Count : flagNames.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = Vector3.zero;
                if (useCoordinates)
                {
                    pos = new Vector3(coordinates[i].x, coordinates[i].y);
                }
                else
                {
                    for (int j = 0; j < AIData.flags.Count; j++)
                    {
                        if (AIData.flags[j].name == flagNames[i])
                        {
                            pos = AIData.flags[j].transform.position;
                            break;
                        }
                    }
                }

                int child = i + 1;
                if (i >= count - 1)
                    child = pathLoop ? 0 : -1;
                path.waypoints.Add(
                        new Path.Node()
                        {
                            position = pos,
                            ID = i,
                            children = new int[] { child }
                        }
                    );
            }

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
