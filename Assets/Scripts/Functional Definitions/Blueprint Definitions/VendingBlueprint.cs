using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VendingBlueprint", menuName = "ShellCore/VendingBlueprint", order = 6)]
public class VendingBlueprint : ScriptableObject {
    [System.Serializable]
    public struct Item
    {
        public EntityBlueprint entityBlueprint;
        public Sprite icon;
        public int cost;
    }

    public int range;
    public List<Item> items;

    public int getItemIndex(string entityName)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if(items[i].entityBlueprint.entityName.Equals(entityName))
            {
                return i;
            }
        }
        return -1;
    }
}
