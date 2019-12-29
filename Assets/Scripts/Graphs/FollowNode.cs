using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Follow")]
    public class AIFollowNode : Node
    {
        public override string GetName { get { return "AIFollowNode"; } }
        public override string Title { get { return "AI Follow"; } }
        public override bool AllowRecursion { get { return true; } }
        public override bool AutoLayout { get { return true; } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string followerName = "";
        public string targetName = "";

        public bool useFollowerInput;
        public bool useTargetInput;
        public bool stopFollowing = false;

        public ConnectionKnob FollowerInput;
        public ConnectionKnob TargetInput;

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
                    TargetInput.name = "TargetInput";
                }
            }

            RTEditorGUI.Seperator();

            GUILayout.BeginHorizontal();
            if (useFollowerInput)
            {
                if (FollowerInput == null)
                {
                    ConnectionKnob input = connectionKnobs.Find((x)=> { return x.name == "FollowerInput"; });

                    if (input == null)
                    {
                        FollowerInput = CreateConnectionKnob(IDInStyle);
                        FollowerInput.name = "FollowerInput";
                    }
                    else
                        FollowerInput = input;
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
                        ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "TargetInput"; });

                        if (input == null)
                        {
                            TargetInput = CreateConnectionKnob(IDInStyle);
                            TargetInput.name = "TargetInput";
                        }
                        else
                            TargetInput = input;
                    }
                    TargetInput.DisplayLayout();
                }
                GUILayout.EndHorizontal();
            }


            useFollowerInput = RTEditorGUI.Toggle(useFollowerInput, "Get follower name from input", GUILayout.MinWidth(400));
            if (GUI.changed)
            {
                if (useFollowerInput && FollowerInput == null)
                {
                    FollowerInput = CreateConnectionKnob(IDInStyle);
                    FollowerInput.name = "FollowerInput";
                }
                else if(!useFollowerInput && FollowerInput != null)
                {
                    DeleteConnectionPort(FollowerInput);
                    FollowerInput = null;
                }
            }
            if (!useFollowerInput)
            {
                GUILayout.Label("Follower Name");
                followerName = GUILayout.TextField(followerName);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetFollowerName;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }
            }

            if (!stopFollowing)
            {
                RTEditorGUI.Seperator();

                useTargetInput = RTEditorGUI.Toggle(useTargetInput, "Get target name from input", GUILayout.MinWidth(400));
                if (GUI.changed)
                {
                    if (useTargetInput && TargetInput == null)
                    {
                        TargetInput = CreateConnectionKnob(IDInStyle);
                        TargetInput.name = "TargetInput";
                    }
                    else if(!useTargetInput && TargetInput != null)
                    {
                        DeleteConnectionPort(TargetInput);
                        TargetInput = null;
                    }
                }
                if (!useTargetInput)
                {
                    GUILayout.Label("Target Name");
                    targetName = GUILayout.TextField(targetName);
                    if (WorldCreatorCursor.instance != null)
                    {
                        if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                        {
                            WorldCreatorCursor.selectEntity += SetTargetName;
                            WorldCreatorCursor.instance.EntitySelection();
                        }
                    }
                }
            }
        }

        void SetFollowerName(string newName)
        {
            Debug.Log("selected " + newName + "!");

            followerName = newName;
            WorldCreatorCursor.selectEntity -= SetFollowerName;
        }


        void SetTargetName(string newName)
        {
            Debug.Log("selected " + newName + "!");

            targetName = newName;
            WorldCreatorCursor.selectEntity -= SetTargetName;
        }

        public override int Traverse()
        {
            if (useFollowerInput)
            {
                ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "FollowerInput"; });

                if (useFollowerInput && FollowerInput == null)
                    FollowerInput = input;

                if (FollowerInput.connected())
                {
                    followerName = (FollowerInput.connections[0].body as SpawnEntityNode).entityName;
                }
                else
                {
                    Debug.LogWarning("Follower name input not connected!");
                }
            }

            if (useTargetInput && !stopFollowing)
            {
                ConnectionKnob input = connectionKnobs.Find((x) => { return x.name == "TargetInput"; });

                if (useTargetInput && TargetInput == null)
                    TargetInput = input;

                if (TargetInput.connected())
                {
                    targetName = (TargetInput.connections[0].body as SpawnEntityNode).entityName;
                }
                else
                {
                    Debug.LogWarning("Target name input not connected!");
                }
            }

            Debug.Log("Follower name: " + followerName);
            Debug.Log("Target name: " + targetName);

            if (!stopFollowing)
            {
                Entity target = null;
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (AIData.entities[i].name == targetName)
                    {
                        target = AIData.entities[i];
                    }
                }
                if (target != null)
                {
                    for (int i = 0; i < AIData.entities.Count; i++)
                    {
                        if (AIData.entities[i].name == followerName && AIData.entities[i] is AirCraft)
                        {
                            (AIData.entities[i] as AirCraft).GetAI().follow(target.transform);
                            Debug.Log("Follow...");
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
                    if (AIData.entities[i].name == followerName && AIData.entities[i] is AirCraft)
                    {
                        (AIData.entities[i] as AirCraft).GetAI().follow(null);
                    }
                }
            }

            return 0;
        }
    }
}
