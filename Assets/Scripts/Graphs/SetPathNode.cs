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
        public string entityID = "";
        public bool asynchronous;
        public PathData path = null;

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

            useIDInput = RTEditorGUI.Toggle(useIDInput, "Use ID Input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInput)
                    IDInput = CreateConnectionKnob(IDInStyle);
                else
                    DeleteConnectionPort(IDInput);
            }
            if (!useIDInput)
            {
                GUILayout.Label("Entity ID");
                entityID = GUILayout.TextField(entityID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetEntityID;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            RTEditorGUI.Seperator();

            asynchronous = RTEditorGUI.Toggle(asynchronous, "Asynchronous Mode", GUILayout.MinWidth(400));

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

        void SetEntityID(string ID)
        {
            Debug.Log("selected " + ID + "!");

            entityID = ID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
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
                    entityID = (IDInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("ID Input not connected!");
                }
            }

            Debug.Log("Entity ID: " + entityID);

            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i].ID == entityID && AIData.entities[i] is AirCraft)
                {
                    if (AIData.entities[i] is PlayerCore)
                    {
                        AIData.entities[i].StartCoroutine(pathPlayer(AIData.entities[i] as PlayerCore));
                    }
                    else
                    {
                        if(!asynchronous) (AIData.entities[i] as AirCraft).GetAI().setPath(path, continueTraversing);
                        else (AIData.entities[i] as AirCraft).GetAI().setPath(path);
                    }
                }
            }
            return asynchronous ? 0 : -1;
        }

        private void continueTraversing()
        {
            TaskManager.Instance.setNode(output);
        }

        IEnumerator pathPlayer(PlayerCore player)
        {
            player.SetIsInteracting(false);
            PathData.Node current = GetNode(0);

            while (current != null)
            {
                Vector2 delta = current.position - (Vector2)player.transform.position;
                player.MoveCraft(delta.normalized);
                if (delta.sqrMagnitude < 0.1f)
                {
                    if (current.children.Count > 0)
                    {
                        int next = Random.Range(0, current.children.Count);
                        current = GetNode(current.children[next]);
                    }
                    else
                        current = null;
                }
                yield return null;
            }

            player.SetIsInteracting(true);

            if(!asynchronous) continueTraversing();
        }

        PathData.Node GetNode(int ID)
        {
            for (int i = 0; i < path.waypoints.Count; i++)
            {
                if (path.waypoints[i].ID == ID)
                    return path.waypoints[i];
            }
            return null;
        }
    }
}
