using UnityEngine;
using System.Collections.Generic;

namespace NodeEditorFramework.Standard
{
    [System.Serializable]
    public class PathData
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
}