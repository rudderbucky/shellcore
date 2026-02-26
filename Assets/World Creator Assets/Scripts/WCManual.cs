using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class WCManual : GUIWindowScripts
{
    public Transform listContents;
    public GameObject buttonPrefab;
    public Text contentText;
    public Image contentPreview;

    void Start()
    {
        contentPreview.enabled = false;
        foreach (var entry in manualDatabase.manualEntries)
        {
            var button = Instantiate(buttonPrefab, listContents).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = entry.title;
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                if (!string.IsNullOrEmpty(entry.imageID))
                {
                    contentPreview.sprite = ResourceManager.GetAsset<Sprite>(entry.imageID);
                    contentPreview.enabled = true;
                }
                else
                {
                    contentPreview.enabled = false;
                }

                contentText.text = entry.title + "\n" + entry.contents + "\n";
            }));
        }
    }

    public ManualDatabase manualDatabase;
}