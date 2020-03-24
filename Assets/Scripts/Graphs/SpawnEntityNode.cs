using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Spawn Entity")]
    public class SpawnEntityNode : Node
    {
        public override string GetName { get { return "SpawnEntityNode"; } }
        public override string Title { get { return "Spawn Entity"; } }

        public override Vector2 DefaultSize { get { return new Vector2(200, 350); } }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Name Out", Direction.Out, "EntityID", NodeSide.Right)]
        public ConnectionKnob IDOut;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public bool action;
        public string blueprint;
        public string entityName;
        public int faction;
        public string flagName;
        public Vector2 coordinates;
        public bool useCoordinates;
        public bool issueID;
        public string entityID;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Note: Using the name of a character will spawn the " +
                "character, rendering the blueprint, faction and entity ID fields obsolete.");
            GUILayout.EndHorizontal();
            GUILayout.Label("Blueprint:");
            blueprint = GUILayout.TextField(blueprint);
            GUILayout.Label("Entity Name:");
            entityName = GUILayout.TextField(entityName);
            GUILayout.Label("Faction number:");
            faction = Utilities.RTEditorGUI.IntField(faction);

            if (useCoordinates = Utilities.RTEditorGUI.Toggle(useCoordinates, "Use coordinates"))
            {
                GUILayout.Label("Coordinates:");
                float x = coordinates.x, y = coordinates.y;
                GUILayout.BeginHorizontal();
                x = Utilities.RTEditorGUI.FloatField("X", x);
                y = Utilities.RTEditorGUI.FloatField("Y", y);
                coordinates = new Vector2(x, y);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Flag Name:");
                flagName = GUILayout.TextField(flagName);
            }

            if (issueID = Utilities.RTEditorGUI.Toggle(issueID, "Issue ID"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Entity ID:");
                entityID = GUILayout.TextField(entityID);
                GUILayout.EndHorizontal();
            }
        }

        public override int Traverse()
        {
            Vector2 coords = coordinates;
            if(!useCoordinates)
            {
                for (int i = 0; i < AIData.flags.Count; i++)
                {
                    if (AIData.flags[i].name == flagName)
                    {
                        coords = AIData.flags[i].transform.position;
                        break;
                    }
                }
            }

            foreach(var data in SectorManager.instance.characters)
            {
                if(data.name == entityName)
                {
                    Debug.Log("Spawn Entity name given matches with a character name! Spawning character...");
                    
                    foreach(var oj in AIData.entities)
                    {
                        if(oj && oj.name == data.name)
                        {
                            Debug.Log("Character already found. Not spawning.");
                            return 0;
                        }
                    }

                    var characterBlueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
                    JsonUtility.FromJsonOverwrite(data.blueprintJSON, characterBlueprint);
                    Sector.LevelEntity entityData = new Sector.LevelEntity
                    {
                        faction = data.faction,
                        name = data.name,
                        position = coords,
                        ID = data.ID,
                    };
                    SectorManager.instance.SpawnEntity(characterBlueprint, entityData);
                    return 0;
                }
            }

            Debug.Log("Spawn Entity name ( " + entityName + " ) does not correspond with a character. Performing normal operations.");
            var blueprint = ResourceManager.GetAsset<EntityBlueprint>(this.blueprint);
            if (blueprint)
            {
                Sector.LevelEntity entityData = new Sector.LevelEntity
                {
                    faction = faction,
                    name = entityName,
                    position = coords,
                    ID = issueID ? entityID : "",
                };
                var entity = SectorManager.instance.SpawnEntity(blueprint, entityData);
                entity.name = entityName;
            }
            else
            {
                Debug.LogWarning("Blueprint not found!");
            }
            return 0;
        }
    }
}
