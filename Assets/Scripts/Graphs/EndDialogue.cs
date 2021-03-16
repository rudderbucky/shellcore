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
        public bool openBuilder = false;
        public bool openTrader = false;
        public string traderJSON = null;
        public NodeEditorGUI.NodeEditorState state;

        public override void NodeGUI()
        {
            if(NodeEditorGUI.state == NodeEditorGUI.NodeEditorState.Dialogue && outputKnobs.Count > 0) 
            {
                DeleteConnectionPort(outputKnobs[0]);
                output = null;
            } 
            else if((NodeEditorGUI.state != NodeEditorGUI.NodeEditorState.Dialogue) && (output == null))
            {
                output = CreateConnectionKnob(OutStyle);
            }
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            if(!jumpToStart && (NodeEditorGUI.state != NodeEditorGUI.NodeEditorState.Dialogue))
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
            GUILayout.BeginHorizontal();
            openBuilder = RTEditorGUI.Toggle(openBuilder, "Open Yard");
            GUILayout.EndHorizontal();
            if(openTrader = RTEditorGUI.Toggle(openTrader, "Open Trader"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Trader Inventory JSON");
                traderJSON = GUILayout.TextArea(traderJSON);
                GUILayout.EndHorizontal();
            }
        }

        public override int Traverse()
        {
            IDialogueOverrideHandler handler = null;
            if(state != NodeEditorGUI.NodeEditorState.Dialogue)
                handler = TaskManager.Instance;
            else handler = DialogueSystem.Instance;
            
            // Debug.Log(handler as DialogueSystem + "sdjhgndfgikuhtdukhntdouhntdh " + StartDialogueNode.dialogueStartNode.EntityID);

            if(handler as TaskManager)
                foreach(var objectiveLocation in TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName])
                {
                    if(objectiveLocation.followEntity &&
                        objectiveLocation.followEntity.ID == StartDialogueNode.missionCanvasNode?.EntityID)
                    {
                        TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Remove(objectiveLocation);
                        TaskManager.DrawObjectiveLocations();
                        break;
                    }
                }
            
            var node = state != NodeEditorGUI.NodeEditorState.Dialogue 
                ? StartDialogueNode.missionCanvasNode : StartDialogueNode.dialogueCanvasNode;

            if(jumpToStart)
            {
                handler.SetNode(node);
                handler.GetInteractionOverrides()[node.EntityID].Pop();
                if(handler is DialogueSystem || (!output || !output.connected())) DialogueSystem.Instance.DialogueViewTransitionOut();

                if(openBuilder)
                {
                    DialogueSystem.Instance.OpenBuilder(SectorManager.instance.GetEntity(node.EntityID).transform.position);
                }

                if(openTrader)
                {
                    DialogueSystem.Instance.OpenTrader(SectorManager.instance.GetEntity(node.EntityID).transform.position, 
                        JsonUtility.FromJson<ShipBuilder.TraderInventory>(traderJSON).parts);
                }

                return -1;
            }
            else
            {
                if(node && node.EntityID != null && node.EntityID != "")
                {
                    handler.GetInteractionOverrides()[node.EntityID].Pop();
                    DialogueSystem.Instance.DialogueViewTransitionOut();
                    if(node == StartDialogueNode.missionCanvasNode) StartDialogueNode.missionCanvasNode = null;
                    else StartDialogueNode.dialogueCanvasNode = null;
                }

                if(openBuilder)
                {
                    DialogueSystem.Instance.OpenBuilder(SectorManager.instance.GetEntity(node.EntityID).transform.position);
                }

                if(openTrader)
                {
                    DialogueSystem.Instance.OpenTrader(SectorManager.instance.GetEntity(node.EntityID).transform.position, 
                        JsonUtility.FromJson<ShipBuilder.TraderInventory>(traderJSON).parts);
                }

                return outputKnobs.Count > 0 ? 0 : -1;
            }
        }
    }
}
