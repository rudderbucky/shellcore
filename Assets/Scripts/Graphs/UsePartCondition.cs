using UnityEngine;
using UnityEngine.Events;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Use Part")]
    public class UsePartCondition : Node, ICondition
    {
        public static UnityEvent OnPlayerReconstruct = new UnityEvent();

        public const string ID = "PartCondition";

        public override string GetName
        {
            get { return ID; }
        }

        public override string Title
        {
            get { return "Use Part"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200, 180); }
        }

        public override bool AutoLayout
        {
            get { return true; }
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
        public bool useCustomCount;
        public int selectedPartCount;
        public bool useCustomSecondaryData;
        public string secondaryData;
        public bool checkTotalPartCount;
        public int totalPartCount;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            if (checkTotalPartCount = Utilities.RTEditorGUI.Toggle(checkTotalPartCount, "Check total part count: "))
            {
                totalPartCount = Utilities.RTEditorGUI.IntField("Total part count: ", totalPartCount);
                return;
            }
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
            if (useCustomCount = Utilities.RTEditorGUI.Toggle(useCustomCount, "Use custom count: "))
            {
                selectedPartCount = Utilities.RTEditorGUI.IntField("Part count: ", selectedPartCount);
            }

            if (useCustomSecondaryData = Utilities.RTEditorGUI.Toggle(useCustomSecondaryData, "Use custom secondary data: "))
            {
                GUILayout.Label("Secondary data:");
                secondaryData = GUILayout.TextField(secondaryData);
            }
        }

        TaskManager.ObjectiveLocation objectiveLocation;

        public void Init(int index)
        {
            OnPlayerReconstruct.AddListener(CheckParts);
            State = ConditionState.Listening;
            TryAddObjective(true);
            CheckParts();
        }

        public void DeInit()
        {
            OnPlayerReconstruct.RemoveListener(CheckParts);
            State = ConditionState.Uninitialized;

            if (TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Contains(objectiveLocation))
            {
                TaskManager.objectiveLocations[(Canvas as QuestCanvas).missionName].Remove(objectiveLocation);
                TaskManager.DrawObjectiveLocations();
            }
        }

        public void CheckParts()
        {
            
            var parts = SectorManager.instance.player.blueprint.parts;
            if (parts == null) return;
            if (checkTotalPartCount)
            {
                if (parts.Count != totalPartCount) return;
                State = ConditionState.Completed;
                connectionKnobs[0].connection(0).body.Calculate();
                return;
            }
            var count = 0;
            if (string.IsNullOrEmpty(sectorName) || ShipBuilder.CheckForOrigin(sectorName, (partID, abilityID)))
            {
                for (int i = 0; i < parts.Count; i++)
                {
                    if ((string.IsNullOrEmpty(parts[i].partID) || parts[i].partID == partID) && parts[i].abilityID == abilityID)
                    {
                        if (!string.IsNullOrEmpty(sectorName))
                        {
                            ShipBuilder.RemoveOrigin(sectorName, (partID, abilityID));
                        }

                        if (useCustomSecondaryData && secondaryData != parts[i].secondaryData)
                        {
                            continue;
                        }

                        if (useCustomCount && count < selectedPartCount - 1)
                        {
                            count++;
                            continue;
                        }

                        State = ConditionState.Completed;
                        connectionKnobs[0].connection(0).body.Calculate();
                    }
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
