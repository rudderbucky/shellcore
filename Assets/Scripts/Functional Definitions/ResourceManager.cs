using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager : MonoBehaviour
{


    [System.Serializable]
    public struct Resource
    {
        public string ID;
        public Object obj;
    }

    [HideInInspector]
    public string fieldID;
    [HideInInspector]
    public Object newObject;
    public List<Resource> builtInResources;

    Dictionary<string, Object> resources;
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        resources = new Dictionary<string, Object>();

        //Add built in resources to dictionaries

        for(int i = 0; i < builtInResources.Count; i++)
        {
            if(builtInResources[i].obj is PartBlueprint)
            {
                continue;
            }
            else
            {
                resources.Add(builtInResources[i].ID, builtInResources[i].obj);
            }
        }

        for (int i = 0; i < builtInResources.Count; i++)
            if (builtInResources[i].obj is PartBlueprint)
                resources.Add(builtInResources[i].ID, ShellPart.BuildPart(builtInResources[i].obj as PartBlueprint));

        if (File.Exists("ResourceData.txt"))
        {
            string[] lines = File.ReadAllLines("ResourceData.txt");
            int mode = -1;

            //get names
            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.ToLower().StartsWith("sprites:"))
                    mode = 0;
                else if (line.ToLower().StartsWith("parts:"))
                    mode = 1;
                else if (line.ToLower().StartsWith("blueprints:"))
                    mode = 2;
                else
                {
                    string[] separated = line.Split(':');
                    switch (mode)
                    {
                        case 0:
                            //load sprite
                            Texture2D texture = new Texture2D(2, 2);
                            texture.LoadImage(File.ReadAllBytes(separated[1]));
                            resources.Add(separated[0], Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                            break;
                        case 1:
                            //load part
                            string partData = File.ReadAllText(separated[1]);
                            resources.Add(separated[0], ShellPart.BuildPart(JsonUtility.FromJson<PartBlueprint>(partData)));
                            break;
                        case 2:
                            //load entity
                            string entityData = File.ReadAllText(separated[1]);
                            resources.Add(separated[0], JsonUtility.FromJson<EntityBlueprint>(entityData));
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    public static T GetAsset<T>(string ID) where T : Object
    {
        return Instance.getAsset<T>(ID);
    }

    public T getAsset<T>(string ID) where T : Object
    {
        if (ID == "")
            return null;

        if (resources.ContainsKey(ID))
            if (resources[ID] is T)
                return resources[ID] as T;
            else
                Debug.LogWarning("Trying to get " + ID + " (" + resources[ID].GetType() + ") as " + typeof(T).FullName);
        else
            Debug.LogWarning("Resource ID " + ID + " not found");
        return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ResourceManager))]
    class ResourceManagerEditor : Editor {
            enum EditorState {
                failedToFind,
                successDelete,
                successFind,
                successAdd,
                None,
                successModify
            }
            EditorState state = EditorState.None;
            SerializedProperty IDField;
            SerializedProperty ObjectField;
            ResourceManager manager;
        private void OnEnable() {
            manager = (ResourceManager)target;
            IDField = serializedObject.FindProperty("fieldID");
            ObjectField = serializedObject.FindProperty("newObject");
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();
            switch(state) {
                case EditorState.failedToFind:
                    EditorGUILayout.HelpBox("Failed to find a resource with the specified ID!", MessageType.Warning);
                    break;
                case EditorState.successAdd:
                    EditorGUILayout.HelpBox("Successfully added resource!", MessageType.Info);
                    break;
                case EditorState.successDelete:
                    EditorGUILayout.HelpBox("Successfully deleted resource!", MessageType.Info);
                    break;
                case EditorState.successModify:
                    EditorGUILayout.HelpBox("Successfully modified resource!", MessageType.Info);
                    break;
                case EditorState.successFind:
                    EditorGUILayout.HelpBox("Successfully found resource!", MessageType.Info);
                    break;
                default:
                    break; 
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resource Manager");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("The #1 choice for ALL ShellCore asset injections!");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            manager.fieldID = EditorGUILayout.TextField("Resource ID:", IDField.stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            manager.newObject = EditorGUILayout.ObjectField("Resource Object:", 
            ObjectField.objectReferenceValue, typeof(Object), true) as Object;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add/Modify Resource by ID!")) {
                ResourceManager.Resource resource = new ResourceManager.Resource();
                resource.ID = manager.fieldID;
                resource.obj = manager.newObject;
                for(int i = 0; i < manager.builtInResources.Count; i++) {
                    if(manager.builtInResources[i].ID == manager.fieldID) {
                        manager.builtInResources[i] = resource;
                        state = EditorState.successModify;
                        break;
                    }
                }
                if(state != EditorState.successModify) {
                    manager.builtInResources.Add(resource);
                    state = EditorState.successAdd;
                }
                manager.fieldID = "";
                manager.newObject = null;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Delete Resource by ID!")) {
                state = EditorState.failedToFind;
                foreach(ResourceManager.Resource res in manager.builtInResources) {
                    if(res.ID == manager.fieldID) {
                        manager.builtInResources.Remove(res);
                        manager.fieldID = "";
                        state = EditorState.successDelete;
                        break;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Find Resource by ID!")) {
                state = EditorState.failedToFind;
                foreach(ResourceManager.Resource res in manager.builtInResources) {
                    if(res.ID == manager.fieldID) {
                        manager.newObject = res.obj;
                        state = EditorState.successFind;
                        break;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            DrawDefaultInspector();
            if (EditorApplication.isPlaying)
                Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif