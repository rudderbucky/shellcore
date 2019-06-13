using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(true, "TaskSsytem/SpawnEntityNode")]
    public abstract class SpawnEntityNode : Node
    {
        public override string GetID { get { return "SpawnEntityNode"; } }
        public override string Title { get { return "Spawn Entity"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 200); } }

        [ConnectionKnob("Output", Direction.Out, "Task", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "Task", NodeSide.Left)]
        public ConnectionKnob input;

        public string entityID;
        public string flagID;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Entity ID:");
            entityID = GUILayout.TextField(entityID);
            GUILayout.Label("Flag ID:");
            flagID = GUILayout.TextField(flagID);
        }

        public override bool Calculate()
        {
            for (int i = 0; i < AIData.flags.Count; i++)
            {
                if(AIData.flags[i].name == flagID)
                {
                    var blueprint = ResourceManager.GetAsset<EntityBlueprint>(entityID);
                    Entity entity = new Entity();
                    entity.blueprint = blueprint;
                    entity.entityName = "ENTITY";
                    entity.faction = 0;
                    entity.spawnPoint = AIData.flags[i].transform.position;
                }
            }
            return true;
        }
    }
}
