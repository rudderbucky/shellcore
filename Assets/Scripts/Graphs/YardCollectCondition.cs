using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Yard Collect")]
    public class YardCollectCondition : Node, ICondition
    {
        public delegate void YardCollectDelegate(string partId, int abilityId, string sector);

        public static YardCollectDelegate OnYardCollect;
        public const string ID = "YardCollectCondition";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Yard Collect Condition"; }
        }

        public override Vector2 DefaultSize
        {
            get { return new Vector2(200, 180); }
        }

        public ConditionState state; // Property can't be serialized -> field

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public string partID;
        public int abilityID;
        public string sectorName;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Part ID:");
            partID = GUILayout.TextField(partID);
            abilityID = Utilities.RTEditorGUI.IntField("Ability ID: ", abilityID);
            if (abilityID < 0)
            {
                abilityID = Utilities.RTEditorGUI.IntField("Ability ID: ", 0);
                Debug.LogWarning("This identification does not exist!");
            }
            GUILayout.Label("Sector name for part to come from:");
            sectorName = GUILayout.TextField(sectorName);
        }

        TaskManager.ObjectiveLocation objectiveLocation;

        public void Init(int index)
        {
            OnYardCollect += (CheckParts);
            State = ConditionState.Listening;
            TryAddObjective(true);
        }

        public void DeInit()
        {
            OnYardCollect -= (CheckParts);
            State = ConditionState.Uninitialized;

            if (TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Contains(objectiveLocation))
            {
                TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Remove(objectiveLocation);
                TaskManager.DrawObjectiveLocations();
            }
        }

        public void CheckParts(string partId, int abilityId, string sector)
        {
            if (string.IsNullOrEmpty(sectorName) || sectorName == sector)
            {
                if (partId == partID && abilityId == abilityID)
                {
                    State = ConditionState.Completed;
                    connectionKnobs[0].connection(0).body.Calculate();
                }
            }
        }

        void TryAddObjective(bool clear)
        {
            foreach (var ent in AIData.entities)
            {
                // TODO: Disambiguate name and entityName
                if (ent.name == "Yard" || ent.entityName == "Yard")
                {
                    if (clear)
                    {
                        TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Clear();
                    }

                    objectiveLocation = new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        (Canvas as QuestCanvas).missionName,
                        SectorManager.instance.current.dimension,
                        ent
                    );
                    TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Add(objectiveLocation);
                    TaskManager.DrawObjectiveLocations();
                }
            }
        }
    }
}
