using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAI : MonoBehaviour
{
    public enum Mode
    {
        Auto,
        Follow,
        Path,
        Inactive
    }

    public struct AutoNode
    {
        public AirConstruct construct;
        public int index;
    }

    public Mode mode;
    public Craft craft;

    // Auto mode
    public static List<AutoNode> targets;

    // Path mode:
    public static PathNode[] path; //TODO: reset when level sector changes, why was this static??

    public PathNode currentPath;
    public Vector3 currentTargetPos;
    // int index = 0;
    PathNode currentNode;

    // Follow mode:
    public Transform followTarget;

    private void Start()
    {
        /*if(path == null)
        {
            path = FindObjectsOfType<PathNode>();
        }*/

        if(targets == null)
        {
            List<AirConstruct> constructs = new List<AirConstruct>();
            constructs.AddRange(FindObjectsOfType<AirConstruct>());
            var nearests = new List<AirConstruct>();
            targets = new List<AutoNode>();

            // Get end target
            for (int i = 0; i < constructs.Count; i++)
            {
                //TODO: find main station, add to targets
            }

            for(int i = 0; i < constructs.Count; i++)
            {
                float minD = float.MaxValue;
                AirConstruct nearest = null;
                for (int j = 0; j < constructs.Count; j++)
                {
                    if(i != j)
                    {
                        float d = (constructs[i].transform.position - constructs[j].transform.position).sqrMagnitude;
                        if(d < minD)
                        {
                            minD = d;
                            nearest = constructs[j];
                        }
                    }
                }
                nearests.Insert(0, nearest);
            }

            /*int distance = 0; // Um no idea what this is supposed to do and it's broken so I just commented it out for now
            int resolved = 0;
            while(resolved < constructs.Count)
            {
                for (int i = 0; i < constructs.Count; i++)
                {
                    for(int j = 0; j < targets.Count; j++)
                    {
                        if (targets[j].index != distance)
                            continue;
                        if(targets[j].construct == nearests[i])
                        {
                            targets.Add(new AutoNode() { construct = constructs[i], index = distance + 1 });
                            resolved++;
                            break;
                        }

                    }
                }
                distance++;
            }*/
        }
    }

    /*void GetPathTarget()
    {
        List<PathNode> nodes = new List<PathNode>();
        for(int i = 0; i < path.Length; i++)
        {
            if(path[i].index == index)
            {
                nodes.Add(path[i]);
            }
        }

        if(nodes.Count > 0)
        {
            currentNode = nodes[Random.Range(0, nodes.Count)];
        }
    }*/

    private void Update()
    {
        //currentNode = paths.Count > 0 ? paths[0] : new PathNode { position = Vector2.zero };
        if (!craft.GetIsDead())
        {
            Vector2 direction;
            switch (mode)
            {
                case Mode.Auto:
                    // TODO: get auto target with smaller index (note opposite order!)
                    break;
                case Mode.Follow:
                    followTarget = FindObjectOfType<PlayerCore>().transform; // temporary, this should change to an owner variable given at spawn
                    direction = (followTarget.position - transform.position).magnitude > 5 ? followTarget.position - transform.position : Vector3.zero;
                    craft.MoveCraft(direction.normalized);
                    break;
                case Mode.Path:
                    if (currentTargetPos != Vector3.zero)
                    {
                        direction = currentTargetPos - transform.position;
                        craft.MoveCraft(direction.normalized);
                        if (direction.magnitude < 0.5f)
                        {
                            currentTargetPos = Vector3.zero;
                        }
                    }
                    else mode = Mode.Inactive;
                    /*if (currentNode != null)
                    {
                        //Get a node
                        direction = currentNode.positon - (Vector2)transform.position;
                        craft.MoveCraft(direction.normalized);
                        if (direction.magnitude < 0.5f)
                        {
                            index++;
                            GetPathTarget();
                        }
                    }
                    else
                    {
                        index = 0;
                        GetPathTarget();
                    }*/
                    break;
                case Mode.Inactive:
                    break;
                default:
                    Debug.LogWarning("Wtf?"); // What the fuck?
                    break;
            }
        }
    }
}
