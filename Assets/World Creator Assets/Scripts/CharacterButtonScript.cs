using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonScript : MonoBehaviour
{
    public WorldData.CharacterData character;
    public WorldCreatorCursor cursor;
    public Text text;
    public GameObject itemObj;

    void Start()
    {
        text.text = character.name;
    }

    public void Instantiate()
    {
        if (Input.GetMouseButton(1))
        {
            Remove();
            return;
        }

        Item item = new Item();
        item.ID = character.ID;
        item.name = character.name;
        item.type = ItemType.Other;
        item.obj = Instantiate(itemObj);

        cursor.SetCurrent(item);
    }

    public void Remove()
    {
        if (cursor.characters.Contains(character))
        {
            cursor.characters.Remove(character);
        }

        Destroy(gameObject);
    }

    public void EditCharacter()
    {
        WCCharacterHandler.EditCharacter(character);
    }
}
