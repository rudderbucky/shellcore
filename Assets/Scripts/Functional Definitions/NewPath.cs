using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NewPath
{
    [System.Serializable]
    public class Node
    {
        public Vector2 position;
        public int ID;
        public List<int> children;
    }

    public List<Node> waypoints;
}
