using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Resource Pack", menuName = "ShellCore/Resource Pack", order = 9)]
public class ResourcePack : ScriptableObject
{
    public List<ResourceManager.Resource> resources;
}
