using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PathAI : AIModule
{
    public struct AutoNode
    {
        public AirConstruct construct;
        public int index;
    }

    private Path path;
    public UnityAction OnPathEnd;
    int waypointID = 0;
    int waypointIndex = 0;

    // if this is set to true the AI restarts on path end
    bool patrolling = false;

    public PathAI(Path path = null)
    {
        this.path = path;
    }

    public void setPath(Path path, bool patrolling = false)
    {
        this.path = path;
        this.patrolling = patrolling;
        if (path == null)
        {
            createPath();
        }

        StartPath();
    }

    //public void MoveToPosition(Vector2 pos)
    //{
    //    ai.movement.SetMoveTarget(pos, 4f);
    //    waypointID = 0;
    //}

    public override void Init()
    {
        waypointID = -1;
        initialized = true;
        if (path)
        {
            StartPath();
        }
    }

    public static readonly float minDist = 4f;

    public override void ActionTick()
    {
        if (waypointID != -1)
        {
            ai.movement.SetMoveTarget(path.waypoints[waypointIndex].position, minDist);
            if (ai.movement.targetIsInRange())
            {
                GetNextWaypoint();
            }
        }
        else
        {
            if (!patrolling)
            {
                ai.setMode(AirCraftAI.AIMode.Inactive);
                if (OnPathEnd != null)
                {
                    OnPathEnd.Invoke();
                }
            }
            else
            {
                waypointID = 0;
                waypointIndex = 0;
                ai.movement.SetMoveTarget(path.waypoints[0].position, minDist);
                if (ai.movement.targetIsInRange())
                {
                    GetNextWaypoint();
                }
            }
        }
    }

    public override void StateTick()
    {
    }

    void createPath()
    {
        var constructs = new List<AirConstruct>();
        constructs.AddRange(Object.FindObjectsOfType<AirConstruct>());
        var nodes = new List<AutoNode>();

        // Get end target
        for (int i = 0; i < constructs.Count; i++)
        {
            //TODO: find carrier (Add to blackboard)
            if (!FactionManager.IsAllied(constructs[i].faction, craft.faction))
            {
                nodes.Add(new AutoNode() {construct = constructs[i], index = 0});
                constructs.RemoveAt(i);
            }
        }

        if (nodes.Count == 0)
        {
            ai.setMode(AirCraftAI.AIMode.Inactive);
            return;
        }

        int index = 0;
        while (constructs.Count > 0)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].index != index)
                {
                    continue;
                }

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
                        nodes.Add(new AutoNode() {construct = constructs[j], index = index + 1});
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
            var node = new Path.Node()
            {
                ID = nodes.Count - i - 1,
                position = nodes[i].construct.spawnPoint
            };
            List<int> children = new List<int>();
            for (int j = 0; j < nodes.Count; j++)
            {
                if (nodes[j].index == nodes[i].index - 1)
                {
                    children.Add(nodes.Count - j - 1);
                    //Debug.Log("ID " + (nodes.Count - i - 1) + "'s child: " + (nodes.Count - j - 1));
                }
            }

            node.children = children;
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
                if (path.waypoints[i].children.Count > 0)
                {
                    waypointID = path.waypoints[i].children[Random.Range(0, path.waypoints[i].children.Count)];
                    for (int j = 0; j < path.waypoints.Count; j++)
                    {
                        if (path.waypoints[j].ID == waypointID)
                        {
                            waypointIndex = j;
                            ai.movement.SetMoveTarget(path.waypoints[j].position, minDist);
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
                ai.movement.SetMoveTarget(path.waypoints[i].position, minDist);
                waypointID = 0;
                waypointIndex = i;
                return;
            }
        }

        waypointID = -1;
    }
}
