using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WCPathCreator : MonoBehaviour
{
    bool dragging = false;
    Path.Node selectedNode = null;
    List<Path.Node> pathNodes;
    const float maxClickDistance = 0.5f;

    MeshFilter mf;
    MeshRenderer mr;
    Mesh lines;
    Material lineMat;

    int IDCount = 1;

    private void Awake()
    {
        mr = gameObject.AddComponent<MeshRenderer>();
        mf = gameObject.AddComponent<MeshFilter>();
        lines = new Mesh();
    }

    private void Start()
    {
        lineMat = ResourceManager.GetAsset<Material>("white_material");
        mr.material = lineMat;
        mf.sharedMesh = lines;
    }

    public void Clear()
    {
        pathNodes = new List<Path.Node>();
        UpdateMesh();
    }

    public void AddPoint(Vector2 point)
    {
        int closest = GetClosestNodeIndex(point);

        int ID = pathNodes.Count == 0 ? 0 : IDCount++;
        pathNodes.Add(new Path.Node()
        {
            ID = ID,
            position = point,
            children = new List<int>()
        });
        dragging = true;
        selectedNode = pathNodes[GetNodeIndex(ID)];

        // add child
        if (closest >= 0)
            pathNodes[closest].children.Add(ID);

        UpdateMesh();
    }

    public void PollPathDrawing()
    {
        Vector2 pos = WorldCreatorCursor.GetMousePos();

        if (pathNodes == null)
        {
            pathNodes = new List<Path.Node>();
        }

        if (Input.GetMouseButton(0))
        {
            if (dragging)
            {
                selectedNode.position = pos;
                UpdateMesh();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            int closest = GetClosestNodeIndex(pos);
            bool grabbed = false;
            if (closest >= 0)
            {
                float d = (pathNodes[closest].position - pos).sqrMagnitude;

                if (d < maxClickDistance * maxClickDistance)
                {
                    Debug.Log("grabbed");
                    grabbed = true;
                    dragging = true;
                    selectedNode = pathNodes[closest];
                    selectedNode.position = pos;
                    UpdateMesh();
                }
            }
            if(!grabbed)
            {
                AddPoint(pos);
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            int closest = GetClosestNodeIndex(pos, selectedNode != null ? selectedNode.ID : -1);

            if (closest >= 0)
            {
                float d = (pathNodes[closest].position - pos).sqrMagnitude;

                if (d < maxClickDistance * maxClickDistance && dragging)
                {
                    Path.Node from = selectedNode;
                    Path.Node to = pathNodes[closest];

                    for (int i = 0; i < pathNodes.Count; i++)
                    {
                        for (int j = 0; j < pathNodes[i].children.Count; j++)
                        {
                            if (pathNodes[i].children[j] == from.ID)
                                pathNodes[i].children[j] = to.ID;
                        }
                    }
                    to.children.AddRange(from.children);

                    for (int i = 0; i < to.children.Count; i++)
                    {
                        if (to.children[i] == to.ID)
                        {
                            to.children.RemoveAt(i--);
                            continue;
                        }
                        for (int j = 0; j < to.children.Count; j++)
                        {
                            if (i != j && to.children[i] == to.children[j])
                            {
                                to.children.RemoveAt(j--);
                            }
                        }
                    }

                    pathNodes.Remove(from);

                    Debug.Log("Combined " + from.ID + " to " + to.ID);

                    CleanPath();
                    UpdateMesh();
                }
            }

            dragging = false;
            selectedNode = null;
        }
        if (Input.GetMouseButtonUp(1))
        {
            int closest = GetClosestNodeID(pos);

            if (closest >= 0)
            {
                RemoveNode(closest);
                UpdateMesh();
                Debug.Log("Node " + closest + " removed!");
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var pathData = new NodeEditorFramework.Standard.PathData();
            pathData.waypoints = new List<NodeEditorFramework.Standard.PathData.Node>();
            for (int i = 0; i < pathNodes.Count; i++)
            {
                pathData.waypoints.Add(new NodeEditorFramework.Standard.PathData.Node()
                {
                    ID = pathNodes[i].ID,
                    position = pathNodes[i].position,
                    children = pathNodes[i].children
                });
            }

            WorldCreatorCursor.finishPath(pathData);
            WorldCreatorCursor.instance.SetMode(WorldCreatorCursor.WCCursorMode.Control);

            Clear();
        }
    }

    void CleanPath()
    {
        if (pathNodes.Count == 0)
            return;

        // make sure there's a node 0
        int zeroIndex = -1;
        for (int i = 0; i < pathNodes.Count; i++)
        {
            if (pathNodes[i].ID == 0)
            {
                zeroIndex = i;
                break;
            }
        }
        if (zeroIndex == -1)
        {
            int minID = int.MaxValue;
            for (int i = 0; i < pathNodes.Count; i++)
            {
                if (pathNodes[i].ID < minID)
                    minID = pathNodes[i].ID;
            }

            pathNodes[GetNodeIndex(minID)].ID = 0;

            for (int i = 0; i < pathNodes.Count; i++)
            {
                for (int j = 0; j < pathNodes[i].children.Count; j++)
                {
                    if (pathNodes[i].children[j] == minID)
                        pathNodes[i].children[j] = 0;
                }
            }
        }

        // remove unconnected nodes
        var closedList = new List<int>();
        var openList = new List<int>();
        openList.Add(0);

        while (openList.Count > 0)
        {
            Path.Node current = pathNodes[GetNodeIndex(openList[0])];

            for (int i = 0; i < current.children.Count; i++)
            {
                if (!closedList.Contains(current.ID))
                    openList.Add(current.children[i]);
            }

            closedList.Add(current.ID);
            openList.Remove(current.ID);
        }

        for (int i = 0; i < pathNodes.Count; i++)
        {
            bool found = false;
            for (int j = 0; j < closedList.Count; j++)
            {
                if (pathNodes[i].ID == closedList[j])
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.Log("Node " + pathNodes[i].ID + " removed!");
                pathNodes.RemoveAt(i--);
            }
        }
    }

    void RemoveNode(int ID)
    {
        pathNodes.RemoveAt(GetNodeIndex(ID));

        for (int i = 0; i < pathNodes.Count; i++)
        {
            for (int j = 0; j < pathNodes[i].children.Count; j++)
            {
                if (pathNodes[i].children[j] == ID)
                {
                    pathNodes[i].children.RemoveAt(j--);
                }
            }
        }
        CleanPath();
    }

    void UpdateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        int count = 0;

        for (int i = 0; i < pathNodes.Count; i++)
        {
            Vector2 pos1 = pathNodes[i].position;
            for (int j = 0; j < pathNodes[i].children.Count; j++)
            {
                int childIndex = GetNodeIndex(pathNodes[i].children[j]);

                if (childIndex < 0)
                    continue;

                Vector2 pos2 = pathNodes[childIndex].position;
                Vector2 d = pos1 - pos2;
                Vector2 n = new Vector2(d.y, -d.x).normalized * 0.05f;

                vertices.Add(pos1 + n);
                vertices.Add(pos1 - n);
                vertices.Add(pos2);
                //vertices.Add(pos2 - n);

                indices.Add(count * 3 + 0);
                indices.Add(count * 3 + 1);
                indices.Add(count * 3 + 2);
                //indices.Add(count * 4 + 2);
                //indices.Add(count * 4 + 3);
                //indices.Add(count * 4 + 0);

                count++;
            }
        }
        lines.SetIndices(new int[0], MeshTopology.Triangles, 0);
        lines.SetVertices(vertices);
        lines.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
    }

    int GetNodeIndex(int ID)
    {
        for (int i = 0; i < pathNodes.Count; i++)
        {
            if (pathNodes[i].ID == ID)
                return i;
        }
        return -1;
    }

    int GetClosestNodeIndex(Vector2 pos, int exclude = -1)
    {
        int index = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < pathNodes.Count; i++)
        {
            float d = (pathNodes[i].position - pos).sqrMagnitude;
            if (d < minDist && pathNodes[i].ID != exclude)
            {
                minDist = d;
                index = i;
            }
        }

        return index;
    }

    int GetClosestNodeID(Vector2 pos, int exclude = -1)
    {
        int index = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < pathNodes.Count; i++)
        {
            float d = (pathNodes[i].position - pos).sqrMagnitude;
            if (d < minDist && pathNodes[i].ID != exclude)
            {
                minDist = d;
                index = i;
            }
        }

        return index != -1 ? pathNodes[index].ID : -1;
    }

    public void SetPath(NodeEditorFramework.Standard.PathData data)
    {
        pathNodes.Clear();

        if (data != null && data.waypoints != null)
        {
            for (int i = 0; i < data.waypoints.Count; i++)
            {
                pathNodes.Add(new Path.Node()
                {
                    ID = data.waypoints[i].ID,
                    position = data.waypoints[i].position,
                    children = data.waypoints[i].children
                });
            }
        }

        CleanPath();
        UpdateMesh();
    }
}
