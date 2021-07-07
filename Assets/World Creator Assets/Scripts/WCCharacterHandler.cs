using UnityEngine;
using UnityEngine.UI;

public class WCCharacterHandler : GUIWindowScripts
{
    public static WCCharacterHandler instance;
    public WorldCreatorCursor cursor;
    public GameObject buttonPrefab;
    public InputField charID;
    public InputField charName;
    public InputField charBlueprint;
    public Toggle charPartyMember;
    public Transform content;
    public Dropdown charFaction;
    public InputField attackDialogue;
    public InputField defendDialogue;
    public InputField collectDialogue;
    public InputField buildDialogue;
    public InputField followDialogue;

    private WorldData.CharacterData currentData = new WorldData.CharacterData();
    // Start is called before the first frame update

    void ClearFieldData()
    {
        currentData = new WorldData.CharacterData()
        {
            partyData = new WorldData.PartyData()
        };
        reflectData();
    }

    void OnEnable()
    {
        ClearFieldData();
    }

    void UpdateCharID()
    {
        currentData.ID = charID.text;
    }

    void UpdateCharName()
    {
        currentData.name = charName.text;
    }

    void UpdateCharBlueprint()
    {
        currentData.blueprintJSON = charBlueprint.text;
    }

    void UpdateCharFaction()
    {
        currentData.faction = charFaction.value;
    }

    void UpdateAttackDialogue()
    {
        if (currentData.partyData == null)
        {
            currentData.partyData = new WorldData.PartyData();
        }

        currentData.partyData.attackDialogue = attackDialogue.text;
    }

    void UpdateDefendDialogue()
    {
        if (currentData.partyData == null)
        {
            currentData.partyData = new WorldData.PartyData();
        }

        currentData.partyData.defendDialogue = defendDialogue.text;
    }

    void UpdateCollectDialogue()
    {
        if (currentData.partyData == null)
        {
            currentData.partyData = new WorldData.PartyData();
        }

        currentData.partyData.collectDialogue = collectDialogue.text;
    }

    void UpdateBuildDialogue()
    {
        if (currentData.partyData == null)
        {
            currentData.partyData = new WorldData.PartyData();
        }

        currentData.partyData.buildDialogue = buildDialogue.text;
    }

    void UpdateFollowDialogue()
    {
        if (currentData.partyData == null)
        {
            currentData.partyData = new WorldData.PartyData();
        }

        currentData.partyData.followDialogue = followDialogue.text;
    }

    void Awake()
    {
        instance = this;
    }

    public void AddCharacter(bool updateFields = true)
    {
        if (updateFields)
        {
            UpdateFields();
        }

        if (!cursor.characters.Contains(currentData))
        {
            cursor.characters.Add(currentData);
            var button = Instantiate(buttonPrefab, content).GetComponentInChildren<CharacterButtonScript>();
            button.character = currentData;
            button.cursor = cursor;
            button.transform.Find("Clear").GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                DeleteCharacter(button.character);
                Destroy(button.gameObject);
            }));
        }

        currentData = new WorldData.CharacterData()
        {
            partyData = new WorldData.PartyData()
        };
        charID.text = charName.text = charBlueprint.text = "";
        charPartyMember.isOn = false;
        ClearFieldData();
    }

    public void UpdateFields()
    {
        UpdateCharID();
        UpdateCharName();
        UpdateCharBlueprint();
        UpdateCharFaction();
        UpdateAttackDialogue();
        UpdateDefendDialogue();
        UpdateCollectDialogue();
        UpdateBuildDialogue();
        UpdateFollowDialogue();
    }

    public void DeleteCharacter(WorldData.CharacterData character)
    {
        cursor.characters.Remove(character);
    }

    public void ReflectButtonData()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        foreach (var ch in cursor.characters)
        {
            AddCharacter(ch);
        }
    }

    public static void ReflectData()
    {
        instance.reflectData();
    }

    public void reflectData()
    {
        charID.text = currentData.ID;
        charName.text = currentData.name;
        charBlueprint.text = currentData.blueprintJSON;
        charFaction.value = currentData.faction;
        if (currentData != null && currentData.partyData != null)
        {
            attackDialogue.text = currentData.partyData.attackDialogue;
            defendDialogue.text = currentData.partyData.defendDialogue;
            collectDialogue.text = currentData.partyData.collectDialogue;
            buildDialogue.text = currentData.partyData.buildDialogue;
            followDialogue.text = currentData.partyData.followDialogue;
        }
        else
        {
            attackDialogue.text = defendDialogue.text = collectDialogue.text = buildDialogue.text = followDialogue.text = "";
        }
    }

    public void AddCharacter(WorldData.CharacterData data)
    {
        currentData = data;
        AddCharacter(false);
    }

    public static void EditCharacter(WorldData.CharacterData data)
    {
        instance.editCharacter(data);
    }

    private void editCharacter(WorldData.CharacterData data)
    {
        currentData = data;
        ReflectData();
    }

    public void EditCurrentBlueprint()
    {
        ShipBuilder.currentCharacter = currentData;
        cursor.ActivateShipBuilder();
    }
}
