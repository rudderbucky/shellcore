using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPropertyDisplay : MonoBehaviour
{
    public RectTransform rectTransform;
    public Dropdown factionDropdown;
    public InputField jsonField;
    public GameObject rotationButtons;
    public InputField nameField;
    public InputField idField;
    public GameObject pathButton;
    Item currentItem;
    bool editingDefaults = false;
    public List<GameObject> nonDefaults;
    public List<int> factionIDs;

    public void SetDefaults()
    {
        editingDefaults = true;
        if (!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.gameObject.SetActive(true);
        rectTransform.position = new Vector2(Screen.width / 2, Screen.height / 2);

        factionDropdown.transform.parent.gameObject.SetActive(true);
        jsonField.transform.parent.gameObject.SetActive(true);
        factionDropdown.value = PlayerPrefs.GetInt("WCItemPropertyDisplay_defaultFaction", 0);
        if (!FactionManager.FactionExists(factionDropdown.value))
        {
            factionDropdown.value = 0;
            PlayerPrefs.SetInt("WCItemPropertyDisplay_defaultFaction", 0);
        }

        jsonField.text = PlayerPrefs.GetString("WCItemPropertyDisplay_defaultJSON", "");
        foreach (var nonDefault in nonDefaults)
        {
            nonDefault.SetActive(false);
        }
    }

    public void LoadFactions()
    {
        factionIDs = new List<int>();
        UpdateFactionIDList(factionIDs);
        AddCustomFactionsToDropdown(factionDropdown);
    }

    void Awake()
    {
        if (!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        LoadFactions();

        if (editingDefaults)
        {
            factionDropdown.value = PlayerPrefs.GetInt("WCItemPropertyDisplay_defaultFaction", 0);
            if (!FactionManager.FactionExists(factionDropdown.value))
            {
                factionDropdown.value = 0;
                PlayerPrefs.SetInt("WCItemPropertyDisplay_defaultFaction", 0);
            }
        }
    }

    public static void UpdateFactionIDList(List<int> factionIDs)
    {
        if (factionIDs == null) return;
        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            if (FactionManager.FactionExists(i))
            {
                factionIDs.Add(i);
            }
        }

    }

    public static void AddCustomFactionsToDropdown(Dropdown factionDropdown)
    {
        factionDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < FactionManager.FactionArrayLength; i++)
        {
            if (FactionManager.FactionExists(i))
            {
                string option = FactionManager.GetFactionName(i);
                options.Add(option);
            }
        }

        factionDropdown.AddOptions(options);
    }

    void Update()
    {
        if (editingDefaults)
        {
            return;
        }

        var pos = Camera.main.WorldToScreenPoint(currentItem.pos);
        pos *= UIScalerScript.GetScale();
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
    }

    public void DisplayProperties(Item item)
    {
        currentItem = item;
        rectTransform.gameObject.SetActive(true);
        foreach (var nonDefault in nonDefaults)
        {
            nonDefault.SetActive(true);
        }

        var pos = Camera.main.WorldToScreenPoint(currentItem.pos);
        pos *= UIScalerScript.GetScale();
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
        AddCustomFactionsToDropdown(factionDropdown);
        factionDropdown.value = item.faction;
        jsonField.text = currentItem.shellcoreJSON;
        jsonField.transform.parent.gameObject.SetActive(item.type == ItemType.Other || item.type == ItemType.DecorationWithMetadata);
        rotationButtons.SetActive(item.type == ItemType.Platform);
        factionDropdown.transform.parent.gameObject.SetActive(item.type != ItemType.Flag && item.type != ItemType.Platform && item.type != ItemType.DecorationWithMetadata);
        idField.text = currentItem.ID;
        nameField.text = currentItem.name;
        pathButton.SetActive(item.type == ItemType.Other);
    }

    public void UpdateFaction()
    {
        if (editingDefaults)
        {
            return;
        }

        if (factionIDs != null)
        {
            currentItem.faction = factionIDs[factionDropdown.value];
        }
        
        Debug.Log("updated faction: " + currentItem.faction);
    }

    public void UpdateBlueprint()
    {
        if (editingDefaults)
        {
            return;
        }

        currentItem.shellcoreJSON = jsonField.text;
    }

    public void UpdateName()
    {
        currentItem.name = nameField.text;
        currentItem.ID = idField.text;
    }

    public void UpdatePatrolPath()
    {
        if (currentItem.patrolPath == null)
        {
            currentItem.patrolPath = new NodeEditorFramework.Standard.PathData();
            currentItem.patrolPath.waypoints = new List<NodeEditorFramework.Standard.PathData.Node>();
        }

        WorldCreatorCursor.finishPath += SetPath;
        WorldCreatorCursor.instance.pathDrawing(WorldCreatorCursor.WCCursorMode.Item, currentItem.patrolPath);
        rectTransform.gameObject.SetActive(false);
    }

    public void Hide()
    {
        if (editingDefaults)
        {
            //Debug.LogWarning(factionDropdown.value);
            PlayerPrefs.SetInt("WCItemPropertyDisplay_defaultFaction", factionDropdown.value);
            PlayerPrefs.SetString("WCItemPropertyDisplay_defaultJSON", jsonField.text);
            WorldCreatorCursor.instance.UpdateCurrentAppearanceToDefault();
        }

        rectTransform.gameObject.SetActive(false);
        editingDefaults = false;
    }

    public void SetPath(NodeEditorFramework.Standard.PathData path)
    {
        rectTransform.gameObject.SetActive(true);
        if (currentItem != null)
            currentItem.patrolPath = path;
        if (WorldCreatorCursor.finishPath != null)
            WorldCreatorCursor.finishPath -= SetPath;
    }

    public void SetRotation(int rotation)
    {
        currentItem.rotation = rotation / 90;
        currentItem.obj.transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    public void RemoveCurrent()
    {
        WorldCreatorCursor.instance.Remove(currentItem);
        currentItem = null;
        Hide();
    }
}
