using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Start Dialogue")]
    public class StartDialogueNode : Node
    {
        public static StartDialogueNode missionCanvasNode = null;
        public static StartDialogueNode dialogueCanvasNode = null;

        public override string GetName { get { return "StartDialogueNode"; } }
        public override string Title { get { return "Start Dialogue"; } }

        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

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

        public override void NodeGUI()
        {
            if(NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Dialogue) 
            {
                DeleteConnectionPort(input);
                input = null;
            } 
            else if(NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Mission && input == null)
            {
                input = CreateConnectionKnob(inputInStyle);
            }
            SpeakToEntity = RTEditorGUI.Toggle(SpeakToEntity, "Speak to entity");
            if(SpeakToEntity)
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
                    flowOutput = CreateConnectionKnob(flowInStyle);
                else
                    DeleteConnectionPort(flowOutput);
                }
            }
        }

        public override int Traverse()
        {
            IDialogueOverrideHandler handler = null;
            var missionNode = this == missionCanvasNode;
            if(missionNode)
                handler = TaskManager.Instance;
            else handler = DialogueSystem.Instance;

            if (SpeakToEntity)
            {
                Debug.Log(EntityID);
                handler.GetSpeakerIDList().Add(EntityID);
                TryAddObjective();
                if (handler.GetInteractionOverrides().ContainsKey(EntityID))
                {
                    handler.GetInteractionOverrides()[EntityID].Push(() => {
                        if(missionNode) missionCanvasNode = this;
                        else dialogueCanvasNode = this;
                        handler.SetSpeakerID(EntityID);
                        handler.SetNode(output);
                    });
                    Debug.Log("called");

                }
                else
                {
                    var stack = new Stack<UnityEngine.Events.UnityAction>();
                    stack.Push(() => {
                            if(missionNode) missionCanvasNode = this;
                            else dialogueCanvasNode = this;
                            handler.SetSpeakerID(EntityID);
                            handler.SetNode(output);
                        });
                    handler.GetInteractionOverrides().Add(EntityID, stack);
                }

                if(!allowAfterSpeaking)
                {
                    if(forceStart)
                    {
                        handler.SetSpeakerID(EntityID);
                        return 0;
                    }
                   else return -1;
                }
                else
                {
                    if(flowOutput == null)
                        flowOutput = outputKnobs[1];
                    handler.SetNode(flowOutput);
                    Debug.Log("Start Dialogue: async pass to: " + flowOutput.connections[0].body);
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
            Debug.Log("selected " + newID + "!");

            EntityID = newID;
            WorldCreatorCursor.selectEntity -= SetEntityID;
        }

        void TryAddObjective()
        {
            foreach(var ent in AIData.entities)
            {
                if(!ent) continue;
                if(ent.ID == EntityID)
                {
                    TaskManager.objectiveLocations.Clear();
                    TaskManager.objectiveLocations.Add(new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        ent
                    ));
                    TaskManager.DrawObjectiveLocations();
                    break;
                }
            }
        }
    }
}
