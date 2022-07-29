using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Delete Faction Entities")]
    public class DestroyFactionEntitiesNode : Node
    {
        public override string GetName
        {
            get { return "DeleteFactionEntities"; }
        }

        public override string Title
        {
            get { return "Delete Faction Entities"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 120); }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public int targetFaction;

        public override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            input.DisplayLayout();
            output.DisplayLayout();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            targetFaction = RTEditorGUI.IntField("Target Faction", targetFaction);
            if (targetFaction < 0)
            {
                targetFaction = RTEditorGUI.IntField("Target Faction", 1);
                Debug.LogWarning("This identification does not exist!");
            }
            GUILayout.EndHorizontal();
        }

        public override int Traverse()
        {
            foreach (var ent in AIData.entities)
            {
                if (ent == PlayerCore.Instance)
                {
                    continue;
                }

                if (ent.GetFaction() == targetFaction)
                {
                    Destroy(ent.gameObject);
                }
            }

            return 0;
        }
    }
}
