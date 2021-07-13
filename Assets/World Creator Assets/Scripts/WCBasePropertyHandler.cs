using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class WCBasePropertyHandler : GUIWindowScripts
{
    [SerializeField]
    private GameObject menuButton;

    [SerializeField]
    private Transform menuContents;

    [SerializeField]
    private Transform fieldContents;

    [SerializeField]
    private GameObject inputFieldPrefab;

    [SerializeField]
    private GameObject colorFieldsPrefab;

    [SerializeField]
    private GameObject dropdownPrefab;

    [SerializeField]
    private FactionManager manager;

    private List<float> floats = new List<float>();
    private List<string> strings = new List<string>();
    private List<Color> colors = new List<Color>();
    private List<int> ints = new List<int>();

    [SerializeField]
    private Button addPropertyButton;

    [SerializeField]
    private WorldCreatorCursor cursor;

    // the display methods are similar to GUI.IntField etc.
    void DisplayFloat(int index, string label)
    {
        if (floats.Count <= index)
        {
            floats.Add(0);
        }

        var gObj = Instantiate(inputFieldPrefab, fieldContents);
        gObj.GetComponentInChildren<Text>().text = label;
        var field = gObj.GetComponentInChildren<InputField>();
        field.text = floats[index].ToString();
        field.contentType = InputField.ContentType.DecimalNumber;
        field.onEndEdit.AddListener((s) =>
        {
            floats[index] = DefaultTryParse(s);
            field.text = floats[index].ToString();
        });
        addPropertyButton.transform.SetAsLastSibling();
    }

    float DefaultTryParse(string s)
    {
        try
        {
            return float.Parse(s);
        }
        catch
        {
            return 0;
        }
    }

    void DisplayString(int index, string label)
    {
        if (strings.Count <= index)
        {
            strings.Add("");
        }

        var gObj = Instantiate(inputFieldPrefab, fieldContents);
        gObj.GetComponentInChildren<Text>().text = label;
        var field = gObj.GetComponentInChildren<InputField>();
        field.contentType = InputField.ContentType.Standard;
        field.text = strings[index];
        field.onEndEdit.AddListener((s) => strings[index] = s);
        addPropertyButton.transform.SetAsLastSibling();
    }

    void DisplayColors(int index, string label)
    {
        if (colors.Count <= index)
        {
            colors.Add(Color.black);
        }

        var gObj = Instantiate(colorFieldsPrefab, fieldContents);
        gObj.GetComponentInChildren<Text>().text = label;
        var fields = gObj.GetComponentsInChildren<InputField>();

        fields[0].onEndEdit.AddListener((s) => colors[index] = new Color(DefaultTryParse(s), colors[index].g, colors[index].b));
        fields[1].onEndEdit.AddListener((s) => colors[index] = new Color(colors[index].r, DefaultTryParse(s), colors[index].b));
        fields[2].onEndEdit.AddListener((s) => colors[index] = new Color(colors[index].r, colors[index].g, DefaultTryParse(s)));
        for (int i = 0; i < 3; i++)
        {
            colors[index] = new Color(Mathf.Max(0, Mathf.Min(1, colors[index].r)),
                Mathf.Max(0, Mathf.Min(1, colors[index].g)),
                Mathf.Max(0, Mathf.Min(1, colors[index].b)));
            fields[i].text = colors[index][i].ToString();
            var x = i;
            fields[x].onEndEdit.AddListener((s) =>
            {
                colors[index] = new Color(Mathf.Max(0, Mathf.Min(1, colors[index].r)),
                    Mathf.Max(0, Mathf.Min(1, colors[index].g)),
                    Mathf.Max(0, Mathf.Min(1, colors[index].b)));
                fields[x].text = colors[index][x].ToString();
            });
        }

        addPropertyButton.transform.SetAsLastSibling();
    }

    void DisplayIntDropdown(int index, List<string> names, string label)
    {
        if (ints.Count <= index)
        {
            ints.Add(0);
        }

        var gObj = Instantiate(dropdownPrefab, fieldContents);
        gObj.GetComponentInChildren<Text>().text = label;
        var dropdown = gObj.GetComponentInChildren<Dropdown>();
        dropdown.AddOptions(names);
        dropdown.value = ints[index];
        dropdown.onValueChanged.AddListener((i) => ints[index] = i);
        addPropertyButton.transform.SetAsLastSibling();
    }

    void DisplayInt(int index, string label)
    {
        if (ints.Count <= index)
        {
            ints.Add(0);
        }

        var gObj = Instantiate(inputFieldPrefab, fieldContents);
        gObj.GetComponentInChildren<Text>().text = label;
        var field = gObj.GetComponentInChildren<InputField>();
        field.text = ints[index].ToString();
        field.contentType = InputField.ContentType.IntegerNumber;
        field.onEndEdit.AddListener((s) =>
        {
            ints[index] = (int)DefaultTryParse(s);
            field.text = ints[index].ToString();
        });
        addPropertyButton.transform.SetAsLastSibling();
    }

    public enum Mode
    {
        Characters,
        Factions,
        Miscellaneous
    }

    Mode currentMode;

    public void SetMode(Mode mode)
    {
        currentMode = mode;
    }

    float testFloat;

    void OnEnable()
    {
        SetupMenu();
    }

    // enter in the property data into the lists and set up the fields
    public void DisplaySelectedProperty(int index)
    {
        floats.Clear();
        strings.Clear();
        ints.Clear();
        colors.Clear();
        switch (currentMode)
        {
            case Mode.Characters:
                var character = cursor.characters[index];
                strings.Add(character.ID);
                strings.Add(character.name);
                strings.Add(character.blueprintJSON);
                ints.Add(character.faction);
                strings.Add(character.partyData.attackDialogue);
                strings.Add(character.partyData.defendDialogue);
                strings.Add(character.partyData.buildDialogue);
                strings.Add(character.partyData.collectDialogue);
                strings.Add(character.partyData.followDialogue);
                SetupAdditionFields();
                break;
            case Mode.Factions:
                var faction = manager.factions[index];
                ints.Add(faction.ID);
                strings.Add(faction.factionName);
                colors.Add(faction.color);
                colors.Add(faction.shinyColor);
                strings.Add(faction.colorName);
                ints.Add(faction.relations);
                SetupAdditionFields();
                break;
        }
    }

    // add/modify the selected item
    public void ParseCurrent()
    {
        switch (currentMode)
        {
            case Mode.Characters:
                WorldData.CharacterData newChar = new WorldData.CharacterData();
                newChar.ID = strings[0];
                newChar.name = strings[1];
                newChar.blueprintJSON = strings[2];
                newChar.faction = ints[0];
                newChar.partyData = new WorldData.PartyData();
                newChar.partyData.attackDialogue = strings[3];
                newChar.partyData.defendDialogue = strings[4];
                newChar.partyData.collectDialogue = strings[5];
                newChar.partyData.buildDialogue = strings[6];
                newChar.partyData.followDialogue = strings[7];
                var existingChar = cursor.characters.Find(c => c.ID == newChar.ID);
                if (existingChar != null)
                {
                    cursor.characters.Remove(existingChar);
                }

                cursor.characters.Add(newChar);
                SetupMenu();
                break;
            case Mode.Factions:
                if (ints[0] <= 2)
                {
                    Debug.LogWarning("Modifying the default three factions");
                    //break;
                }

                Faction newFaction = ScriptableObject.CreateInstance<Faction>();
                newFaction.ID = ints[0];
                newFaction.factionName = strings[0];
                newFaction.color = colors[0];
                newFaction.shinyColor = colors[1];
                newFaction.colorName = strings[1];
                newFaction.relations = ints[1];
                var factionList = new List<Faction>(manager.factions);
                var existingFaction = factionList.Find(f => f.ID == newFaction.ID);
                if (existingFaction != null)
                {
                    factionList.Remove(existingFaction);
                }

                factionList.Insert(newFaction.ID, newFaction);
                manager.factions = factionList.ToArray();
                if (!File.Exists(System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt")))
                {
                    File.Create(System.IO.Path.Combine(Application.streamingAssetsPath, "ResourceDataPlaceholder.txt"));
                }

                Directory.CreateDirectory(System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder"));
                File.WriteAllText(System.IO.Path.Combine(
                        System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder"), $"{newFaction.factionName}.json"),
                    JsonUtility.ToJson(newFaction)
                );
                SetupMenu();
                break;
            case Mode.Miscellaneous:
                break;
        }
    }

    // displays modification fields similar to GUI.fields
    void SetupAdditionFields()
    {
        foreach (Transform child in fieldContents)
        {
            if (child.GetComponentInChildren<Button>() != addPropertyButton)
            {
                Destroy(child.gameObject);
            }
        }

        switch (currentMode)
        {
            case Mode.Characters:
                addPropertyButton.GetComponentInChildren<Text>().text = "Add/modify character";
                DisplayString(0, "ID: ");
                DisplayString(1, "Name: ");
                DisplayString(2, "Default blueprint: ");
                var factions = new List<string>();
                foreach (var faction in manager.factions)
                {
                    factions.Add(faction.factionName);
                }

                DisplayIntDropdown(0, factions, "Faction: ");
                DisplayString(3, "Attack dialogue: ");
                DisplayString(4, "Defend dialogue: ");
                DisplayString(5, "Collect dialogue: ");
                DisplayString(6, "Build turrets/tanks dialogue: ");
                DisplayString(7, "Follow dialogue: ");
                break;
            case Mode.Factions:
                addPropertyButton.GetComponentInChildren<Text>().text = "Add/modify faction";
                DisplayInt(0, "ID: ");
                DisplayString(0, "Name: ");
                DisplayColors(0, "Faction color: ");
                DisplayColors(1, "Shiny color: ");
                DisplayString(1, "Color name: ");
                DisplayInt(1, "Relations: ");
                break;
            case Mode.Miscellaneous:
                break;
        }
    }

    void DeleteSelectedProperty(int index)
    {
        switch (currentMode)
        {
            case Mode.Characters:
                cursor.characters.RemoveAt(index);
                break;
            case Mode.Factions:
                var newFaction = new Faction[manager.factions.Length - 1];
                for (int i = 0; i < index; i++)
                {
                    newFaction[i] = manager.factions[i];
                }

                for (int i = index + 1; i < manager.factions.Length; i++)
                {
                    newFaction[i - 1] = manager.factions[i];
                }

                var factionPath = System.IO.Path.Combine(
                    System.IO.Path.Combine(Application.streamingAssetsPath, "FactionPlaceholder"), $"{manager.factions[index].factionName}.json");
                if (File.Exists(factionPath))
                {
                    File.Delete(factionPath);
                }

                manager.factions = newFaction;
                break;
        }

        SetupMenu();
    }

    // sets up the menu containing the relevant items
    void SetupMenu()
    {
        foreach (Transform child in menuContents)
        {
            Destroy(child.gameObject);
        }

        IEnumerable<IBaseProperty> baseList = null;
        switch (currentMode)
        {
            case Mode.Characters:
                baseList = cursor.characters;
                break;
            case Mode.Factions:
                baseList = manager.factions;
                break;
        }

        int i = 0;
        foreach (var obj in baseList)
        {
            var index = i;
            i++;
            var gObj = Instantiate(menuButton, menuContents);
            gObj.GetComponentInChildren<Text>().text = obj.GetName();
            gObj.GetComponentInChildren<Button>().onClick.AddListener(() => { DisplaySelectedProperty(index); });
            gObj.transform.Find("Clear").GetComponentInChildren<Button>().onClick.AddListener(() => { DeleteSelectedProperty(index); });
        }

        SetupAdditionFields();
    }
}

public interface IBaseProperty
{
    public string GetName();
}
