using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WCCharacterHandler : MonoBehaviour
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
    private WorldData.CharacterData currentData = new WorldData.CharacterData();
    // Start is called before the first frame update

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
        UpdateFields();
    }

    public void UpdateFields()
    {
        UpdateCharID();
        UpdateCharName();
        UpdateCharBlueprint();
        UpdateCharPartyMember();
        UpdateCharFaction();
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
