using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WCCharacterHandler : MonoBehaviour
{
    public WorldCreatorCursor cursor;
    public GameObject buttonPrefab;
    public InputField charID;
    public InputField charName;
    public InputField charBlueprint;
    public Toggle charPartyMember;
    public Transform content;
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

    public void AddCharacter(bool updateFields=true)
    {
        if(updateFields)
        {
            UpdateCharID();
            UpdateCharName();
            UpdateCharBlueprint();
            UpdateCharPartyMember();
        }

        cursor.characters.Add(currentData);
        var button = Instantiate(buttonPrefab, content).GetComponentInChildren<CharacterButtonScript>();
        button.character = currentData;
        button.cursor = cursor;

        currentData = new WorldData.CharacterData();
        charID.text = charName.text = charBlueprint.text = "";
        charPartyMember.isOn = false;
        UpdateCharID();
        UpdateCharName();
        UpdateCharBlueprint();
        UpdateCharPartyMember();
    }

    public void AddCharacter(WorldData.CharacterData data)
    {
        currentData = data;
        AddCharacter(false);
    }
}
