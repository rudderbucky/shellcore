using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(true, "Actions/SpawnEntityNode")]
    public abstract class SpawnEntityNode : Node
    {
        public override string GetID { get { return "SpawnEntityNode"; } }
        public override string Title { get { return "Spawn Entity"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 220); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("ID Out", Direction.Out, "EntityID", NodeSide.Right)]
        public ConnectionKnob IDOut;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string entityID;
        public string flagID;
        public Vector2 coordinates;
        public bool useCoordinates;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Entity ID:");
            entityID = GUILayout.TextField(entityID);

            if(useCoordinates = Utilities.RTEditorGUI.Toggle(useCoordinates, "Use coordinates"))
            {
                GUILayout.Label("Flag ID:");
                flagID = GUILayout.TextField(flagID);
            }
            else
            {
                GUILayout.Label("Coordinates:");
                float x = coordinates.x, y = coordinates.y;
                GUILayout.BeginHorizontal();
                x = Utilities.RTEditorGUI.FloatField("X", x);
                y = Utilities.RTEditorGUI.FloatField("Y", y);
                coordinates = new Vector2(x, y);
                GUILayout.EndHorizontal();
            }
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
