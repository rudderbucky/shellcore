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

        public override string GetName { get { return "StartDialogueNode"; } }
        public override string Title { get { return "Start Dialogue"; } }

        public override bool AutoLayout { get { return true; } }
        public override bool AllowRecursion { get { return true; } }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        public bool SpeakToEntity;
        public string EntityName;
        public bool forceStart;
        public string originSector;
        public ConnectionKnob flowOutput;
        ConnectionKnobAttribute flowInStyle = new ConnectionKnobAttribute("ContinueAsync", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 20);
        public bool allowAfterSpeaking;

        public override void NodeGUI()
        {
            SpeakToEntity = RTEditorGUI.Toggle(SpeakToEntity, "Speak to entity");
            if(SpeakToEntity)
            {
                GUILayout.Label("Entity Name");
                EntityName = GUILayout.TextField(EntityName);
                if (WorldCreatorCursor.instance != null)
                {
                    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
                    {
                        WorldCreatorCursor.selectEntity += SetEntityName;
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
            dialogueStartNode = this;
            if (SpeakToEntity)
            {
                TaskManager.speakerName = EntityName;
                TryAddObjective();
                if (TaskManager.interactionOverrides.ContainsKey(EntityName))
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

                if(!allowAfterSpeaking)
                    return forceStart ? 0 : -1;
                else
                {
                    if(flowOutput == null)
                        flowOutput = outputKnobs[1];
                    TaskManager.Instance.setNode(flowOutput);
                    Debug.Log(flowOutput.name);
                    return 0;
                }
            }
            else
            {
                TaskManager.speakerName = null;
                return 0;
            }
        }

        void SetEntityName(string newName)
        {
            Debug.Log("selected " + newName + "!");

            EntityName = newName;
            WorldCreatorCursor.selectEntity -= SetEntityName;
        }

        void TryAddObjective()
        {
            foreach(var ent in AIData.entities)
            {
                if(!ent) continue;
                if(ent.entityName == EntityName)
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
