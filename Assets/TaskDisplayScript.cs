using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskDisplayScript : MonoBehaviour
{
    public Transform contents;
    public GameObject displayPrefab;

    void OnEnable() {
        for(int i = 0; i < contents.childCount; i++) {
            Destroy(contents.GetChild(i).gameObject);
        }

        foreach(KeyValuePair<string, string> pair in StatusMenu.taskInfo) {
            var obj = Instantiate(displayPrefab, contents, false);
            var strings = obj.GetComponentsInChildren<Text>();
            strings[0].text = pair.Key;
            strings[1].text = pair.Value;
        }
    }
}
