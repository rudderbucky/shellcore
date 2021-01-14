using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class WCManual : GUIWindowScripts
{

    public Transform listContents;
    public GameObject buttonPrefab;
    public Text contentText;

    void Start()
    {
        foreach(var entry in manualEntries)
        {
            var button = Instantiate(buttonPrefab, listContents).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = entry.title;
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                contentText.text = entry.contents;
            }));
        }
    }

    [System.Serializable]
    public class ManualEntry
    {
        public string title;
        public string contents;
    }
    public ManualEntry placeholder;

    public List<ManualEntry> manualEntries;
}


#if UNITY_EDITOR
[CustomEditor(typeof(WCManual))]
public class WCManualEditor : Editor 
{   
    public override void OnInspectorGUI ()
    {
        var areaStyle = new GUIStyle(GUI.skin.textArea);
        


        areaStyle.wordWrap = true;
        WCManual manual = target as WCManual;
        EditorGUILayout.BeginHorizontal();
        manual.listContents = EditorGUILayout.ObjectField("List contents:", manual.listContents, typeof(Transform), true) as Transform;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        manual.buttonPrefab = EditorGUILayout.ObjectField("Button Prefab:", manual.buttonPrefab, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        manual.contentText = EditorGUILayout.ObjectField("Content Text:", manual.contentText, typeof(Text), true) as Text;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        manual.placeholder.title = EditorGUILayout.TextField("Title:", manual.placeholder.title);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Contents:");
        EditorGUILayout.EndHorizontal();
        var height = new GUIStyle(GUI.skin.textArea).CalcHeight(new GUIContent(manual.placeholder.contents), Screen.width);
        manual.placeholder.contents = EditorGUILayout.TextArea(manual.placeholder.contents, areaStyle, GUILayout.MinHeight(height));

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add"))
        {
            manual.manualEntries.Add(manual.placeholder);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Delete"))
        {
            manual.manualEntries.RemoveAt(manual.manualEntries.Count-1);
        }
        EditorGUILayout.EndHorizontal();

        foreach(var entry in manual.manualEntries)
        {
            EditorGUILayout.BeginHorizontal();
            entry.title = EditorGUILayout.TextField("Title:", entry.title);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Contents:");
            EditorGUILayout.EndHorizontal();
            var theight = new GUIStyle(GUI.skin.textArea).CalcHeight(new GUIContent(entry.contents), Screen.width);
            entry.contents = EditorGUILayout.TextArea(entry.contents, areaStyle, GUILayout.MinHeight(theight));
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif