using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(fileName = "Path", menuName = "ShellCore/Path", order = 5)]
[ExecuteInEditMode]
public class Path : ScriptableObject
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

#if UNITY_EDITOR
[CustomEditor(typeof(Path))]
[ExecuteInEditMode]
class PathEditor : Editor
{
    SerializedProperty waypoints;

    Vector2 scrollPos;

    Path path;

    private void OnEnable()
    {
        waypoints = serializedObject.FindProperty("waypoints");
        SceneView.duringSceneGui += OnSceneGUI;
        path = target as Path;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        //// Make sure everything is initialized
        //for (int i = 0; i < path.waypoints.Count; i++)
        //{
        //    if (!path.waypoints[i].isInitialized)
        //    {
        //        path.waypoints[i].init(i);
        //    }
        //}

        serializedObject.Update();

        //int oldCount = waypoints.arraySize;
        waypoints.arraySize = EditorGUILayout.DelayedIntField(waypoints.arraySize);
        for (int i = 0; i < waypoints.arraySize; i++)
        {
            SerializedProperty point = waypoints.GetArrayElementAtIndex(i);
            point.FindPropertyRelative("position").vector2Value = EditorGUILayout.Vector2Field("Position: ", point.FindPropertyRelative("position").vector2Value);
            point.FindPropertyRelative("ID").intValue = EditorGUILayout.IntField("ID: ", point.FindPropertyRelative("ID").intValue);

            SerializedProperty childArray = point.FindPropertyRelative("children");
            int oldSize = childArray.arraySize;
            point.FindPropertyRelative("children").arraySize = EditorGUILayout.IntField("Children: ", oldSize);
            if (oldSize != point.FindPropertyRelative("children").arraySize)
            {
                continue;
            }

            for (int j = 0; j < childArray.arraySize; j++)
            {
                childArray.GetArrayElementAtIndex(j).intValue = EditorGUILayout.DelayedIntField($"Child {j}: ", childArray.GetArrayElementAtIndex(j).intValue);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Handles.color = new Color(1f, 0.5f, 0.05f);

        for (int i = 0; i < path.waypoints.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.FreeMoveHandle(path.waypoints[i].position, 0.1f, Vector3.zero, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(path, "Changed waypoint position");
                path.waypoints[i] = new Path.Node
                {
                    ID = path.waypoints[i].ID,
                    children = path.waypoints[i].children,
                    position = newPos
                };
            }

            for (int j = 0; j < path.waypoints[i].children.Count; j++)
            {
                int childID = getWaypointByID(path.waypoints[i].children[j]);
                if (childID != -1)
                {
                    Handles.DrawLine(path.waypoints[i].position, path.waypoints[childID].position);
                }
            }
        }
    }

    int getWaypointByID(int ID)
    {
        for (int i = 0; i < path.waypoints.Count; i++)
        {
            if (path.waypoints[i].ID == ID)
            {
                return i;
            }
        }

        return -1;
    }
}

#endif
