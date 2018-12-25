using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VendingBlueprint", menuName = "ShellCore/VendingBlueprint", order = 6)]
public class VendingBlueprint : ScriptableObject {
    [System.Serializable]
    public struct Item
    {
        public EntityBlueprint entityBlueprint; //TODO: replace this with blueprint
        public Sprite icon;
        public int cost;
    }

    public List<Item> items;
}
