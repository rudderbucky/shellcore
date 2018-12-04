using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ShellCore/Dialogue", order = 2)]
public class Dialogue : ScriptableObject
{
    public enum DialogueAction
    {
        None,
        Shop,
        Yard,
        Exit
    }

    public List<Node> nodes;

    [System.Serializable]
    public struct Node
    {
        public string buttonText;
        public string text;
        public int ID;
        public List<int> nextNodes;
        public DialogueAction action;
    }
}