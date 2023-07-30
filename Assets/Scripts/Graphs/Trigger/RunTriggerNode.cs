using NodeEditorFramework.Utilities;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Trigger/Run Trigger")]
    public class RunTriggerNode : Node
    {

        public override string GetName
        {
            get { return "RunTriggerNode"; }
        }

        public override string Title
        {
            get { return "Run Trigger"; }
        }

        public override bool AutoLayout
        {
            get { return true; }
        }

        public override bool AllowRecursion
        {
            get { return true; }
        }

        [ConnectionKnob("Input Left", Direction.In, "TaskFlow", NodeSide.Left)]
        public ConnectionKnob input;

        [ConnectionKnob("Output", Direction.Out, "TaskFlow", ConnectionCount.Single, NodeSide.Right)]
        public ConnectionKnob output;

        public string triggerName;
        public bool threadMode;

        public override void NodeGUI()
        {
            GUILayout.Label("Trigger Name: ");
            triggerName = GUILayout.TextField(triggerName);
            threadMode = RTEditorGUI.Toggle(threadMode, "Thread mode");
        }

        public override int Traverse()
        {
            if (TriggerManager.instance)
            {
                if (threadMode)
                {
                    var traverser = new TriggerTraverser(triggerName, Canvas, null, null);
                    TriggerManager.instance.traversers.Add(traverser);
                    if (Canvas.Traversal is SectorTraverser sectorTraverser) traverser.sectorStartNode = sectorTraverser.startNode;
                    traverser.StartQuest();
                    return 0;
                }
                else
                {
                    var node = outputKnobs[0].connections.Count > 0 ? outputKnobs[0].connections[0]?.body : null;
                    var traverser = new TriggerTraverser(triggerName, Canvas, Canvas.Traversal as Traverser, node);
                    TriggerManager.instance.traversers.Add(traverser);
                    if (Canvas.Traversal is SectorTraverser sectorTraverser) traverser.sectorStartNode = sectorTraverser.startNode;
                    traverser.StartQuest();
                    return -1;
                }

            }
            else Debug.LogWarning("No trigger manager instance. What");
            return 0;
        }
    }
}
