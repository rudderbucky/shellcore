using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Start Dialogue")]
    public class StartDialogueNode : Node
    {
        public static Node dialogueStartNode;

        public override string GetID { get { return "StartDialogueNode"; } }
        public override string Title { get { return "Start Dialogue"; } }

        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        public bool SpeakToEntity;
        public string EntityID;

        public override void NodeGUI()
        {
            SpeakToEntity = RTEditorGUI.Toggle(SpeakToEntity, "Speak to entity");
            if(SpeakToEntity)
            {
                GUILayout.Label("EntityID");
                EntityID = RTEditorGUI.TextField(EntityID);
            }
        }

        //TODO: override "Q"
        public override int Traverse()
        {
            Debug.Log("Start dialogue Entity ID: " + EntityID);
            dialogueStartNode = this;
            if (SpeakToEntity)
            {
                if(TaskManager.interactionOverrides.ContainsKey(EntityID))
                {
                    TaskManager.interactionOverrides[EntityID] = () => {
                        TaskManager.Instance.setNode(output);
                        TaskManager.interactionOverrides.Remove(EntityID);
                    };

                }
                else
                {
                    TaskManager.interactionOverrides.Add(EntityID, () => {
                        TaskManager.Instance.setNode(output);
                        TaskManager.interactionOverrides.Remove(EntityID);
                    });
                }
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
