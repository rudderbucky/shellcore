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

        [ConnectionKnob("Output Right", Direction.Out, "Task", NodeSide.Right)]
        public ConnectionKnob outputRight;
    }
}