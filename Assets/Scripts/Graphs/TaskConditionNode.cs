using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeEditorFramework.Standard
{
    [System.Serializable]
    public enum ConditionState
    {
        Uninitialized,
        Listening,
        Completed
    }
    public interface ICondition
    {
        ConditionState State { get; set; }

        void Init(int index); // Add listeners & check if the condition is already met
        void DeInit(); // Remove listeners
    }
}