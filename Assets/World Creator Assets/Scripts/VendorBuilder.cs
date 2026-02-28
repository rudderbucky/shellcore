using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VendorBuilder : GUIWindowScripts
{
    public static VendorBuilder instance;
    [SerializeField] private Transform vendorItemContent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private InputField vendorBlueprintInputField;
    [SerializeField] private InputField rangeInputField;

    [SerializeField] private InputField itemBlueprintInputField;
    [SerializeField] private InputField imageIDInputField;
    [SerializeField] private InputField itemCostBlueprintInputField;
    [SerializeField] private InputField descriptionInputField;
    [SerializeField] private Dropdown equivalentDropdown;
    
    [SerializeField] private Button selectedItemButton;
    private Color32 selectedColor = new Color32(100, 150, 100, 255);
    private Color32 unselectedColor = new Color32(100, 100, 100, 255);

    [SerializeField] private VendingBlueprint.Item currentVendingItem;
    private VendingBlueprintData currentVendorBlueprint;
    private VendorList currentVendor;

    void OnEnable()
    {
        ClearVendor();
        instance = this;

        if (equivalentDropdown.options.Count == 0)
        {
            foreach (var id in System.Enum.GetValues(typeof(VendingBlueprint.Item.AIEquivalent)))
            {
                equivalentDropdown.options.Add(new Dropdown.OptionData
                    (id.ToString()));
            }
        }
    }

    void OnDisable()
    {
        ClearVendor();
    }

    void ClearVendor()
    {
        for (int i = 0; i < vendorItemContent.childCount; i++)
        {
            Destroy(vendorItemContent.GetChild(i).gameObject);
        }

        currentVendor = null;
        currentVendorBlueprint = null;
        currentVendingItem = null;
        selectedItemButton = null;
    }

    public void ResetSelectedVendorItem()
    {
        currentVendingItem = null;
        if (selectedItemButton != null)
        {
            selectedItemButton.GetComponent<Image>().color = unselectedColor;
            selectedItemButton = null;
        }
    }

    public void RemoveSelectedVendorItem()
    {
        if (currentVendingItem == null || selectedItemButton == null)
            return;

        Destroy(selectedItemButton.gameObject);
        selectedItemButton = null;
        currentVendingItem = null;
    }

    public void SetModifyVendorItemFields(VendingBlueprint.Item vendorItem)
    {
        itemBlueprintInputField.text = vendorItem.json;
        imageIDInputField.text = vendorItem.icon;
        descriptionInputField.text = vendorItem.description;
        itemCostBlueprintInputField.text = vendorItem.cost.ToString();
        equivalentDropdown.value = ((int)vendorItem.equivalentTo);
        currentVendingItem = vendorItem;
    }

    // Button for creating/editing spawns
    public void AddOrModifyVendorItem()
    {
        var newSpawn = TryParseFields((itemBlueprintInputField, imageIDInputField, descriptionInputField, itemCostBlueprintInputField, equivalentDropdown));
        if (newSpawn == null)
            return;

        if (currentVendingItem != null)
        {
            currentVendingItem = newSpawn;
            if (selectedItemButton)
                selectedItemButton.GetComponentInChildren<Text>().text = itemBlueprintInputField.text;
            ResetSelectedVendorItem();
        }
        else
        {
            currentVendorBlueprint.items.Add(newSpawn);
            AddVendingItemToTable(newSpawn);
        }
    }

    // Adds new vendor item button
    private void AddVendingItemToTable(VendingBlueprint.Item vendorItem)
    {
        var button = Instantiate(buttonPrefab, vendorItemContent.transform).GetComponent<Button>();
        button.GetComponentInChildren<Text>().text = vendorItem.json;
        button.onClick.AddListener(() =>
        {
            if (Input.GetKey(KeyCode.LeftShift) && currentVendingItem != null)
            {
                currentVendorBlueprint.items.Remove(currentVendingItem);
                if (currentVendingItem == vendorItem)
                    currentVendingItem = null;
                Destroy(button.gameObject);
                return;
            }

            var doNotSetModify = vendorItem == GetCurrentVendorItem();
            ResetSelectedVendorItem();
            if (doNotSetModify)
                return;

            currentVendingItem = vendorItem;
            selectedItemButton = button;
            selectedItemButton.GetComponent<Image>().color = selectedColor;
            SetModifyVendorItemFields(vendorItem);
        });
    }

    // Writes vendor as JSON
    public void ParseVendor(string path)
    {
        string itemList = JsonUtility.ToJson(currentVendorBlueprint);
        currentVendor.entityBlueprint = vendorBlueprintInputField.text;
        currentVendor.vendingBlueprint = itemList;
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(currentVendor));
    }

    public void ReadVendor(VendorList vendor)
    {
        ClearVendor();

        currentVendor = vendor;
        vendorBlueprintInputField.text = vendor.entityBlueprint;
        currentVendorBlueprint = JsonUtility.FromJson<VendingBlueprintData>(vendor.vendingBlueprint);
        rangeInputField.text = currentVendorBlueprint.range.ToString();

        foreach (var item in currentVendorBlueprint.items)
        {
            AddVendingItemToTable(item);
        }
    }

    private VendingBlueprint.Item GetCurrentVendorItem()
    {
        return currentVendingItem;
    }

    private VendingBlueprint.Item TryParseFields((InputField, InputField, InputField, InputField, Dropdown) field)
    {
        if (string.IsNullOrEmpty(field.Item1.text))
        {
            return null;
        }

        VendingBlueprint.Item newItem = new VendingBlueprint.Item();
        newItem.json = field.Item1.text;
        newItem.icon = field.Item2.text;
        newItem.description = field.Item3.text;
        int.TryParse(field.Item4.text, out newItem.cost);
        newItem.equivalentTo = (VendingBlueprint.Item.AIEquivalent)field.Item5.value;

        return newItem;
    }
}
