using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Flow/Start")]
    public class StartQuestNode : Node
    {
        //Node things
        public const string ID = "StartNode";
        public override string GetID { get { return ID; } }

        public override string Title { get { return "Start"; } }

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;
    }
}