using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Delete Entity")]
    public class DeleteEntity : Node
    {
        public override string GetName { get { return "Delete"; } }
        public override string Title { get { return "Delete Entity"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 350); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Name Out", Direction.Out, "EntityID", NodeSide.Right)]
        public ConnectionKnob IDOut;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;
        public string entityID;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
           
            GUILayout.BeginHorizontal();
            GUILayout.Label("Entity ID:");
            entityID = GUILayout.TextField(entityID);
            GUILayout.EndHorizontal();

        }

        public override int Traverse()
        {
            foreach(var data in AIData.entities)
            {
                
                if(data.ID == entityID)
                {
                    Destroy(data.gameObject);
                    return 0;
                }

            }
            return 0;
        }
    }
}
