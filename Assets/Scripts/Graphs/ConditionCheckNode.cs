using System;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Condition Check Node", typeof(QuestCanvas), typeof(SectorCanvas))]
    public class ConditionCheckNode : Node
    {
        public enum ComparisonMode
        {
            EqualTo,
            GreaterThan,
            LesserThan
        }

        public enum MissionStatus
        {
            Inactive,
            Ongoing,
            Complete
        }

        public enum VariableType
        {
            Checkpoint,
            TaskVariable,
            Reputation,
            PartsSeen,
            PartsObtained,
            MissionStatus,
            Shards,
            Credits
        }

        public override string GetName
        {
            get { return "GeneralConditionCheckNode"; }
        }

        public override string Title
        {
            get { return "Condition Check"; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(200f, 100f); }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }
        
        public ConnectionKnob input;
        public ConnectionKnob outputPass;
        public ConnectionKnob outputFail;

        ConnectionKnobAttribute inputStyle = new ConnectionKnobAttribute("Input", Direction.In, "TaskFlow", NodeSide.Left);
        ConnectionKnobAttribute outputPassStyle = new ConnectionKnobAttribute("Pass", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 20);
        ConnectionKnobAttribute outputFailStyle = new ConnectionKnobAttribute("Fail", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right, 60);

        public string variableName = "";
        public VariableType variableType = VariableType.Checkpoint;
        public int comparisonMode = 0;
        public int value = 0;

        protected PopupMenu typePopup = null;
        protected PopupMenu comparisonPopup = null;

        static readonly string[] comparisonModes = Enum.GetNames(typeof(ComparisonMode));

        static readonly string[] missionStatus = Enum.GetNames(typeof(MissionStatus));

        static readonly string[] variableTypes = Enum.GetNames(typeof(VariableType));

        public virtual void InitConnectionKnobs()
        {
            if (input == null)
            {
                input = CreateConnectionKnob(inputStyle);
                outputPass = CreateConnectionKnob(outputPassStyle);
                outputFail = CreateConnectionKnob(outputFailStyle);
            }
        }

        public override void OnCreate()
        {
            InitConnectionKnobs();
        }

        public override void NodeGUI()
        {
            if (input == null)
            {
                InitConnectionKnobs();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pass: ");
            outputPass.DrawKnob();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Fail: ");
            outputFail.DrawKnob();
            GUILayout.EndHorizontal();

            GUILayout.Label("Variable type:");
            if (GUILayout.Button(variableType.ToString()))
            {
                typePopup = new PopupMenu();
                typePopup.SetupGUI();
                for (int i = 0; i < variableTypes.Length; i++)
                {
                    typePopup.AddItem(new GUIContent(variableTypes[i]), false, SelectType, i);
                }

                typePopup.Show(GUIScaleUtility.GUIToScreenSpace(GUILayoutUtility.GetLastRect().max));
            }

            if (variableType == VariableType.Checkpoint || variableType == VariableType.TaskVariable || variableType == VariableType.MissionStatus)
            {
                GUILayout.Label($"{(variableType == VariableType.MissionStatus ? "Mission" : "Variable")} Name:");
                GUILayout.BeginHorizontal();
                variableName = GUILayout.TextArea(variableName);
                GUILayout.EndHorizontal();
            }

            if (variableType > 0)
            {
                if (variableType != VariableType.MissionStatus)
                {
                    GUILayout.Label("Value:");
                    value = RTEditorGUI.IntField(value);
                }

                GUILayout.Label("Comparison mode:");
                string[] comparisonTexts = variableType == VariableType.MissionStatus ? missionStatus : comparisonModes;

                if (GUILayout.Button(comparisonTexts[comparisonMode]))
                {
                    comparisonPopup = new PopupMenu();
                    comparisonPopup.SetupGUI();
                    for (int i = 0; i < comparisonTexts.Length; i++)
                    {
                        comparisonPopup.AddItem(new GUIContent(comparisonTexts[i]), false, SelectMode, i);
                    }

                    comparisonPopup.Show(GUIScaleUtility.GUIToScreenSpace(GUILayoutUtility.GetLastRect().max));
                }
            }
        }

        void SelectType(object data)
        {
            
            variableType = (VariableType)data;
        }

        void SelectMode(object data)
        {
            comparisonMode = (int)data;
        }

        public override int Traverse()
        {
            if (variableType == VariableType.Checkpoint)
            {
                return TaskManager.TraversersContainCheckpoint(variableName) ? 0 : 1;
            }
            else if (variableType == VariableType.MissionStatus)
            {
                foreach (var mission in PlayerCore.Instance.cursave.missions)
                {
                    if (mission.name == variableName)
                    {
                        var status = mission.status;
                        switch ((MissionStatus)comparisonMode)
                        {
                            case MissionStatus.Inactive: return (status == Mission.MissionStatus.Inactive) ? 0 : 1;
                            case MissionStatus.Ongoing: return (status == Mission.MissionStatus.Ongoing) ? 0 : 1;
                            case MissionStatus.Complete: return (status == Mission.MissionStatus.Complete) ? 0 : 1;
                            default:
                                return 0;
                        }
                    }
                }

                return 0;
            }
            else
            {
                int variableToCompare = 0;
                switch (variableType)
                {
                    case VariableType.TaskVariable:
                        if (TaskManager.Instance.taskVariables.ContainsKey(variableName))
                        {
                            variableToCompare = TaskManager.Instance.taskVariables[variableName];
                        }
                        else
                        {
                            Debug.LogWarning("Unknown task variable: " + variableName);
                            return 1;
                        }

                        break;
                    case VariableType.Reputation:
                        variableToCompare = PlayerCore.Instance.reputation;
                        break;
                    case VariableType.PartsSeen:
                        variableToCompare = PartIndexScript.GetNumberOfPartsSeen();
                        break;
                    case VariableType.PartsObtained:
                        variableToCompare = PartIndexScript.GetNumberOfPartsObtained();
#if UNITY_EDITOR
                        if (Input.GetKey(KeyCode.J))
                        {
                            variableToCompare = 1000;
                        }
#endif
                        break;
                    case VariableType.MissionStatus:
                        return PlayerCore.Instance.cursave.missions.Exists(m => m.name == variableName) &&
                               PlayerCore.Instance.cursave.missions.Find(m => m.name == variableName).status == (Mission.MissionStatus)comparisonMode
                            ? 0
                            : 1;
                    case VariableType.Shards:
                        variableToCompare = PlayerCore.Instance.shards;
                        break;
                    case VariableType.Credits:
                        variableToCompare = PlayerCore.Instance.GetCredits();
                        break;
                }

                switch ((ComparisonMode)comparisonMode)
                {
                    case ComparisonMode.EqualTo: return (variableToCompare == value) ? 0 : 1;
                    case ComparisonMode.GreaterThan: return (variableToCompare > value) ? 0 : 1;
                    case ComparisonMode.LesserThan: return (variableToCompare < value) ? 0 : 1;
                    default:
                        return 0;
                }
            }
        }
    }
}
