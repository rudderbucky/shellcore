using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskDisplayScript : MonoBehaviour
{
    public Transform contents;
    public GameObject displayPrefab;

    static TaskDisplayScript instance;
    void OnEnable() {
        instance = this;
        Activate();
    }

    public static void Activate() {
        if(instance) {
            for(int i = 0; i < instance.contents.childCount; i++) {
                Destroy(instance.contents.GetChild(i).gameObject);
            }

            foreach(Task task in StatusMenu.taskInfo) {
                var obj = Instantiate(instance.displayPrefab, instance.contents, false);
                var strings = obj.GetComponentsInChildren<Text>();
                strings[0].text = task.dialogue;
                strings[1].text = task.objectived;
                strings[0].color = task.dialogueColor;
            }
        }
    }
}
