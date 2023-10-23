using System.Collections.Generic;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Sector Limiter", typeof(QuestCanvas))]
    public class SectorLimiterNode : Node
    {
        public static string LimitedSector;
        //public static SectorLimiterNode StartPoint;

        //Node things
        public const string ID = "SectorLimiterNode";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Sector Limiter"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(208, height); }
        }

        public string sectorName;
        public bool freeSector;

        float height = 40f;

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left, 20)]
        public ConnectionKnob input;

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right, 20)]
        public ConnectionKnob output;

        public override void NodeGUI()
        {
            height = 40f;
            freeSector = RTEditorGUI.Toggle(freeSector, "Free sector");
            if (!freeSector)
            {
                height = 84f;
                GUILayout.Label("Sector Name:");
                sectorName = GUILayout.TextField(sectorName, GUILayout.Width(200f));
            }
            else
            {
                sectorName = "";
            }
        }

        public override int Traverse()
        {
            LimitedSector = sectorName ?? "";


            if (sectorName == "" || freeSector)
            {
                (Canvas.Traversal as MissionTraverser).traverserLimiterDelegate = null;
                // draw objectives
                if (TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Contains(objectiveLocation))
                {
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Remove(objectiveLocation);
                }
                return 0;
            }
            else
            {
                (Canvas.Traversal as MissionTraverser).traverserLimiterDelegate = SectorUpdate;
                TryAddObjective();
                SectorUpdate(SectorManager.instance.current.sectorName);
                return -1;
            }
        }

        void SectorUpdate(string name)
        {
            if (name == sectorName)
            {
                TaskManager.Instance.setNode(output);
            }
            else
            {
                Node current = Canvas.Traversal.currentNode;
                if (current != this)
                {
                    if (current is TimelineNode)
                    {
                        TaskManager.Instance.StopAllCoroutines();
                    }

                    if (current is ConditionGroupNode cgn)
                    {
                        cgn.DeInit();
                    }

                    TaskManager.Instance.setNode(this);
                }
            }
        }

        TaskManager.ObjectiveLocation objectiveLocation;

        void TryAddObjective()
        {
            if (TaskManager.objectiveLocations == null 
                || !TaskManager.objectiveLocations.ContainsKey((Canvas as QuestCanvas).missionName) 
                || TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName] == null)
            {
                Debug.LogWarning($"Task Manager does not contain an objective list for mission {(Canvas as QuestCanvas).missionName}");
                return;
            }
            var sect = SectorManager.GetSectorByName(sectorName);
            if (!sect) return;
            var bounds = sect.bounds;
            TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Clear();
            objectiveLocation = new TaskManager.ObjectiveLocation
            (
                new Vector2(bounds.x + bounds.w / 2, bounds.y - bounds.h / 2),
                (Canvas as QuestCanvas).missionName,
                sect.dimension
            );

            TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Add(objectiveLocation);
            TaskManager.DrawObjectiveLocations();
        }
    }
}
