using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WCItemFinder : GUIWindowScripts
{
    [SerializeField]
    private WorldCreatorCursor cursor;
    [SerializeField]
    private WorldCreatorCamera cameraScript;
    [SerializeField]
    private Dropdown searchType;
    [SerializeField]
    private InputField stringField;
    private int currentIndex = 0;

    private void OnEnable()
    {
        currentIndex = 0;
    }

    private void FindItemByID(string ID)
    {
        for(int i = 0; i < cursor.placedItems.Count; i++)
        {
            if(cursor.placedItems[i].ID == ID)
            {
                cameraScript.transform.position = cursor.placedItems[i].pos + new Vector3(0,0,cameraScript.transform.position.z);
                break;
            }
        }
    }

    private void FindItemByName(string name, int start=0)
    {
        for(int i = start; i < cursor.placedItems.Count; i++)
        {
            if(cursor.placedItems[i].name == name)
            {
                cameraScript.transform.position = cursor.placedItems[i].pos + new Vector3(0,0,cameraScript.transform.position.z);
                currentIndex = i+1;
                return;
            }
        }
        currentIndex = 0;
    }

    public void ExecuteSearch()
    {
        switch(searchType.value)
        {
            case 0:
                FindItemByID(stringField.text);
                break;
            case 1:
                FindItemByName(stringField.text, currentIndex);
                break;
        }
    }
}
