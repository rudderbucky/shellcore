using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonScript : MonoBehaviour
{
    public WorldData.CharacterData character;
    public WorldCreatorCursor cursor;
    public Text text;
    void Start()
    {
        text.text = character.name;
    }

    public void Instantiate()
    {
        if(Input.GetMouseButton(1))
        {
            Remove();
            return;
        }
    }

    public void Remove()
    {
        if(cursor.characters.Contains(character)) cursor.characters.Remove(character);
        Destroy(gameObject);
    }
}
