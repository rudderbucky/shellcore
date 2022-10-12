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
        public bool stopForceTractor = false;
        public string entityID = "";
        public string targetEntityID = "";

        public ConnectionKnob TractorInput;
        public ConnectionKnob TargetInput;
        public bool disableTractor;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("Name Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();

            stopForceTractor = RTEditorGUI.Toggle(stopForceTractor, "Stop Force Tractor");
            if (GUI.changed)
            {
                if (stopForceTractor && TargetInput != null)
                {
                    DeleteConnectionPort(TargetInput);
                    TargetInput = null;
                }
                else if (!stopForceTractor && useIDInputTarget && TargetInput == null)
                {
                    TargetInput = CreateConnectionKnob(IDInStyle);
                    TargetInput.name = "Target Input";
                }
            }

            RTEditorGUI.Seperator();

            GUILayout.BeginHorizontal();
            if (useIDInput)
            {
                if (TractorInput == null)
                {
                    ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Name Input"; });

                    if (input == null)
                    {
                        TractorInput = CreateConnectionKnob(IDInStyle);
                        TractorInput.name = "Name Input";
                    }
                    else
                    {
                        TractorInput = input;
                    }
                }

                TractorInput.DisplayLayout();
            }

            GUILayout.EndHorizontal();

            if (!useIDInput)
            {
                GUILayout.Label("Tractor ID");
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

            useIDInput = RTEditorGUI.Toggle(useIDInput, "Get tractor ID from input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInput && TractorInput == null)
                {
                    TractorInput = CreateConnectionKnob(IDInStyle);
                    TractorInput.name = "Name Input";
                }
                else if (!useIDInput && TractorInput != null)
                {
                    DeleteConnectionPort(TractorInput);
                    TractorInput = null;
                }
            }

            if (!stopForceTractor)
            {
                RTEditorGUI.Seperator();

                useIDInputTarget = RTEditorGUI.Toggle(useIDInputTarget, "Get target ID from input", GUILayout.MinWidth(400));
                if (GUI.changed)
                {
                    if (useIDInputTarget && TargetInput == null)
                    {
                        TargetInput = CreateConnectionKnob(IDInStyle);
                        TargetInput.name = "Target Input";
                    }
                    else if (!useIDInputTarget && TargetInput != null)
                    {
                        DeleteConnectionPort(TargetInput);
                        TargetInput = null;
                    }
                }

                if (!useIDInputTarget)
                {
                    GUILayout.Label("Target ID");
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

                if (useIDInputTarget)
                {
                    if (TargetInput == null)
                    {
                        ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Target Input"; });

                        if (input == null)
                        {
                            TargetInput = CreateConnectionKnob(IDInStyle);
                            TargetInput.name = "Target Input";
                        }
                        else
                        {
                            TargetInput = input;
                        }
                    }

                    TargetInput.DisplayLayout();
                }
            }
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
                ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Name Input"; });

                if (useIDInput && TractorInput == null)
                {
                    TractorInput = input;
                }

                if (TractorInput.connected())
                {
                    entityID = (TractorInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Tractor name input not connected!");
                }
            }

            if (useIDInputTarget && !stopForceTractor)
            {
                ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Target Input"; });

                if (useIDInputTarget && TargetInput == null)
                {
                    TargetInput = input;
                }

                if (TargetInput.connected())
                {
                    targetEntityID = (TargetInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Target name input not connected!");
                }
            }

            Debug.Log("Entity ID: " + entityID);
            Debug.Log("Target ID: " + targetEntityID);

            if (!(target && entity)) // room for improvement but probably unecessary
            {
                foreach (var ent in AIData.entities)
                {
                    if (ent is AirCraft airCraft)
                    {
                        if (ent.ID == entityID)
                        {
                            entity = airCraft;
                        }
                    }

                    if (ent.ID == targetEntityID)
                    {
                        target = ent;
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
