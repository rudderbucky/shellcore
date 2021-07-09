using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Pack", menuName = "WorldCreator/Item Pack", order = 1)]
public class ItemPack : ScriptableObject
{
    public List<Item> items;
}
