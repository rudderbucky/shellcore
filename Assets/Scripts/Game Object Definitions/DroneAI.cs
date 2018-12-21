using System.Collections.Generic;
using UnityEngine;

public class DroneAI : MonoBehaviour
{
    public enum AIMode
    {
        Position,
        AutoPath,
        Follow,
        Path,
        Inactive
    }

    public struct AutoNode
    {
        public AirConstruct construct;
        public int index;
    }

    private AIMode mode;
    public AIMode Mode
    {
        get
        {
            return mode;
        }
        set
        {
            mode = value;
            if(mode == AIMode.AutoPath || mode == AIMode.Path)
            {
                StartPath();
            }
        }
    }
    public Craft craft;

    // Auto mode
    public Path autoPath;

    // Path mode:
    public Path path;
    public Vector2 currentTargetPos;
    int waypointID = 0;

    // Follow mode:
    public Transform followTarget;

    private void Start()
    {
        if (autoPath == null && Mode == AIMode.AutoPath)
        {
            createPath();
        }
        StartPath();
    }

    void createPath()
    {
        var constructs = new List<AirConstruct>();
        constructs.AddRange(FindObjectsOfType<AirConstruct>());
        var nodes = new List<AutoNode>();

        // Get end target
        for (int i = 0; i < constructs.Count; i++)
        {
            //TODO: find carrier (from sector data?)
            if(constructs[i].faction != craft.faction)
            {
                nodes.Add(new AutoNode() { construct = constructs[i], index = 0 });
                constructs.RemoveAt(i);
            }
        }

        if (nodes.Count == 0)
        {
            Debug.Log("Abort!!");
            Mode = AIMode.Inactive;
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

        autoPath = ScriptableObject.CreateInstance<Path>();
        autoPath.waypoints = new List<Path.Node>();
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
                    Debug.Log("ID " + (nodes.Count - i - 1) + "'s child: " + (nodes.Count - j - 1));
                }
            }
            node.children = children.ToArray();
            autoPath.waypoints.Add(node);
            Debug.Log("Created a node (index " + nodes[i].index + ") @ " + node.position + " with ID " + node.ID + " and " + children.Count + "children.");
        }
        waypointID = 0;
    }

    void GetNextWaypoint()
    {
        Debug.Log("Getting next waypoint...");

        Path p = (Mode == AIMode.AutoPath) ? autoPath : path;
        if (p == null)
        {
            waypointID = -1;
            return;
        }

        for (int i = 0; i < p.waypoints.Count; i++)
        {
            if (p.waypoints[i].ID == waypointID)
            {
                if (p.waypoints[i].children.Length > 0)
                {
                    waypointID = p.waypoints[i].children[Random.Range(0, p.waypoints[i].children.Length)];
                    for (int j = 0; j < p.waypoints.Count; j++)
                    {
                        if(p.waypoints[j].ID == waypointID)
                        {
                            currentTargetPos = p.waypoints[j].position;
                            break;
                        }
                    }
                    Debug.Log("New waypoint ID: " + waypointID + " @ " + currentTargetPos);
                    return;
                }
            }
        }
        waypointID = -1;
    }

    void StartPath()
    {
        Path p = (Mode == AIMode.AutoPath) ? autoPath : path;
        if (p == null)
        {
            waypointID = -1;
            return;
        }

        for (int i = 0; i < p.waypoints.Count; i++)
        {
            if (p.waypoints[i].ID == 0)
            {
                currentTargetPos = p.waypoints[i].position;
                waypointID = 0;
                return;
            }
        }
        waypointID = -1;
    }

    private void Update()
    {
        if (!craft.GetIsDead())
        {
            Vector2 direction;
            switch (Mode)
            {
                case AIMode.AutoPath:
                    if (waypointID != -1)
                    {
                        direction = currentTargetPos - (Vector2)transform.position;
                        craft.MoveCraft(direction.normalized);
                        if (direction.magnitude < 0.5f)
                        {
                            GetNextWaypoint();
                        }
                    }
                    break;
                case AIMode.Follow:
                    followTarget = FindObjectOfType<PlayerCore>().transform; // temporary, this should change to an owner variable given at spawn
                    direction = (followTarget.position - transform.position).magnitude > 5 ? followTarget.position - transform.position : Vector3.zero;
                    craft.MoveCraft(direction.normalized);
                    break;
                case AIMode.Path:
                    if (waypointID != -1)
                    {
                        direction = currentTargetPos - (Vector2)transform.position;
                        craft.MoveCraft(direction.normalized);
                        if (direction.magnitude < 0.5f)
                        {
                            GetNextWaypoint();
                        }
                    }
                    else Mode = AIMode.Inactive;
                    break;
                case AIMode.Inactive:
                    break;
                case AIMode.Position:
                    direction = currentTargetPos - (Vector2)transform.position;
                    craft.MoveCraft(direction.normalized);
                    if (direction.magnitude < 0.5f)
                    {
                        Mode = AIMode.Inactive;
                    }
                    break;
                default:
                    Debug.LogWarning("Movement mode missing!");
                    break;
            }
        }
    }
}
