using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
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

    #if UNITY_EDITOR
    public List<Resource> segmentedBuiltIns;
    

    [HideInInspector]
    public string fieldID;
    [HideInInspector]
    public Object newObject;
    #endif

    //List<Resource> builtInResources;
    public static List<string> allPartNames;
    public ResourcePack resourcePack;

    Dictionary<string, Object> resources;
    public static ResourceManager Instance { get; private set; }
    public static float soundVolume = 1;
    public void Initialize()
    {
        allPartNames = new List<string>();
        Instance = this;
        resources = new Dictionary<string, Object>();

        //Add built in resources to dictionaries

        for (int i = 0; i < resourcePack.resources.Count; i++)
        {
            resources.Add(resourcePack.resources[i].ID, resourcePack.resources[i].obj);
        }

        LoadResources(Application.streamingAssetsPath);
    }

    public bool LoadResources(string path)
    {
        if (!path.Equals(Application.streamingAssetsPath))
        {
            Debug.Log("Attempting to load additional resources from: \"" + path + "\"");
        }

        var sprites = new List<(string, string)>();
        var parts = new List<(string, string)>();
        var entities = new List<(string, string)>();
        var vending = new List<(string, string)>();
        var paths = new List<(string, string)>();
        var factions = new List<(string, string)>();

        string resDataPath = System.IO.Path.Combine(path, "ResourceData.txt");

        if (File.Exists(resDataPath))
        {
            string[] lines = File.ReadAllLines(resDataPath);
            int mode = -1;

            //get files
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line == "")
                    continue;
                if (line.ToLower().StartsWith("sprites:"))
                    mode = 0;
                else if (line.ToLower().StartsWith("parts:"))
                    mode = 1;
                else if (line.ToLower().StartsWith("entities:"))
                    mode = 2;
                else if (line.ToLower().StartsWith("vending-options:"))
                    mode = 3;
                else if (line.ToLower().StartsWith("paths:"))
                    mode = 4;
                else if (line.ToLower().StartsWith("factions:"))
                    mode = 5;
                else
                {
                    string[] names = line.Split(':');
                    string resPath = System.IO.Path.Combine(path, names[1]);
                    if (File.Exists(resPath))
                    {
                        switch (mode)
                        {
                            case 0: sprites.    Add((names[0], resPath)); break;
                            case 1: parts.      Add((names[0], resPath)); break;
                            case 2: entities.   Add((names[0], resPath)); break;
                            case 3: vending.    Add((names[0], resPath)); break;
                            case 4: paths.      Add((names[0], resPath)); break;
                            case 5: factions.   Add((names[0], resPath)); break;
                            default:
                                break;
                        }
                        if (!path.Equals(Application.streamingAssetsPath))
                        {
                            Debug.Log("Resource loaded: " + names[0]);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("File '{0}' for resource '{1}' does not exist", Application.streamingAssetsPath + "\\" + names[1], names[0]);
                    }
                }
            }

            //load sprites
            for (int i = 0; i < sprites.Count; i++)
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.wrapMode = TextureWrapMode.Mirror;
                texture.LoadImage(File.ReadAllBytes(sprites[i].Item2));
                texture.filterMode = FilterMode.Trilinear;
                resources[sprites[i].Item1] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }

            //load parts
            for (int i = 0; i < parts.Count; i++)
            {
                string partData = File.ReadAllText(parts[i].Item2);
                var partBlueprint = ScriptableObject.CreateInstance<PartBlueprint>();
                JsonUtility.FromJsonOverwrite(partData, partBlueprint);
                allPartNames.Add(parts[i].Item1);
                resources[parts[i].Item1] = partBlueprint;
            }

            //load entities
            for (int i = 0; i < entities.Count; i++)
            {
                string entityData = File.ReadAllText(entities[i].Item2);
                var entityBlueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
                JsonUtility.FromJsonOverwrite(entityData, entityBlueprint);
                resources[entities[i].Item1] = entityBlueprint;
            }

            //load vending blueprints
            for (int i = 0; i < vending.Count; i++)
            {
                string vendingData = File.ReadAllText(vending[i].Item2);
                var vendingBlueprint = ScriptableObject.CreateInstance<VendingBlueprint>();
                JsonUtility.FromJsonOverwrite(vendingData, vendingBlueprint);
                resources[vending[i].Item1] = vendingBlueprint;
            }

            //load paths
            for (int i = 0; i < paths.Count; i++)
            {
                string pathData = File.ReadAllText(paths[i].Item2);
                var pathBlueprint = ScriptableObject.CreateInstance<Path>();
                JsonUtility.FromJsonOverwrite(pathData, pathBlueprint);
                resources[paths[i].Item1] = pathBlueprint;
            }

            //load factions
            for (int i = 0; i < factions.Count; i++)
            {
                string factionData = File.ReadAllText(factions[i].Item2);
                var faction = ScriptableObject.CreateInstance<Faction>();
                JsonUtility.FromJsonOverwrite(factionData, faction);
                resources[factions[i].Item1] = faction;
            }

            FactionManager.UpdateFactions();
        }
        else
        {
            Debug.Log("Resource file does not exist.");
            return false;
        }

        return true;
    }

    #if UNITY_EDITOR
    public void GenerateSegmentedList(ResourceManagerEditor.ResourcesByType type)
    {
        /*
            Heads up: if you are adding a part, make sure to ID the sprite with the extension _sprite to the part's ID.
            For example, test_part's sprite would be test_part_sprite
            Some systems work with this in mind for now (this is likely going to change since it's annoying)
         */
        segmentedBuiltIns = new List<Resource>();
        if (resourcePack == null)
            return;
        switch (type) {
            case ResourceManagerEditor.ResourcesByType.entity:
                foreach(Resource res in resourcePack.resources) {
                    if(res.obj as EntityBlueprint) {
                        segmentedBuiltIns.Add(res);
                    }
                }
                break;
            case ResourceManagerEditor.ResourcesByType.part:
                foreach(Resource res in resourcePack.resources) {
                    if(res.obj as PartBlueprint) {
                        segmentedBuiltIns.Add(res);
                    }
                }
                break;
            case ResourceManagerEditor.ResourcesByType.sprite:
                foreach(Resource res in resourcePack.resources) {
                    if(res.obj as Sprite) {
                        segmentedBuiltIns.Add(res);
                    }
                }
                break;
            case ResourceManagerEditor.ResourcesByType.prefab:
                foreach(Resource res in resourcePack.resources) {
                    if(res.obj as GameObject) {
                        segmentedBuiltIns.Add(res);
                    }
                }
                break;
            case ResourceManagerEditor.ResourcesByType.all:
                segmentedBuiltIns = resourcePack.resources;
                break;
        }
    }
    #endif
    public static T GetAsset<T>(string ID) where T : Object
    {
        return Instance.getAsset<T>(ID);
    }

    public T getAsset<T>(string ID) where T : Object
    {
        if (ID == "" || ID == null)
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

    public bool resourceExists(string ID)
    {
        if (ID == "" || ID == null)
            return false;
        return resources.ContainsKey(ID);
    }

    public static T[] GetAssetsOfType<T>() where T : Object
    {
        List<T> results = new List<T>();
        foreach (var resource in Instance.resources)
        {
            if (resource.Value is T)
                results.Add(resource.Value as T);
        }
        return results.ToArray();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ResourceManager))]
public class ResourceManagerEditor : Editor
{

    public enum ResourcesByType
    {
        part,
        sprite,
        entity,
        prefab,
        all,
    }

    enum EditorState
    {
        failedToFind,
        successDelete,
        successFind,
        successAdd,
        None,
        successModify
    }
    EditorState state = EditorState.None;
    ResourcesByType displayType = ResourcesByType.all;
    SerializedProperty IDField;
    SerializedProperty ObjectField;
    SerializedProperty segmentedBuiltIns;
    SerializedProperty resourcePack;
    SerializedProperty playerSource;
    SerializedProperty playerMusicSource;
    SerializedProperty masterVolume;
    ResourceManager manager;
    private void OnEnable()
    {
        manager = (ResourceManager)target;
        manager.GenerateSegmentedList(ResourcesByType.all);
        segmentedBuiltIns = serializedObject.FindProperty("segmentedBuiltIns");
        resourcePack = serializedObject.FindProperty("resourcePack");
        IDField = serializedObject.FindProperty("fieldID");
        ObjectField = serializedObject.FindProperty("newObject");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var oldDisplayType = displayType;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Resource Manager");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("The #1 choice for ALL ShellCore asset injections!");
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(resourcePack, new GUIContent("Resource pack: "));
        if(EditorGUI.EndChangeCheck())
        {
            manager.GenerateSegmentedList(ResourcesByType.all);
            segmentedBuiltIns = serializedObject.FindProperty("segmentedBuiltIns");
        }
        if(manager.resourcePack == null)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }
        EditorGUILayout.BeginHorizontal();
        manager.fieldID = EditorGUILayout.TextField("Resource ID:", IDField.stringValue) as string;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        manager.newObject = EditorGUILayout.ObjectField("Resource Object:",
        ObjectField.objectReferenceValue, typeof(Object), true) as Object;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("add");
        if (GUILayout.Button("Add/Modify Resource by ID!"))
        {
            ResourceManager.Resource resource = new ResourceManager.Resource();
            resource.ID = manager.fieldID;
            resource.obj = manager.newObject;
            for (int i = 0; i < manager.resourcePack.resources.Count; i++)
            {
                if (manager.resourcePack.resources[i].ID == manager.fieldID)
                {
                    manager.resourcePack.resources[i] = resource;
                    Debug.Log(manager.fieldID);
                    state = EditorState.successModify;
                    break;
                }
            }
            if (state != EditorState.successModify)
            {
                
                manager.resourcePack.resources.Add(resource);
                state = EditorState.successAdd;
            }
            manager.GenerateSegmentedList(displayType);
            IDField.stringValue = null;
            manager.fieldID = null;
            manager.newObject = ObjectField.objectReferenceValue = null;
            GUI.FocusControl("add");
            serializedObject.Update();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("delete");
        if (GUILayout.Button("Delete Resource by ID!"))
        {
            state = EditorState.failedToFind;
            foreach (ResourceManager.Resource res in manager.resourcePack.resources)
            {
                if (res.ID == manager.fieldID)
                {
                    manager.resourcePack.resources.Remove(res);
                    manager.fieldID = IDField.stringValue = "";
                    manager.newObject = ObjectField.objectReferenceValue = null;
                    manager.GenerateSegmentedList(displayType);
                    state = EditorState.successDelete;
                    GUI.FocusControl("delete");
                    serializedObject.Update();
                    break;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find Resource by ID!"))
        {
            state = EditorState.failedToFind;
            foreach (ResourceManager.Resource res in manager.resourcePack.resources)
            {
                if (res.ID == manager.fieldID)
                {
                    manager.newObject = res.obj;
                    state = EditorState.successFind;
                    break;
                }
            }
            serializedObject.Update();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Export all to ResourcePack"))
        {
            ResourcePack pack = CreateInstance<ResourcePack>();
            pack.resources = new List<ResourceManager.Resource>();
            foreach (ResourceManager.Resource res in manager.resourcePack.resources)
            {
                pack.resources.Add(res);
            }

            string path = "Assets/DefaultResources.asset";
            AssetDatabase.CreateAsset(pack, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("CreateJSON");
        if (GUILayout.Button("Create JSON file"))
        {
            string type = null;
            if (manager.newObject is EntityBlueprint)
            {
                type = "entities";
            }
            else if(manager.newObject is PartBlueprint)
            {
                type = "parts";
            }
            if(type != null)
            {
                // Create JSON
                File.WriteAllText(manager.newObject.name + ".json", JsonUtility.ToJson(manager.newObject));

                // Add path and ID to resource data
                if (!File.Exists("ResourceData.txt"))
                    File.Create("ResourceData.txt").Close();

                List<string> lines = new List<string>(File.ReadAllLines("ResourceData.txt"));
                bool sectionFound = false;
                for (int i = 0; i < lines.Count; i++)
                    if(lines[i].StartsWith(type))
                    {
                        lines.Insert(i + 1, manager.fieldID + ":" + manager.newObject.name + ".json");
                        sectionFound = true;
                        break;
                    }
                if (!sectionFound)
                {
                    lines.Add(type + ":");
                    lines.Add(manager.fieldID + ":" + manager.newObject.name + ".json");
                }

                File.WriteAllLines("ResourceData.txt", lines.ToArray());

                manager.fieldID = null;
                manager.newObject = ObjectField.objectReferenceValue = null;
            }
            else
            {
                Debug.Log("Can't serialize that asset to json format!");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        displayType = (ResourcesByType)EditorGUILayout.EnumPopup("Resources by type: ", displayType);
        if (displayType != oldDisplayType)
        {
            manager.GenerateSegmentedList(displayType);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(segmentedBuiltIns, new GUIContent("Built-ins by type"), true);
        EditorGUILayout.EndHorizontal();
        switch (state)
        {
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

        serializedObject.ApplyModifiedProperties();
    }
}
#endif