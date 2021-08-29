﻿using System.Collections;
using UnityEditor;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [Node(false, "Conditions/Time")]
    public class TimeCondition : Node, ICondition
    {
        public const string ID = "TimeTrigger";

        public override string GetName
        {
            get { return ID; }
        }
        public override bool AutoLayout
        {
            get { return true; }
        }

        public override Vector2 MinSize
        {
            get { return new Vector2(300F, 100f); }
        }
        public override string Title
        {
            get { return "Time Trigger"; }
        }

        public ConditionState state; // Property can't be serialized -> field

        public ConditionState State
        {
            get { return state; }
            set { state = value; }
        }

        [ConnectionKnob("Output", Direction.Out, "Condition", NodeSide.Right)]
        public ConnectionKnob output;

        public int seconds = 0;
        public int milliseconds = 0;
        public float totalTime = 0;
        Coroutine timer = null;

        public override void NodeGUI()
        {
            output.DisplayLayout();
            seconds = Utilities.RTEditorGUI.IntField("Time (seconds): ", seconds);
            milliseconds = Utilities.RTEditorGUI.IntField("Additional Time (milliseconds): ", milliseconds, GUILayout.Width(300F));
        }

        public void Init(int index)
        {
            Debug.Log("Initializing...");
            State = ConditionState.Listening;
            totalTime = seconds + (milliseconds / 1000f);
            if (timer == null)
            {
                timer = TaskManager.Instance.StartCoroutine(Timer(totalTime));
                Debug.Log("Timer started!");
            }
        }

        public void DeInit()
        {
            // TODO: Find why the timer is sometimes null
            if (timer != null)
            {
                TaskManager.Instance.StopCoroutine(timer);
            }

            timer = null;
            State = ConditionState.Uninitialized;
            Debug.Log("Timer stopped!");
        }

        public void Trigger()
        {
            State = ConditionState.Completed;
            connectionKnobs[0].connection(0).body.Calculate();
        }

        IEnumerator Timer(float delay)
        {
            yield return new WaitForSeconds(delay);
            Trigger();
            timer = null;
        }
    }
}
