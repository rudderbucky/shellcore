using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Test")]
    public class TestCondition : Node, ICondition
    {
        public static UnityEvent TestTrigger = new UnityEvent();

        public const string ID = "TestTrigger";
        public override string GetID { get { return ID; } }
        public override string Title { get { return "Test Trigger"; } }

        private ConditionState state;
        public ConditionState State { get { return state; } set { state = value; } } // Can't be serialized -> field

        [ConnectionKnob("Output Right", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob outputRight;

        public void Init(int index)
        {
            TestTrigger.AddListener(Trigger);
            State = ConditionState.Listening;
        }

        public void DeInit()
        {
            TestTrigger.RemoveListener(Trigger);
            State = ConditionState.Uninitialized;
        }

        public void Trigger()
        {
            connectionKnobs[0].connection(0).body.Calculate();
            State = ConditionState.Completed;
        }
    }
}