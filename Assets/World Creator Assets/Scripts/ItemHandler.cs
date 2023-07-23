﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

#endif

public enum ItemType
{
    Other,
    Platform,
    Flag,
    Decoration,
    BackgroundDecoration,
    DecorationWithMetadata
}

/// <summary>
/// The base type of object that is placeable in the world.
/// </summary>
[System.Serializable]
public class Item
{
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
    public NodeEditorFramework.Standard.PathData patrolPath;
    public string ID;
    public int dimension;
}

public class ItemHandler : MonoBehaviour
{
#if UNITY_EDITOR

    [HideInInspector]
    public string text;

#endif

    public void GenerateItemList()
    {
        if (!itemPack)
        {
            items = new List<Item>();
        }
        else
        {
            items = itemPack.items;
        }
    }

    public List<Item> items;
    public ItemPack itemPack;
    public GameObject buttonPrefab;
    public Transform viewContent;
    public WorldCreatorCursor cursor;
    public static ItemHandler instance;

    void Awake()
    {
        instance = this;
        GenerateItemList();
    }

    void Start()
    {
    }

    public Item GetItemByIndex(int index)
    {
        return CopyItem(index);
    }

    public void ClearInstantiation() 
    {
        if (itemsThatNeedInstantiation == null) return;
        itemsThatNeedInstantiation.Clear();
    }

    Dictionary<Item, GameObject> itemsThatNeedInstantiation = new Dictionary<Item, GameObject>();

    public void StartInstantiation()
    {

#if UNITY_EDITOR
        StartCoroutine(InstantiateCoroutine());
#else        
        foreach(var kvp in itemsThatNeedInstantiation)
        {
            kvp.Key.obj = Instantiate(kvp.Value);
            kvp.Key.obj.transform.position = kvp.Key.pos;
            if (kvp.Key.type == ItemType.Platform)
                kvp.Key.obj.transform.RotateAround(kvp.Key.pos, Vector3.forward, 90 * kvp.Key.rotation);
        }
        itemsThatNeedInstantiation.Clear();
#endif
    }

    private IEnumerator InstantiateCoroutine()
    {
        while (itemsThatNeedInstantiation.Count > 0)
        {
            var items = new List<Item>();
            foreach(var kvp in itemsThatNeedInstantiation)
            {
                var pos = Camera.main.WorldToViewportPoint(kvp.Key.pos);
                if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1) continue;
                kvp.Key.obj = Instantiate(kvp.Value);
                kvp.Key.obj.transform.position = kvp.Key.pos;
                if (kvp.Key.type == ItemType.Platform)
                    kvp.Key.obj.transform.RotateAround(kvp.Key.pos, Vector3.forward, 90 * kvp.Key.rotation);
                items.Add(kvp.Key);
            }
            foreach (var item in items)
                itemsThatNeedInstantiation.Remove(item);
            yield return new WaitForEndOfFrame();
        }
        
        itemsThatNeedInstantiation.Clear();
    }

    // soft copy of seeded items
    public Item CopyItem(int index, bool instantiate = true)
    {
        var toCopy = itemPack.items[index];
        Item item = new Item();
        item.assetID = toCopy.assetID;
        item.isTarget = toCopy.isTarget;
        item.type = toCopy.type;
        item.shellcoreJSON = toCopy.shellcoreJSON;
        item.placeablesIndex = toCopy.placeablesIndex;
        item.name = toCopy.name;
        if (instantiate)
            item.obj = Instantiate(toCopy.obj);
        else
        {
            itemsThatNeedInstantiation.Add(item, toCopy.obj);
        }
        return item;
    }

    // hard copy
    public Item CopyItem(Item toCopy, bool instantiate = true)
    {
        Item item = new Item();
        item.ID = toCopy.ID;
        item.assetID = toCopy.assetID;
        item.isTarget = toCopy.isTarget;
        item.type = toCopy.type;
        item.faction = toCopy.faction;
        item.rotation = toCopy.rotation;
        item.shellcoreJSON = toCopy.shellcoreJSON;
        item.placeablesIndex = toCopy.placeablesIndex;
        item.pos = toCopy.pos;
        item.name = toCopy.name;
        if (instantiate)
            item.obj = Instantiate(toCopy.obj);
        else
        {
            itemsThatNeedInstantiation.Add(item, toCopy.obj);
        }
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
    int testIndex;

    private void OnEnable()
    {
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

    public override void OnInspectorGUI()
    {
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
        mode = GUILayout.Toolbar(mode, new string[] { "Add Mode", "View Mode" });
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Index:");
        testIndex = EditorGUILayout.IntField(testIndex);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Shift Up"))
        {
            var entry = handler.itemPack.items[testIndex];
            handler.itemPack.items.RemoveAt(testIndex);
            testIndex--;
            handler.itemPack.items.Insert(testIndex, entry);
        }

        if (GUILayout.Button("Shift Down"))
        {
            var entry = handler.itemPack.items[testIndex];
            handler.itemPack.items.RemoveAt(testIndex);
            testIndex++;
            handler.itemPack.items.Insert(testIndex, entry);
        }

        EditorGUILayout.EndHorizontal();
        switch (mode)
        {
            case 0:
                EditorGUILayout.BeginHorizontal();
                placeholder.obj = EditorGUILayout.ObjectField("Item appearance:",
                    placeholder.obj, typeof(GameObject), true) as GameObject;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.type = (ItemType)EditorGUILayout.EnumPopup("Item type: ", placeholder.type);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.name = EditorGUILayout.TextField("Asset Name:", placeholder.name);
                //placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.assetID = EditorGUILayout.TextField("Asset ID:", placeholder.assetID);
                // placeholder.assetID = EditorGUILayout.TextField("Asset ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.shellcoreJSON = EditorGUILayout.TextField("Ship JSON/Secondary data:", placeholder.shellcoreJSON);
                //placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID:", IDField.stringValue) as string;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                placeholder.vendingID = EditorGUILayout.TextField("Vending Blueprint ID (if any):", placeholder.vendingID);
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
                if (GUILayout.Button("Add Item"))
                {
                    if (handler.itemPack)
                    {
                        handler.itemPack.items.Add(placeholder);
                        placeholder = new Item();
                        ExportData();
                    }
                }

                ;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName("update");
                if (GUILayout.Button("Force Update Item Pack"))
                {
                    if (handler.itemPack)
                    {
                        ExportData();
                    }
                }

                ;
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

    private void ExportData()
    {
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
