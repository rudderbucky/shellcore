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

        public bool jumpToStart = false; //Unnecessary, because unconnected dialogue ports go to start automatically. TODO: determine the fate of this functionality

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            if(!jumpToStart)
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
                TaskManager.Instance.setNode(StartDialogueNode.dialogueStartNode);
                return -1;
            }
            else
            {
                if(StartDialogueNode.dialogueStartNode.EntityID != null)
                {
                    TaskManager.interactionOverrides[StartDialogueNode.dialogueStartNode.EntityID].Pop();
                    DialogueSystem.Instance.DialogueViewTransitionOut();
                    StartDialogueNode.dialogueStartNode = null;
                }
                return 0;
            }
        }
    }
}
