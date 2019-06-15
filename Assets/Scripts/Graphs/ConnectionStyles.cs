using UnityEngine;

namespace NodeEditorFramework.Standard
{

    public class TaskConnection : ConnectionKnobStyle
    {
        public override string Identifier { get { return "TaskFlow"; } }
        public override Color Color { get { return Color.green; } }
    }

    public class ConditionConnection : ConnectionKnobStyle
    {
        public override string Identifier { get { return "Condition"; } }
        public override Color Color { get { return Color.red; } }
    }

    public class DialogConnection : ConnectionKnobStyle
    {
        public override string Identifier { get { return "Dialogue"; } }
        public override Color Color { get { return Color.cyan; } }
    }

    public class TaskCompleteConnection : ConnectionKnobStyle
    {
        public override string Identifier { get { return "Complete"; } }
        public override Color Color { get { return Color.yellow; } }
    }

    public class ActionConnection : ConnectionKnobStyle
    {
        public override string Identifier { get { return "Action"; } }
        public override Color Color { get { return new Color(1.0f, 0.5f, 0f); } }
    }

    public class EntityIDConnection : ConnectionKnobStyle
    {
        public override string Identifier { get { return "EntityID"; } }
        public override Color Color { get { return Color.blue; } }
    }
}