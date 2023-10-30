using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueable
{
}

[CreateAssetMenu(fileName = "Dialogue", menuName = "ShellCore/Dialogue", order = 3)]
public class Dialogue : ScriptableObject, IDialogueable
{
    public enum DialogueAction
    {
        None,
        Outpost,
        Shop,
        Yard,
        Exit,
        Workshop,
        Upgrader,
        InvokeEnd,
        ForceToNextID,
        Call,
        FinishTask
    }

    public List<Node> nodes;
    public List<EntityBlueprint.PartInfo> traderInventory;
    public VendingBlueprint vendingBlueprint;

    [System.Serializable]
    public struct Node
    {
        public string buttonText;
        public string text;
        public Color textColor;
        public int ID;
        public List<int> nextNodes;
        public DialogueAction action;
        [NonSerialized]
        public string speakerID;
        [NonSerialized]
        public bool forceSpeakerChange;
        [NonSerialized]
        public bool useSpeakerColor;
        [NonSerialized]
        public Task task;
        [NonSerialized]
        public string functionID;
        [NonSerialized]
        public float typingSpeedFactor;
        [NonSerialized]
        public bool useLocalMap;
    }
}
