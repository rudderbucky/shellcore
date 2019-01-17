using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sector", menuName = "ShellCore/Sector", order = 8)]
public class ResourcePack : ScriptableObject
{
    public List<ResourceManager.Resource> resources;
}
