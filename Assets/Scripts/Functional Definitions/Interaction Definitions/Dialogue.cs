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
        Upgrader
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
    }
}
