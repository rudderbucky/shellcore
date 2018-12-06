using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutpostUI : MonoBehaviour
{
    [System.Serializable]
    public struct Item
    {
        public GameObject prefab; //TODO: replace this with blueprint
        public Sprite icon;
        public int cost;
    }

    public List<Item> items;
    public GameObject UIPrefab;
    public GameObject buttonPrefab;

    private GameObject UI;
    private Transform background;

    public void openUI()
    {
        if (UI)
            return;

        UI = Instantiate(UIPrefab);
        background = UI.transform.Find("Background");
        Button close = background.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(closeUI);

        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            GameObject itemButton = Instantiate(buttonPrefab);

            RectTransform rt = itemButton.GetComponent<RectTransform>();
            rt.SetParent(background, false);
            rt.anchoredPosition = new Vector2(16 + i * 64, -16 + (i > 6 ? 96 : 0));

            Button button = itemButton.GetComponent<Button>();
            button.onClick.AddListener(() => { onButtonPressed(index); });

            Image sr = itemButton.transform.Find("Icon").GetComponent<Image>();
            sr.sprite = items[i].icon;
        }
    }

    public void closeUI()
    {
        Destroy(UI);
    }

    public void onButtonPressed(int index)
    {
        //TODO: outpost item cost
        //TODO: construct entity from blueprint
        //TODO: add sprites and all necessary prefab IDs to the blueprint

        GameObject creation = Instantiate(items[index].prefab);
        creation.transform.position = transform.position;
        creation.GetComponent<Entity>().spawnPoint = transform.position;
        //TODO: auto tractor beam to turrets
        closeUI();
    }
}
