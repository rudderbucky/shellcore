using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Condition Check Node", typeof(QuestCanvas), typeof(SectorCanvas))]
    public class ConditionCheckNode : Node
    {
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
        public int variableType = 0;
        public int comparisonMode = 0;
        public int value = 0;
        public bool inPercentage = false;
        public string sectorName;

        protected PopupMenu typePopup = null;
        protected PopupMenu comparisonPopup = null;

        protected readonly string[] comparisonModes = new string[]
        {
            "EqualTo",
            "GreaterThan",
            "LesserThan"
        };

        protected readonly string[] missionStatus = new string[]
        {
            "Inactive",
            "Ongoing",
            "Complete"
        };

        protected readonly string[] variableTypes = new string[]
        {
            "Checkpoint",
            "Task Variable",
            "Reputation",
            "Parts Seen",
            "Parts Obtained",
            "Mission Status",
            "Shards",
            "Credits",
            "Player's Build Value",
            "Sector"
        };

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

            GUILayout.Label("Variable Type:");
            if (GUILayout.Button(variableTypes[variableType]))
            {
                typePopup = new PopupMenu();
                typePopup.SetupGUI();
                for (int i = 0; i < variableTypes.Length; i++)
                {
                    typePopup.AddItem(new GUIContent(variableTypes[i]), false, SelectType, i);
                }

                typePopup.Show(GUIScaleUtility.GUIToScreenSpace(GUILayoutUtility.GetLastRect().max));
            }

            if (variableType <= 1)
            {
                GUILayout.Label("Variable Name:");
                GUILayout.BeginHorizontal();
                variableName = GUILayout.TextArea(variableName);
                GUILayout.EndHorizontal();
            }
            else if (variableType == 5)
            {
                GUILayout.Label("Mission Name:");
                GUILayout.BeginHorizontal();
                variableName = GUILayout.TextArea(variableName);
                GUILayout.EndHorizontal();
            }

            if (variableType == 0)
            {
                return;
            }

            if (variableType != 5 && variableType != 9)
            {
                GUILayout.Label("Value:");
                value = RTEditorGUI.IntField(value);
                if (variableType == 3 || variableType == 4)
                {
                    inPercentage = Utilities.RTEditorGUI.Toggle(inPercentage, "In Percent");
                    if (inPercentage && (value > 100 || value < 0))
                    {
                        value = RTEditorGUI.IntField(0);
                        Debug.LogWarning("Value must be between 0 and 100 inclusive.");
                    }
                }
            }

            if (variableType == 9)
            {
                GUILayout.Label("Current sector:");
                sectorName = RTEditorGUI.TextField(sectorName);
                return;
            }
            
            if (variableType == 5)
            {
                GUILayout.Label("Mission Status:");
            }
            else
            {
                GUILayout.Label("Comparison Mode:");
            }

            string[] comparisonTexts = variableType == 5 ? missionStatus : comparisonModes;

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

        void SelectType(object data)
        {
            int index = (int)data;
            variableType = index;
        }

        void SelectMode(object data)
        {
            int index = (int)data;
            comparisonMode = index;
        }

        public override int Traverse()
        {
            if (variableType == 0)
            {
                return TaskManager.TraversersContainCheckpoint(variableName) ? 0 : 1;
            }
            else if (variableType == 5)
            {
                for (int i = 0; i < PlayerCore.Instance.cursave.missions.Count; i++)
                {
                    if (PlayerCore.Instance.cursave.missions[i].name == variableName)
                    {
                        var status = PlayerCore.Instance.cursave.missions[i].status;
                        switch (comparisonMode)
                        {
                            case 0: return (status == Mission.MissionStatus.Inactive) ? 0 : 1;
                            case 1: return (status == Mission.MissionStatus.Ongoing) ? 0 : 1;
                            case 2: return (status == Mission.MissionStatus.Complete) ? 0 : 1;
                            default:
                                Debug.LogWarning("Found mission is neither complete, ongoing nor inactive.");
                                return 1;
                        }
                    }
                }

                Debug.LogWarning("Could not find mission in player mission list.");
                return 1;
            }
            else
            {
                int variableToCompare = 0;
                switch (variableType)
                {
                    case 1:
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
                    case 2:
                        variableToCompare = PlayerCore.Instance.reputation;
                        break;
                    case 3:
                        variableToCompare = PartIndexScript.GetNumberOfPartsSeen(inPercentage);
                        break;
                    case 4:
                        variableToCompare = PartIndexScript.GetNumberOfPartsObtained(inPercentage);
                        break;
                    case 5:
                        return PlayerCore.Instance.cursave.missions.Exists(m => m.name == variableName) &&
                               PlayerCore.Instance.cursave.missions.Find(m => m.name == variableName).status == (Mission.MissionStatus)comparisonMode
                            ? 0
                            : 1;
                    case 6:
                        variableToCompare = PlayerCore.Instance.cursave.shards;
                        break;
                    case 7:
                        variableToCompare = PlayerCore.Instance.GetCredits();
                        break;
                    case 8:
                        variableToCompare = PlayerCore.Instance.GetBuildValue();
                        break;
                    case 9:
                        return sectorName == SectorManager.instance.current.sectorName ? 0 : 1;
                }

                switch (comparisonMode)
                {
                    case 0: return (variableToCompare == value) ? 0 : 1;
                    case 1: return (variableToCompare > value) ? 0 : 1;
                    case 2: return (variableToCompare < value) ? 0 : 1;
                    default:
                        return 0;
                }
            }
        }
    }
}
