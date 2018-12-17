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
    public PlayerCore player;
    public Vector3 outpostPosition;
    private GameObject UI;
    private Transform background;
    private bool opened;
    private GameObject[] buttons;

    public void openUI()
    {
        if (UI)
            return;

        UI = Instantiate(UIPrefab);
        background = UI.transform.Find("Background");
        Button close = background.transform.Find("Close").GetComponent<Button>();
        close.onClick.AddListener(closeUI);
        buttons = new GameObject[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            buttons[i] = Instantiate(buttonPrefab);

            RectTransform rt = buttons[i].GetComponent<RectTransform>();
            rt.SetParent(background, false);
            rt.anchoredPosition = new Vector2(16 + i * 64, -16 + (i > 6 ? 96 : 0));

            Button button = buttons[i].GetComponent<Button>();
            button.onClick.AddListener(() => { onButtonPressed(index); });

            Image sr = buttons[i].transform.Find("Icon").GetComponent<Image>();
            sr.sprite = items[i].icon;
        }
        opened = true;
    }

    private void Update()
    {
        if (opened)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if(player.GetPower() < items[i].cost)
                {
                    buttons[i].GetComponent<Image>().color = new Color(0, 0, 0.4F);
                } else buttons[i].GetComponent<Image>().color = Color.white;
            }
        } 
    }

    public void closeUI()
    {
        opened = false;
        Destroy(UI);
    }

    public void onButtonPressed(int index)
    {
        //TODO: construct entity from blueprint
        //TODO: add sprites and all necessary prefab IDs to the blueprint
        if (player.GetPower() >= items[index].cost)
        {
            GameObject creation = Instantiate(items[index].prefab);
            creation.transform.position = outpostPosition;
            //creation.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1F,2) * 1000, Random.Range(-1F, 2) * 1000));
            creation.GetComponent<Entity>().spawnPoint = outpostPosition;
            player.SetTractorTarget(creation.GetComponent<Draggable>());
            player.AddPower(-items[index].cost);
            closeUI();
        }
    }
}
