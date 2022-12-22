using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "AI/Change Character Blueprint")]
    public class ChangeCharacterBlueprintNode : Node
    {
        public override string GetName
        {
            get { return "ChangeCharacterBlueprint"; }
        }

        public override string Title
        {
            get { return "Change Character Blueprint"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200f, 100f); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string charID;
        public string blueprintJSON;
        public bool forceReconstruct;

        public override void NodeGUI()
        {
            GUILayout.Label("Character ID:");
            charID = GUILayout.TextField(charID);

            GUILayout.Label("Blueprint JSON:");
            blueprintJSON = GUILayout.TextField(blueprintJSON);

            forceReconstruct = GUILayout.Toggle(forceReconstruct, "Force Reconstruct");
        }

        public override int Traverse()
        {
            var charList = new List<WorldData.CharacterData>(SectorManager.instance.characters);
            if (charList.Exists(c => c.ID == charID))
            {
                Debug.Log("<Change Character Blueprint Node> Character found, changing blueprint");
                var character = charList.Find(c => c.ID == charID);
                character.blueprintJSON = blueprintJSON;

                if (forceReconstruct)
                {
                    if (AIData.entities.Exists(c => c.ID == charID))
                    {
                        Debug.Log("<Change Character Blueprint Node> Forcing reconstruct");
                        var ent = AIData.entities.Find(c => c.ID == charID);
                        var oldName = ent.entityName;
                        ent.blueprint = SectorManager.TryGettingEntityBlueprint(blueprintJSON);
                        ent.entityName = oldName;
                        ent.Rebuild();
                    }
                    else
                    {
                        Debug.LogWarning("<Change Character Blueprint Node> Cannot force reconstruct since entity does not exist, traversing");
                    }
                }
            }
            else
            {
                Debug.LogWarning("<Change Character Blueprint Node> Character not found, traversing");
            }

            return 0;
        }
    }
}
