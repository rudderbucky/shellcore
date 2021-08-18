using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Dialogue/Condition Check Node")]
    public class DialogueConditionCheckNode : ConditionCheckNode
    {
        public override string GetName
        {
            get { return "DialogueConditionCheckNode"; }
        }

        public override string Title
        {
            get { return "Dialogue Condition Check"; }
        }

        ConnectionKnobAttribute inputStyle = new ConnectionKnobAttribute("Input", Direction.In, "Dialogue", NodeSide.Left);
        ConnectionKnobAttribute outputPassStyle = new ConnectionKnobAttribute("Pass", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right, 20);
        ConnectionKnobAttribute outputFailStyle = new ConnectionKnobAttribute("Fail", Direction.Out, "Dialogue", ConnectionCount.Single, NodeSide.Right, 60);

        public string checkpointName = ""; // preserved for backwards compatibility

        public override void InitConnectionKnobs()
        {
            if (connectionPorts.Count == 0)
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
            if (variableName.Equals(checkpointName, System.StringComparison.CurrentCulture))
            {
                checkpointName = "";
            }

            if (checkpointName != "")
            {
                GUILayout.Label($"<color=red>Deprecated data detected! Checkpoint name = '{checkpointName}'</color>\n");
            }

            base.NodeGUI();
        }

        public override int Traverse()
        {
            // Backward compatibility
            if (variableType == VariableType.Checkpoint)
            {
                if (variableName == "" && checkpointName != "")
                {
                    variableName = checkpointName;
                }

                if (TaskManager.TraversersContainCheckpoint(variableName))
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            return base.Traverse();
        }
    }
}
