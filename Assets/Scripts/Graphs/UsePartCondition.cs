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
            GUILayout.Label("Sector name for part to come from:");
            sectorName = GUILayout.TextField(sectorName);
        }

        TaskManager.ObjectiveLocation objectiveLocation;

        public void Init(int index)
        {
            OnPlayerReconstruct.AddListener(CheckParts);
            State = ConditionState.Listening;
            TryAddObjective(true);
        }

        public void DeInit()
        {
            OnPlayerReconstruct.RemoveListener(CheckParts);
            State = ConditionState.Uninitialized;

            var missionName = (Canvas as QuestCanvas).missionName;
            if (TaskManager.objectiveLocations[missionName].Contains(objectiveLocation))
            {
                TaskManager.objectiveLocations[missionName].Remove(objectiveLocation);
                TaskManager.DrawObjectiveLocations();
            }
        }

        public void CheckParts()
        {
            if (string.IsNullOrEmpty(sectorName) || ShipBuilder.CheckForOrigin(sectorName, (partID, abilityID)))
            {
                var parts = SectorManager.instance.player.blueprint.parts;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].partID == partID && parts[i].abilityID == abilityID)
                    {
                        if (sectorName != "")
                        {
                            ShipBuilder.RemoveOrigin(sectorName, (partID, abilityID));
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
                    var missionName = (Canvas as QuestCanvas).missionName;
                    if (clear)
                    {
                        TaskManager.objectiveLocations[missionName].Clear();
                    }

                    objectiveLocation = new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        missionName,
                        ent
                    );
                    TaskManager.objectiveLocations[missionName].Add(objectiveLocation);
                    TaskManager.DrawObjectiveLocations();
                }
            }
        }
    }
}
