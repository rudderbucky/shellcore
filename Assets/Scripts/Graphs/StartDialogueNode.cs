using NodeEditorFramework.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Start Dialogue")]
    public class StartDialogueNode : Node
    {
        public static StartDialogueNode dialogueStartNode;

        public override string GetID { get { return "StartDialogueNode"; } }
        public override string Title { get { return "Start Dialogue"; } }

        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        public bool SpeakToEntity;
        public string EntityName;

        public override void NodeGUI()
        {
            SpeakToEntity = RTEditorGUI.Toggle(SpeakToEntity, "Speak to entity");
            if(SpeakToEntity)
            {
                GUILayout.Label("Entity Name");
                EntityName = RTEditorGUI.TextField(EntityName);
            }
        }

        public override int Traverse()
        {
            dialogueStartNode = this;
            if (SpeakToEntity)
            {
                if(TaskManager.interactionOverrides.ContainsKey(EntityName))
                {
                    TaskManager.interactionOverrides[EntityName] = () => {
                        TaskManager.Instance.setNode(output);
                    };

                }
                else
                {
                    TaskManager.interactionOverrides.Add(EntityName, () => {
                        TaskManager.Instance.setNode(output);
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
