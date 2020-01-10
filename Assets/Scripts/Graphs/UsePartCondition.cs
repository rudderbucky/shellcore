using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Use Part")]
    public class UsePartCondition : Node, ICondition
    {
        public static UnityEvent OnPlayerReconstruct = new UnityEvent();

        public const string ID = "PartCondition";
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Use Part"; } }
        public override Vector2 DefaultSize { get { return new Vector2(200, 120); } }

        public ConditionState state; // Property can't be serialized -> field
        public ConditionState State { get { return state; } set { state = value; } }

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public string partID;
        public int abilityID;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            GUILayout.Label("Part ID:");
            partID = GUILayout.TextField(partID);
            abilityID = Utilities.RTEditorGUI.IntField("Ability ID: ", abilityID);
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

            if(TaskManager.objectiveLocations.Contains(objectiveLocation))
            {
                TaskManager.objectiveLocations.Remove(objectiveLocation);
                MapMakerScript.DrawObjectiveLocations();
            }
        }

        public void CheckParts()
        {

            var parts = SectorManager.instance.player.blueprint.parts;
            for (int i = 0; i < parts.Count; i++)
            {
                if(parts[i].partID == partID && parts[i].abilityID == abilityID)
                {
                    State = ConditionState.Completed;
                    connectionKnobs[0].connection(0).body.Calculate();
                }
            }
        }

        void TryAddObjective(bool clear)
        {
            foreach(var ent in AIData.entities)
            {
                // TODO: Disambiguate name and entityName
                if(ent.name == "Yard" || ent.entityName == "Yard")
                {
                    if(clear) TaskManager.objectiveLocations.Clear();
                    objectiveLocation = new TaskManager.ObjectiveLocation(
                        ent.transform.position,
                        true,
                        ent
                    );
                    TaskManager.objectiveLocations.Add(objectiveLocation);
                    MapMakerScript.DrawObjectiveLocations();
                }
            }
        }
    }
}