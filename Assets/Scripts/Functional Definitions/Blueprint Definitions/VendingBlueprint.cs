using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VendingBlueprint", menuName = "ShellCore/VendingBlueprint", order = 6)]
public class VendingBlueprint : ScriptableObject
{
    [System.Serializable]
    public class Item
    {
        public EntityBlueprint entityBlueprint;
        public string icon;
        public string description;
        public int cost;
        public string json;
    }

    public int range;
    public List<Item> items;

    public int getItemIndex(string entityName)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].entityBlueprint.entityName.Equals(entityName))
            {
                return i;
            }
        }

        return -1;
    }
}
