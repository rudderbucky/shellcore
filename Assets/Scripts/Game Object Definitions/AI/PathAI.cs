using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathAI : AIModule
{
    public struct AutoNode
    {
        public AirConstruct construct;
        public int index;
    }

    private Path path;
    public Vector2 currentTargetPos;
    int waypointID = 0;

    public PathAI(Path path = null)
    {
        this.path = path;
    }

    public void setPath(Path path)
    {
        this.path = path;
        if (path == null)
            createPath();
        StartPath();
    }

    public void MoveToPosition(Vector2 pos)
    {
        currentTargetPos = pos;
        waypointID = 0;
    }

    public override void Init()
    {
        waypointID = -1;
        initialized = true;
        if (path)
            StartPath();
    }

    public override void Tick()
    {
        if (waypointID != -1)
        {
            Vector2 direction = currentTargetPos - (Vector2)craft.transform.position;
            craft.MoveCraft(direction.normalized);
            if (direction.magnitude < 0.5f)
            {
                GetNextWaypoint();
            }
        }
        else
        {
            
        }
    }

    void createPath()
    {
        var constructs = new List<AirConstruct>();
        constructs.AddRange(Object.FindObjectsOfType<AirConstruct>());
        var nodes = new List<AutoNode>();

        // Get end target
        for (int i = 0; i < constructs.Count; i++)
        {
            //TODO: find carrier (from sector data?)
            if (constructs[i].faction != craft.faction)
            {
                nodes.Add(new AutoNode() { construct = constructs[i], index = 0 });
                constructs.RemoveAt(i);
            }
        }

        if (nodes.Count == 0)
        {
            //TODO: AIManager.mode = inactive;
            return;
        }

        int index = 0;
        while (constructs.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].index != index)
                    continue;

                Vector3 zero = nodes[i].construct.spawnPoint;
                AirConstruct nearest = null;
                float minD = float.MaxValue;
                for (int j = 0; j < constructs.Count; j++)
                {
                    float d = (constructs[j].spawnPoint - zero).sqrMagnitude;
                    if (d < minD)
                    {
                        nearest = constructs[j];
                        minD = d;
                    }
                }

                float trueDistance = Mathf.Sqrt(minD);
                for (int j = 0; j < constructs.Count; j++)
                {
                    float d = (constructs[j].spawnPoint - zero).magnitude;
                    if (d < trueDistance + 1f)
                    {
                        nodes.Add(new AutoNode() { construct = constructs[j], index = index + 1 });
                        constructs.RemoveAt(j);
                        j--;
                    }
                }
            }
            index++;
        }

        path = ScriptableObject.CreateInstance<Path>();
        path.waypoints = new List<Path.Node>();
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = new Path.Node();
            node.ID = nodes.Count - i - 1;
            node.position = nodes[i].construct.spawnPoint;
            List<int> children = new List<int>();
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].index == nodes[i].index - 1)
                {
                    children.Add(nodes.Count - j - 1);
                    //Debug.Log("ID " + (nodes.Count - i - 1) + "'s child: " + (nodes.Count - j - 1));
                }
            }
            node.children = children.ToArray();
            path.waypoints.Add(node);
            //Debug.Log("Created a node (index " + nodes[i].index + ") @ " + node.position + " with ID " + node.ID + " and " + children.Count + "children.");
        }
        waypointID = 0;
    }

    void GetNextWaypoint()
    {
        if (path == null)
        {
            waypointID = -1;
            return;
        }

        for (int i = 0; i < path.waypoints.Count; i++)
        {
            if (path.waypoints[i].ID == waypointID)
            {
                if (path.waypoints[i].children.Length > 0)
                {
                    waypointID = path.waypoints[i].children[Random.Range(0, path.waypoints[i].children.Length)];
                    for (int j = 0; j < path.waypoints.Count; j++)
                    {
                        if (path.waypoints[j].ID == waypointID)
                        {
                            currentTargetPos = path.waypoints[j].position;
                            break;
                        }
                    }
                    //Debug.Log("New waypoint ID: " + waypointID + " @ " + currentTargetPos);
                    return;
                }
            }
        }
        waypointID = -1;
    }

    void StartPath()
    {
        if (path == null)
        {
            waypointID = -1;
            return;
        }

        for (int i = 0; i < path.waypoints.Count; i++)
        {
            if (path.waypoints[i].ID == 0)
            {
                currentTargetPos = path.waypoints[i].position;
                waypointID = 0;
                return;
            }
        }
        waypointID = -1;
    }
}
