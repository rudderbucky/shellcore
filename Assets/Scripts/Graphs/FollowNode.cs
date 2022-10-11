using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Follow")]
    public class AIFollowNode : Node
    {
        public override string GetName
        {
            get { return "AIFollowNode"; }
        }

        public override string Title
        {
            get { return "AI Follow"; }
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

        public string followerID = "";
        public string targetID = "";

        public bool useFollowerInput;
        public bool useTargetInput;
        public bool stopFollowing = false;

        public ConnectionKnob FollowerInput;
        public ConnectionKnob TargetInput;
        public bool disallowAggression;

        ConnectionKnobAttribute IDInStyle = new ConnectionKnobAttribute("Name Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left);

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();

            stopFollowing = RTEditorGUI.Toggle(stopFollowing, "Stop Following");
            if (GUI.changed)
            {
                if (stopFollowing && TargetInput != null)
                {
                    DeleteConnectionPort(TargetInput);
                    TargetInput = null;
                }
                else if (!stopFollowing && useTargetInput && TargetInput == null)
                {
                    TargetInput = CreateConnectionKnob(IDInStyle);
                    TargetInput.name = "Target Input";
                }
            }

            RTEditorGUI.Seperator();

            GUILayout.BeginHorizontal();
            if (useFollowerInput)
            {
                if (FollowerInput == null)
                {
                    ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Follower Input"; });

                    if (input == null)
                    {
                        FollowerInput = CreateConnectionKnob(IDInStyle);
                        FollowerInput.name = "Follower Input";
                    }
                    else
                    {
                        FollowerInput = input;
                    }
                }

                FollowerInput.DisplayLayout();
            }

            GUILayout.EndHorizontal();

            if (!stopFollowing)
            {
                GUILayout.BeginHorizontal();
                if (useTargetInput)
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

                GUILayout.EndHorizontal();
            }


            useFollowerInput = RTEditorGUI.Toggle(useFollowerInput, "Get follower ID from input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useFollowerInput && FollowerInput == null)
                {
                    FollowerInput = CreateConnectionKnob(IDInStyle);
                    FollowerInput.name = "Follower Input";
                }
                else if (!useFollowerInput && FollowerInput != null)
                {
                    DeleteConnectionPort(FollowerInput);
                    FollowerInput = null;
                }
            }

            if (!useFollowerInput)
            {
                GUILayout.Label("Follower ID");
                followerID = GUILayout.TextField(followerID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetFollowerID;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            if (!stopFollowing)
            {
                RTEditorGUI.Seperator();

                useTargetInput = RTEditorGUI.Toggle(useTargetInput, "Get target ID from input", GUILayout.MinWidth(400));
                if (GUI.changed)
                {
                    if (useTargetInput && TargetInput == null)
                    {
                        TargetInput = CreateConnectionKnob(IDInStyle);
                        TargetInput.name = "Target Input";
                    }
                    else if (!useTargetInput && TargetInput != null)
                    {
                        DeleteConnectionPort(TargetInput);
                        TargetInput = null;
                    }
                }

                if (!useTargetInput)
                {
                    GUILayout.Label("Target ID");
                    targetID = GUILayout.TextField(targetID);
                    if (WorldCreatorCursor.instance != null)
                    {
                        if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                        {
                            WorldCreatorCursor.selectEntity += SetTargetID;
                            WorldCreatorCursor.instance.EntitySelection();
                        }
                    }
                }
            }

            GUILayout.BeginHorizontal();
            disallowAggression = GUILayout.Toggle(disallowAggression, "Disallow Aggression", GUILayout.MinWidth(40));
            GUILayout.EndHorizontal();
        }

        void SetFollowerID(string ID)
        {
            followerID = ID;
            WorldCreatorCursor.selectEntity -= SetFollowerID;
        }

        void SetTargetID(string ID)
        {
            followerID = ID;
            WorldCreatorCursor.selectEntity -= SetTargetID;
        }

        public override int Traverse()
        {
            if (useFollowerInput)
            {
                ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Follower Input"; });

                if (useFollowerInput && FollowerInput == null)
                {
                    FollowerInput = input;
                }

                if (FollowerInput.connected())
                {
                    followerID = (FollowerInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Follower name input not connected!");
                }
            }

            if (useTargetInput && !stopFollowing)
            {
                ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "Target Input"; });

                if (useTargetInput && TargetInput == null)
                {
                    TargetInput = input;
                }

                if (TargetInput.connected())
                {
                    targetID = (TargetInput.connections[0].body as SpawnEntityNode).entityID;
                }
                else
                {
                    Debug.LogWarning("Target name input not connected!");
                }
            }


            if (!stopFollowing)
            {
                Entity target = SectorManager.instance.GetEntity(targetID);
                if (target != null)
                {
                    for (int i = 0; i < AIData.entities.Count; i++)
                    {
                        if (AIData.entities[i].ID == followerID && AIData.entities[i] is AirCraft airCraft)
                        {
                            airCraft.GetAI().follow(target.transform);


                            if (disallowAggression)
                            {
                                airCraft.GetAI().aggression = AirCraftAI.AIAggression.KeepMoving;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Follow target not found!");
                }
            }
            else
            {
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (AIData.entities[i].ID == followerID && AIData.entities[i] is AirCraft airCraft)
                    {
                        airCraft.GetAI().follow(null);
                    }
                }
            }

            return 0;
        }
    }
}
