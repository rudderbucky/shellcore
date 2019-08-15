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
        public override string GetName { get { return ID; } }

        public override string Title { get { return "Start"; } }
        public override Vector2 DefaultSize { get { return new Vector2(128, 64); } }

        [ConnectionKnob("Output Right", Direction.Out, "TaskFlow", NodeSide.Right)]
        public ConnectionKnob outputRight;
    }
}