using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Force Tractor")]
    public class ForceTractorNode : Node
    {
        public override string GetName
        {
            get { return "ForceTractorNode"; }
        }

        public override string Title
        {
            get { return "Force Tractor"; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        //public bool action; //TODO: action input
        public bool useIDInput;
        public bool useIDInputTarget;
        public string entityID = "";
        public string targetEntityID = "";

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
                    {
                        IDInput = CreateConnectionKnob(IDInStyle);
                    }
                    else
                    {
                        IDInput = inputKnobs[1];
                    }
                }

                IDInput.DisplayLayout();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Subject to tractor");
            GUILayout.EndHorizontal();
            useIDInput = RTEditorGUI.Toggle(useIDInput, "Use Name Input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInput)
                {
                    IDInput = CreateConnectionKnob(IDInStyle);
                }
                else
                {
                    DeleteConnectionPort(IDInput);
                }
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target for tractor");
            GUILayout.EndHorizontal();
            useIDInputTarget = RTEditorGUI.Toggle(useIDInputTarget, "Use Name Input (Unfinished, don't use)", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInputTarget)
                {
                    IDInput = CreateConnectionKnob(IDInStyle);
                }
                else
                {
                    DeleteConnectionPort(IDInput);
                }
            }

            if (!useIDInputTarget)
            {
                GUILayout.Label("Entity ID");
                targetEntityID = GUILayout.TextField(targetEntityID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetTargetID;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            RTEditorGUI.Seperator();
        }

        void SetEntityID(string ID)
        {
            Debug.Log($"selected ID {ID}!");

            entityID = ID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        void SetTargetID(string ID)
        {
            Debug.Log($"selected ID {ID}!");

            targetEntityID = ID;
            WorldCreatorCursor.selectEntity -= SetTargetID;
        }

        AirCraft entity = null;
        Entity target = null;

        public override int Traverse()
        {
            if (useIDInput)
            {
                if (useIDInput && IDInput == null)
                {
                    IDInput = inputKnobs[1];
                }

                if (IDInput.connected())
                {
                    entityID = (IDInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Name Input not connected!");
                }
            }

            Debug.Log("Entity ID: " + entityID);
            Debug.Log("Target ID: " + targetEntityID);

            if (!(target && entity)) // room for improvement but probably unecessary
            {
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (AIData.entities[i] is AirCraft airCraft)
                    {
                        if (AIData.entities[i].ID == entityID)
                        {
                            entity = airCraft;
                        }

                        if (AIData.entities[i].ID == targetEntityID)
                        {
                            target = AIData.entities[i];
                        }
                    }
                }
            }

            //Debug.LogError(target + " " + entity);

            if (entity && entity.GetComponent<TractorBeam>() && target)
            {
                entity.GetComponentInChildren<TractorBeam>().ForceTarget(target.transform);
            }
            else if (entity && entity.GetComponent<TractorBeam>())
            {
                entity.GetComponentInChildren<TractorBeam>().ForceTarget(null);
            }
            else
            {
                Debug.LogError(entity + " " + entity.GetComponentInChildren<TractorBeam>());
            }

            return 0;
        }
    }
}
