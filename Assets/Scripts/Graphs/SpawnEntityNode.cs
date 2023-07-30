using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Spawn Entity")]
    public class SpawnEntityNode : Node
    {
        public override string GetName
        {
            get { return "SpawnEntityNode"; }
        }

        public override string Title
        {
            get { return "Spawn Entity"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200, 350); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

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
        public int count = 1;
        public string flagName;
        public Vector2 coordinates;
        public bool useCoordinates;
        public bool issueID;
        public string entityID;
        public bool forceCharacterTeleport;

        public List<string> additionalFlags = new List<string>();
        public List<int> additionalCounts = new List<int>();

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();
            GUILayout.Label("Note: Using the ID of a character will spawn the " +
                            "character, rendering the blueprint, faction and entity name fields obsolete.");
            GUILayout.Label("Blueprint:");
            blueprint = GUILayout.TextField(blueprint);
            GUILayout.Label("Entity Name:");
            entityName = GUILayout.TextField(entityName);
            faction = Utilities.RTEditorGUI.IntField("Faction Number: ", faction);
            if (faction < 0)
            {
                faction = Utilities.RTEditorGUI.IntField("Faction Number: ", 0);
                Debug.LogWarning("This identification does not exist!");
            }
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
                GUILayout.Label("Entity ID:");
                entityID = GUILayout.TextField(entityID);
            }

            count = Utilities.RTEditorGUI.IntField("Spawn Count: ", Mathf.Max(1, count));

            forceCharacterTeleport = Utilities.RTEditorGUI.Toggle(forceCharacterTeleport, "Force Character Teleport");

            GUILayout.Label("Additional Spawn Points:");
            for (int i = 0; i < additionalFlags.Count; i++)
            {
                RTEditorGUI.Seperator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
                {
                    additionalFlags.RemoveAt(i);
                    additionalCounts.RemoveAt(i);
                    i--;
                    if (i == -1)
                    {
                        break;
                    }

                    continue;
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Flag Name:");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                additionalFlags[i] = GUILayout.TextField(additionalFlags[i]);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                additionalCounts[i] = Utilities.RTEditorGUI.IntField("Spawn Count: ", Mathf.Max(1, additionalCounts[i]));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.MinWidth(100f)))
            {
                additionalFlags.Add("");
                additionalCounts.Add(1);
            }

            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            count = Mathf.Max(1, count);
            if (issueID)
            {
                Vector2 coords = coordinates;
                if (!useCoordinates)
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

                foreach (var data in SectorManager.instance.characters)
                {
                    if (data.ID == entityID)
                    {
                        Debug.Log("Spawn Entity ID given matches with a character name! Spawning character...");

                        foreach (var oj in AIData.entities)
                        {
                            if (oj && oj.ID == data.ID)
                            {
                                Debug.Log("Character already found. Not spawning.");
                                if (forceCharacterTeleport)
                                {
                                    if (!(oj is AirCraft airCraft)) continue;
                                    airCraft.Warp(coords, false);
                                }

                                return 0;
                            }
                        }

                        var characterBlueprint = SectorManager.TryGettingEntityBlueprint(data.blueprintJSON);
                        Sector.LevelEntity entityData = new Sector.LevelEntity
                        {
                            faction = data.faction,
                            name = data.name,
                            position = coords,
                            ID = data.ID
                        };
                        SectorManager.instance.SpawnEntity(characterBlueprint, entityData);
                        return 0;
                    }
                }

                Debug.Log($"Spawn Entity ID ( {entityID} ) does not correspond with a character. Performing normal operations.");
            }

            for (int i = 0; i < count; i++)
            {
                SpawnAdditionalEntity(flagName);
            }

            if (additionalFlags != null)
            {
                for (int i = 0; i < additionalFlags.Count; i++)
                {
                    for (int j = 0; j < additionalCounts[i]; j++)
                    {
                        SpawnAdditionalEntity(additionalFlags[i]);
                    }
                }
            }

            return 0;
        }

        void SpawnAdditionalEntity(string flagName)
        {
            Vector2 coords = coordinates;
            if (!useCoordinates)
            {
                for (int i = 0; i < AIData.flags.Count; i++)
                {
                    if (AIData.flags[i].name == flagName)
                    {
                        coords = AIData.flags[i].transform.position;
                        if (DevConsoleScript.fullLog)
                        {
                            Debug.Log(coords);
                        }

                        break;
                    }
                }
            }

            EntityBlueprint blueprint = SectorManager.TryGettingEntityBlueprint(this.blueprint);

            if (blueprint)
            {
                Sector.LevelEntity entityData = new Sector.LevelEntity
                {
                    faction = faction,
                    name = entityName,
                    position = coords,
                    ID = issueID ? entityID : ""
                };
                var entity = SectorManager.instance.SpawnEntity(blueprint, entityData);
                if (DevConsoleScript.fullLog)
                {
                    Debug.Log(entity.transform.position + " " + entity.spawnPoint);
                }

                entity.name = entityName;
            }
            else
            {
                Debug.LogWarning("Blueprint not found!");
            }
        }
    }
}
