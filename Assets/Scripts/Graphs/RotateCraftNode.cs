using System.Collections;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Rotate Craft")]
    public class RotateCraftNode : Node
    {
        public override string GetName
        {
            get { return "RotateCraftNode"; }
        }

        public override string Title
        {
            get { return "Rotate Craft"; }
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
        public bool asynchronous;
        public string entityID = "";
        public string targetEntityID = "";

        public bool useNumericalAngle = false;
        public string angle;

        public ConnectionKnob RotateInput;
        public ConnectionKnob TargetInput;

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
                if (RotateInput == null)
                {
                    ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Name Input"; });

                    if (input == null)
                    {
                        RotateInput = CreateConnectionKnob(IDInStyle);
                        RotateInput.name = "Name Input";
                    }
                    else
                    {
                        RotateInput = input;
                    }
                }

                RotateInput.DisplayLayout();
            }

            GUILayout.EndHorizontal();
            if (!useIDInput)
            {
                GUILayout.Label("Subject of rotation:");
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

            useIDInput = RTEditorGUI.Toggle(useIDInput, "Get rotator ID from input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useIDInput && RotateInput == null)
                {
                    RotateInput = CreateConnectionKnob(IDInStyle);
                    RotateInput.name = "Name Input";
                }
                else if (!useIDInput && RotateInput != null)
                {
                    DeleteConnectionPort(RotateInput);
                    RotateInput = null;
                }
            }
            RTEditorGUI.Seperator();

            if (!(useNumericalAngle = RTEditorGUI.Toggle(useNumericalAngle, "Use Numerical Angle", GUILayout.MinWidth(400))))
            {
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
                    GUILayout.Label("Target ID:");
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
            else
            {
                DeleteConnectionPort(TargetInput);
                TargetInput = null;
                GUILayout.Label("Angle (Enter number or bad)");
                GUILayout.BeginHorizontal();
                angle = GUILayout.TextField(angle, GUILayout.MinWidth(400));
                GUILayout.EndHorizontal();
            }
            asynchronous = RTEditorGUI.Toggle(asynchronous, "Asynchronous Mode", GUILayout.MinWidth(400));
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

        Entity entity = null;
        Transform target = null;

        public override int Traverse()
        {
            if (useIDInput)
            {
                if (useIDInput && RotateInput == null)
                {
                    RotateInput = inputKnobs[1];
                }

                if (RotateInput.connected())
                {
                    entityID = (RotateInput.connections[0].body as SpawnEntityNode).entityID;
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
                    if (!AIData.entities[i]) continue;
                    if (AIData.entities[i].ID == entityID)
                    {
                        entity = AIData.entities[i];
                    }

                    if (AIData.entities[i].ID == targetEntityID)
                    {
                        target = AIData.entities[i].transform;
                    }
                }
                if (!target)
                {
                    foreach (var flag in AIData.flags)
                    {
                        if (flag.name != targetEntityID) continue;
                        target = flag.transform;
                    }
                }
            }

            if (!useNumericalAngle)
            {
                if (!(target && entity))
                {
                    Debug.LogWarning($"Could not find target/entity! {target} {entity}");
                    return 0;
                }

                Vector2 targetVector = target.position - entity.transform.position;
                //calculate difference of angles and compare them to find the correct turning direction
                if (!(entity is PlayerCore))
                {
                    if (entity is AirCraft airCraft)
                    {
                        if (!asynchronous)
                        {
                            airCraft.GetAI().RotateTo(targetVector, continueTraversing);
                        }
                        else
                        {
                            airCraft.GetAI().RotateTo(targetVector);
                        }
                    }
                    else
                    {
                        entity.transform.RotateAround(entity.transform.position, Vector3.forward, Vector3.SignedAngle(Vector3.up, -targetVector, Vector3.forward));
                        return 0;
                    }
                }
                else
                {
                    entity.StartCoroutine(rotatePlayer(targetVector));
                }
            }
            else
            {
                entity.transform.rotation = Quaternion.Euler(new Vector3(0, 0, float.Parse(angle)));
                return 0;
            }


            return asynchronous ? 0 : -1;
        }

        private void continueTraversing()
        {
            TaskManager.Instance.setNode(output);
        }

        IEnumerator rotatePlayer(Vector2 targetVector)
        {
            var player = (entity as PlayerCore);
            player.SetIsInteracting(false);

            Vector2 normalizedTarget = targetVector.normalized;
            float delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
            while (delta > 0.0001F)
            {
                player.RotateCraft(targetVector);
                delta = Mathf.Abs(Vector2.Dot(player.transform.up, normalizedTarget) - 1f);
                yield return null;
            }

            player.SetIsInteracting(true);

            if (!asynchronous)
            {
                continueTraversing();
            }
        }
    }
}
