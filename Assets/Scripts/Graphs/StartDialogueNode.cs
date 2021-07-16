using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Start Dialogue")]
    public class StartDialogueNode : Node
    {
        public static StartDialogueNode missionCanvasNode = null;
        public static StartDialogueNode dialogueCanvasNode = null;

        public override string GetName
        {
            get { return "StartDialogueNode"; }
        }

        public override string Title
        {
            get { return "Start Dialogue"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        public bool SpeakToEntity;
        public string EntityID;
        public bool forceStart;
        public string originSector;
        public ConnectionKnob flowOutput;
        ConnectionKnobAttribute inputInStyle = new ConnectionKnobAttribute("Input Left", Direction.In, "TaskFlow", NodeSide.Left);
        ConnectionKnobAttribute flowInStyle = new ConnectionKnobAttribute("ContinueAsync", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 20);
        public bool allowAfterSpeaking;

        public NodeEditorGUI.NodeEditorState state;

        public override void NodeGUI()
        {
            if (NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Dialogue)
            {
                DeleteConnectionPort(input);
                input = null;
            }
            else if (NodeEditorGUI.state != NodeEditorGUI.NodeEditorState.Dialogue && input == null)
            {
                input = CreateConnectionKnob(inputInStyle);
            }

            SpeakToEntity = RTEditorGUI.Toggle(SpeakToEntity, "Speak to entity");
            if (SpeakToEntity)
            {
                GUILayout.Label("Entity ID");
                EntityID = GUILayout.TextField(EntityID);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetEntityID;
                        WorldCreatorCursor.instance.EntitySelection();
                    }
                }

                forceStart = RTEditorGUI.Toggle(forceStart, "Force dialogue start");
                allowAfterSpeaking = RTEditorGUI.Toggle(allowAfterSpeaking, "Allow passing async");

                if (GUI.changed)
                {
                    if (allowAfterSpeaking)
                    {
                        flowOutput = CreateConnectionKnob(flowInStyle);
                    }
                    else
                    {
                        DeleteConnectionPort(flowOutput);
                    }
                }
            }
        }

        public override int Traverse()
        {
            IDialogueOverrideHandler handler = null;
            if (state != NodeEditorGUI.NodeEditorState.Dialogue)
            {
                handler = TaskManager.Instance;
            }
            else
            {
                handler = DialogueSystem.Instance;
            }


            if (SpeakToEntity)
            {
                handler.GetSpeakerIDList().Add(EntityID);
                if (handler as TaskManager)
                {
                    TryAddObjective();
                }

                if (handler.GetInteractionOverrides().ContainsKey(EntityID))
                {
                    handler.GetInteractionOverrides()[EntityID].Push(() =>
                    {
                        if (handler as TaskManager)
                        {
                            missionCanvasNode = this;
                        }
                        else
                        {
                            dialogueCanvasNode = this;
                        }

                        handler.SetSpeakerID(EntityID);
                        handler.SetNode(output);
                    });
                }
                else
                {
                    var stack = new Stack<UnityEngine.Events.UnityAction>();
                    stack.Push(() =>
                    {
                        /* Theoretically, I do not believe you need to check for prerequisites at the bottom of the stack.
                           I may be wrong though. */

                        if (handler as TaskManager)
                        {
                            missionCanvasNode = this;
                        }
                        else
                        {
                            dialogueCanvasNode = this;
                        }

                        handler.SetSpeakerID(EntityID);
                        handler.SetNode(output);
                    });
                    handler.GetInteractionOverrides().Add(EntityID, stack);
                }

                if (!allowAfterSpeaking)
                {
                    if (forceStart)
                    {
                        if (handler as TaskManager)
                        {
                            missionCanvasNode = this;
                        }
                        else
                        {
                            dialogueCanvasNode = this;
                        }

                        handler.SetSpeakerID(EntityID);
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    if (flowOutput == null)
                    {
                        flowOutput = outputKnobs[1];
                    }

                    handler.SetNode(flowOutput);
                    return -1;
                }
            }
            else
            {
                return 0;
            }
        }

        void SetEntityID(string newID)
        {
            EntityID = newID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        void TryAddObjective()
        {
            foreach (var ent in AIData.entities)
            {
                if (!ent)
                {
                    continue;
                }

                if (ent.ID == EntityID)
                {
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Clear();
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Add(new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        (Canvas as QuestCanvas).missionName,
                        SectorManager.instance.current.dimension,
                        ent
                    ));
                    TaskManager.DrawObjectiveLocations();
                    break;
                }
            }
        }
    }
}
