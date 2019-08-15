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
        public override string GetName { get { return ID; } }
        public override string Title { get { return "Test Trigger"; } }

        public ConditionState state; // Property can't be serialized -> field
        public ConditionState State { get { return state; } set { state = value; } }

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
            SectorManager.instance.player.alerter.showMessage("TEST INPUT DETECTED!", "clip_explosion");
            State = ConditionState.Completed;
            connectionKnobs[0].connection(0).body.Calculate();
        }
    }
}