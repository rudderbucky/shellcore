using System.Collections;
using System.Collections.Generic;
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

    void ClearData()
    {
        currentData = new WorldData.CharacterData();
        ReflectData();
    }

    void OnEnable()
    {
        ClearData();
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

    void UpdateCharPartyMember()
    {
        currentData.partyMember = charPartyMember.isOn;
    }

    void UpdateCharFaction()
    {
        currentData.faction = charFaction.value;
    }

    void UpdateAttackDialogue()
    {
        if(currentData.partyData == null) currentData.partyData = new WorldData.PartyData();
        currentData.partyData.attackDialogue = attackDialogue.text;
    }
    void UpdateDefendDialogue()
    {
        if(currentData.partyData == null) currentData.partyData = new WorldData.PartyData();
        currentData.partyData.defendDialogue = defendDialogue.text;
    }
    void UpdateCollectDialogue()
    {
        if(currentData.partyData == null) currentData.partyData = new WorldData.PartyData();
        currentData.partyData.collectDialogue = collectDialogue.text;
    }
    void UpdateBuildDialogue()
    {
        if(currentData.partyData == null) currentData.partyData = new WorldData.PartyData();
        currentData.partyData.buildDialogue = buildDialogue.text;
    }
    void UpdateFollowDialogue()
    {
        if(currentData.partyData == null) currentData.partyData = new WorldData.PartyData();
        currentData.partyData.followDialogue = followDialogue.text;
    }

    void Awake()
    {
        instance = this;
    }

    public void AddCharacter(bool updateFields=true)
    {
        if(updateFields)
        {
            UpdateFields();
        }

        if(!cursor.characters.Contains(currentData))
        {
            cursor.characters.Add(currentData);
            var button = Instantiate(buttonPrefab, content).GetComponentInChildren<CharacterButtonScript>();
            button.character = currentData;
            button.cursor = cursor;
        }

        currentData = new WorldData.CharacterData();
        charID.text = charName.text = charBlueprint.text = "";
        charPartyMember.isOn = false;
        if(updateFields)
            UpdateFields();
    }

    public void UpdateFields()
    {
        UpdateCharID();
        UpdateCharName();
        UpdateCharBlueprint();
        UpdateCharPartyMember();
        UpdateCharFaction();
        UpdateAttackDialogue();
        UpdateDefendDialogue();
        UpdateCollectDialogue();
        UpdateBuildDialogue();
        UpdateFollowDialogue();
    }

    public static void ReflectButtonData()
    {
        for(int i = 0; i < instance.content.childCount; i++)
        {
            Destroy(instance.content.GetChild(i).gameObject);
        }
        
        foreach(var ch in instance.cursor.characters)
        {
            instance.AddCharacter(ch);
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
        charPartyMember.isOn = currentData.partyMember;
        charFaction.value = currentData.faction;
        if(currentData != null && currentData.partyData != null)
        {
            attackDialogue.text = currentData.partyData.attackDialogue;
            defendDialogue.text = currentData.partyData.defendDialogue;
            collectDialogue.text = currentData.partyData.collectDialogue;
            buildDialogue.text = currentData.partyData.buildDialogue;
            followDialogue.text = currentData.partyData.followDialogue;
        }
        else
        {
            attackDialogue.text = 
            defendDialogue.text = 
            collectDialogue.text = 
            buildDialogue.text = 
            followDialogue.text = "";
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
