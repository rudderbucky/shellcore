using UnityEngine;

namespace NodeEditorFramework.Standard
{
    // Warps player to specified entity
    [Node(false, "Actions/Warp Player")]
    public class WarpPlayerNode : Node
    {
        public override string GetName
        {
            get { return "WarpPlayerNode"; }
        }

        public override string Title
        {
            get { return "Warp Player"; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob output;

        [ConnectionKnob("Input", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        public string sectorName;
        public string entityID;

        public override void NodeGUI()
        {
            GUILayout.Label("Sector Name: ");
            sectorName = GUILayout.TextField(sectorName);
            GUILayout.Label("Entity ID: ");
            entityID = GUILayout.TextField(entityID);
            if (WorldCreatorCursor.instance != null)
            {
                if (GUILayout.Button("Select Warp Entity", GUILayout.ExpandWidth(false)))
                {
                    WorldCreatorCursor.selectEntity += SelectEntity;
                    WorldCreatorCursor.instance.EntitySelection();
                }
            }
        }

        public override int Traverse()
        {
            Flag.FindEntityAndWarpPlayer(sectorName, entityID);
            return 0;
        }

        public void SelectEntity(string entityID)
        {
            this.entityID = entityID;
            // search for entity sector for autofilling
            foreach (var ent in WorldCreatorCursor.instance.placedItems)
            {
                if (ent.ID == entityID)
                {
                    var sectorWrapper = WorldCreatorCursor.instance.GetWrapperByPos(ent);
                    this.sectorName = sectorWrapper.sector.sectorName;
                }
            }

            WorldCreatorCursor.selectEntity -= SelectEntity;
        }
    }
}
