using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LandPlatform", menuName = "ShellCore/LandPlatform", order = 3)]
public class LandPlatform : ScriptableObject
{
    //public LandPlatform blueprint;
    public string[] prefabs;
    [HideInInspector]
    public int rows = 1, columns = 1;

    [HideInInspector]
    public int[] tilemap = new int[1];

    public int[] rotations = new int[1];

    public void SetViaWrapper(LandPlatformDataWrapper wrapper) {
        rows = wrapper.rows;
        columns = wrapper.columns;
        tilemap = wrapper.tilemap;
        rotations = wrapper.rotations;
        prefabs = wrapper.prefabs;
    }
}

public class LandPlatformDataWrapper {
    public int rows = 1;
    public int columns = 1;
    public int[] tilemap = new int[1];
    public int[] rotations = new int[1];
    public string[] prefabs;
}

#if UNITY_EDITOR
[CustomEditor(typeof(LandPlatform))]
class LandPlatformEditor : Editor
{
    SerializedProperty tilemap;
    SerializedProperty rows;
    SerializedProperty columns;

    Vector2 scrollPos;
    private void OnEnable()
    {
        tilemap = serializedObject.FindProperty("tilemap");
        rows = serializedObject.FindProperty("rows");
        columns = serializedObject.FindProperty("columns");

        if (tilemap.arraySize < rows.intValue * columns.intValue)
        {
            serializedObject.Update();
            tilemap.arraySize = rows.intValue * columns.intValue;
            serializedObject.ApplyModifiedProperties();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        int oldRows = rows.intValue;
        int oldColumns = columns.intValue;

        // Edit size
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Rows: ");
        rows.intValue = EditorGUILayout.IntField(rows.intValue);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Columns: ");
        columns.intValue = EditorGUILayout.IntField(columns.intValue);
        if (rows.intValue <= 0)
            rows.intValue = 1;
        if (columns.intValue <= 0)
            columns.intValue = 1;

        EditorGUILayout.EndHorizontal();

        if (rows.intValue != oldRows || columns.intValue != oldColumns)
            tilemap.arraySize = rows.intValue * columns.intValue;

        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(columns.intValue * 20 + 16), GUILayout.Height(rows.intValue * 20 + 16)))
        {
            scrollPos = scrollView.scrollPosition;
            for (int i = 0; i < rows.intValue; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < columns.intValue; j++)
                {
                    int index = i * (columns.intValue) + j;
                    SerializedProperty type = tilemap.GetArrayElementAtIndex(index);
                    type.intValue = EditorGUILayout.IntField(type.intValue, GUILayout.Width(16), GUILayout.Height(16));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}

#endif