using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemType {
		Other,
		Platform,
        Flag,
        Decoration,
        BackgroundDecoration
}

/// <summary>
/// The base type of object that is placeable in the world.
/// </summary>
[System.Serializable]
public class Item {
        public string name;
    	public GameObject obj;
        public ItemType type;
		public string assetID;
		public string shellcoreJSON;
		public string vendingID;
		public bool isTarget;
		public int faction;
		public int placeablesIndex;
		public Vector3 pos;
		public int rotation;
        public string ID;
}

public class ItemHandler : MonoBehaviour
{
    #if UNITY_EDITOR

    [HideInInspector]
    public string text;

    #endif
    
    public void GenerateItemList() {
        if(!itemPack) items = new List<Item>();
        else items = itemPack.items;
    }

    public List<Item> items;
    public ItemPack itemPack;
    public GameObject buttonPrefab;
    public Transform viewContent;
    public WorldCreatorCursor cursor;
    public static ItemHandler instance;
    void Start() {
        GenerateItemList();
        for(int i = 0; i < itemPack.items.Count; i++) {
            var ib = Instantiate(buttonPrefab, viewContent, false).GetComponent<ItemButtonScript>();
            ib.item = itemPack.items[i];
            ib.itemIndex = i;
            ib.cursor = cursor;
        }
        instance = this;
    }

    public Item GetItemByIndex(int index) {
        return CopyItem(index);
    }

    // soft copy of seeded items
    public Item CopyItem(int index) {
        var toCopy = itemPack.items[index];
        Item item = new Item();
        item.assetID = toCopy.assetID;
        item.isTarget = toCopy.isTarget;
        item.type = toCopy.type;
        item.shellcoreJSON = toCopy.shellcoreJSON;
        item.placeablesIndex = toCopy.placeablesIndex;
        item.name = toCopy.name;
        item.obj = Instantiate(toCopy.obj);
        return item;
    }

    // hard copy
    public Item CopyItem(Item toCopy) {
        Item item = new Item();
        item.ID = toCopy.ID;
        item.assetID = toCopy.assetID;
        item.isTarget = toCopy.isTarget;
        item.type = toCopy.type;
        item.shellcoreJSON = toCopy.shellcoreJSON;
        item.placeablesIndex = toCopy.placeablesIndex;
        item.pos = toCopy.pos;
        item.name = toCopy.name;
        item.obj = Instantiate(toCopy.obj);
        return item;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemHandler))]
public class ItemHandlerEditor : Editor 
{
    ItemHandler handler;
    SerializedProperty builtIns;
    SerializedProperty pack;
    Object objRef;
    Item placeholder;
    int mode;
    SerializedProperty buttonPrefab;
    SerializedProperty cursor;
    SerializedProperty viewContent;
    private void OnEnable() {
        objRef = new Object();
        placeholder = new Item();
        handler = (ItemHandler)target;
        handler.GenerateItemList();
        builtIns = serializedObject.FindProperty("items");
        pack = serializedObject.FindProperty("itemPack");
        buttonPrefab = serializedObject.FindProperty("buttonPrefab");
        cursor = serializedObject.FindProperty("cursor");
        viewContent = serializedObject.FindProperty("viewContent");
    }
    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Item Handler");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("The #1 choice for ALL ShellCore World Creator item injections!");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(pack, new GUIContent("Item Pack: "));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
            mode = GUILayout.Toolbar(mode, new string[] {"Add Mode", "View Mode"});
        EditorGUILayout.EndHorizontal();
        switch(mode) {
            case 0:
                EditorGUILayout.BeginHorizontal();
                placeholder.obj = EditorGUILayout.ObjectField("Item appearance:",
                placeholder.obj, typeof(GameObject), true) as GameObject;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.type = (ItemType)EditorGUILayout.EnumPopup("Item type: ", placeholder.type);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.name = EditorGUILayout.TextField("Asset Name:", placeholder.name) as string;
                //placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.assetID = EditorGUILayout.TextField("Asset ID:", placeholder.assetID) as string;
                // placeholder.assetID = EditorGUILayout.TextField("Asset ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.shellcoreJSON = EditorGUILayout.TextField("Ship JSON/Secondary data:", placeholder.shellcoreJSON) as string;
                //placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID (if any):", placeholder.vendingID) as string;
                //placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Faction:");
                placeholder.faction = EditorGUILayout.IntField(placeholder.faction);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Is Target:");
                placeholder.isTarget = EditorGUILayout.Toggle(placeholder.isTarget);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName("add");
                if(GUILayout.Button("Add Item")) {
                    if(handler.itemPack) {
                        handler.itemPack.items.Add(placeholder);
                        ExportData();
                    }
                };
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName("update");
                if(GUILayout.Button("Force Update Item Pack")) {
                    if(handler.itemPack) {
                        ExportData();
                    }
                };
                EditorGUILayout.EndHorizontal();
                break;
            default:
                break;
        }
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(builtIns, new GUIContent("Built-ins by type"), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        handler.cursor = EditorGUILayout.ObjectField("Cursor:", handler.cursor, typeof(WorldCreatorCursor), true) as WorldCreatorCursor;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        handler.buttonPrefab = EditorGUILayout.ObjectField("Button Prefab:", handler.buttonPrefab, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        handler.viewContent = EditorGUILayout.ObjectField("Content Transform:", handler.viewContent, typeof(Transform), true) as Transform;
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void ExportData() {
        ItemPack pack = CreateInstance<ItemPack>();
        pack.items = new List<Item>();
        foreach (Item i in handler.itemPack.items)
        {
            pack.items.Add(i);
        }
        string path = "Assets/World Creator Assets/DefaultItems.asset";
        AssetDatabase.CreateAsset(pack, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        handler.itemPack = pack;
        handler.GenerateItemList();
    }
}
#endif