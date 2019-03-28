using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "TaskSystem/Start")]
    public class StartQuestNode : Node
    {
        //Node things
        public const string ID = "StartNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Start"; } }

        public override bool AllowRecursion { get { return true; } }
        public override bool ContinueCalculation { get { return true; } }

        [ConnectionKnob("Output Right", Direction.In, "Task", NodeSide.Right)]
        public ConnectionKnob outputRight;
    }
}