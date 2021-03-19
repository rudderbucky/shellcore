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

        public override Vector2 DefaultSize { get { return new Vector2(200, 160); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("ID Input", Direction.In, "EntityID", ConnectionCount.Single, NodeSide.Left)]
        public ConnectionKnob IDIn;

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
            IDIn.DisplayLayout();
            //GUILayout.Label("ID Input");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Entity ID:");
            entityID = GUILayout.TextField(entityID);
            GUILayout.EndHorizontal();

            // TODO: find out why this doesn't work:

            //if (WorldCreatorCursor.instance != null)
            //{
            //    if (GUILayout.Button("Select", GUILayout.ExpandWidth(false)))
            //    {
            //        WorldCreatorCursor.selectEntity += SetEntityID;
            //        WorldCreatorCursor.instance.EntitySelection();
            //    }
            //}

        }

        //void SetEntityID(string SelectedID)
        //{
        //    entityID = SelectedID;
        //    WorldCreatorCursor.selectEntity -= SetEntityID;
        //}

        public override int Traverse()
        {
            if (IDIn.connected())
            {
                if (IDIn.connection(0).body is SpawnEntityNode)
                {
                    string ID = (IDIn.connection(0).body as SpawnEntityNode).entityID;
                }
            }

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
