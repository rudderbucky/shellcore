using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/End Dialogue")]
    public class EndDialogue : Node
    {
        public override string GetName { get { return "EndDialogue"; } }
        public override string Title { get { return "End Dialogue"; } }

        public override Vector2 MinSize { get { return new Vector2(200f, 100f); } }
        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        ConnectionKnobAttribute OutStyle = new ConnectionKnobAttribute("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right);

        [ConnectionKnob("Input", Direction.In, "Dialogue", NodeSide.Left)]
        public ConnectionKnob input;
        public ConnectionKnob output;

        public bool jumpToStart = false; // This is now necessary :)

        public override void NodeGUI()
        {
            if(NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Dialogue && outputKnobs.Count > 0) 
            {
                DeleteConnectionPort(outputKnobs[0]);
                output = null;
            } 
            else if((NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Mission) && (output == null))
            {
                output = CreateConnectionKnob(OutStyle);
            }
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            if(!jumpToStart && (NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Mission))
            {
                if (output == null)
                {
                    if (outputKnobs.Count == 0)
                        output = CreateConnectionKnob(OutStyle);
                    else
                        output = outputKnobs[0];
                }
                output.DisplayLayout();
            }
            GUILayout.EndHorizontal();
            jumpToStart = RTEditorGUI.Toggle(jumpToStart, "Jump to start");
            if(jumpToStart && outputKnobs.Count > 0)
            {
                DeleteConnectionPort(outputKnobs[0]);
                output = null;
            }
        }

        public override int Traverse()
        {
            IDialogueOverrideHandler handler = null;
            if(StartDialogueNode.dialogueStartNode.canvasState == NodeEditorGUI.NodeEditorState.Mission)
                handler = TaskManager.Instance;
            else handler = DialogueSystem.Instance;
            
            if(handler as TaskManager)
                foreach(var objectiveLocation in TaskManager.objectiveLocations)
                {
                    if(StartDialogueNode.dialogueStartNode && objectiveLocation.followEntity &&
                        objectiveLocation.followEntity.ID == StartDialogueNode.dialogueStartNode.EntityID)
                    {
                        TaskManager.objectiveLocations.Remove(objectiveLocation);
                        TaskManager.DrawObjectiveLocations();
                        break;
                    }
                }

            if(jumpToStart)
            {
                handler.SetNode(StartDialogueNode.dialogueStartNode);
                return -1;
            }
            else
            {
                if(StartDialogueNode.dialogueStartNode.EntityID != null)
                {
                    handler.GetInteractionOverrides()[StartDialogueNode.dialogueStartNode.EntityID].Pop();
                    DialogueSystem.Instance.DialogueViewTransitionOut();
                    StartDialogueNode.dialogueStartNode = null;
                }
                return outputKnobs.Count > 0 ? 0 : -1;
            }
        }
    }
}
