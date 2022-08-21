using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Actions/Map Marker")]
    public class MapMarkerNode : Node
    {
        public const string ID = "MapMarker";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Map Marker"; }
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
            output.DisplayLayout();
            GUILayout.Label("Sector name: ");
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

        // TODO: SetFlagInteractibilityNode also has this in common
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

        public override int Traverse()
        {
            var ent = SectorManager.instance.GetEntity(entityID);
            TaskManager.ObjectiveLocation objectiveLocation;
            if (ent)
            {
                objectiveLocation = new TaskManager.ObjectiveLocation(
                    ent.transform.position,
                    true,
                    (Canvas as QuestCanvas).missionName,
                    SectorManager.instance.current.dimension,
                    ent
                );
            }
            else
            {
                var sect = SectorManager.GetSectorByName(sectorName);
                var bounds = sect.bounds;
                objectiveLocation = new TaskManager.ObjectiveLocation(
                    new Vector2(bounds.x + bounds.w / 2, bounds.y - bounds.h / 2),
                    true,
                    (Canvas as QuestCanvas).missionName,
                    sect.dimension
                );
            }

            TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Add(objectiveLocation);
            TaskManager.DrawObjectiveLocations();
            return 0;
        }
    }
}
