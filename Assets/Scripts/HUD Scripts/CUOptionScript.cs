using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CUOptionScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public GameObject tooltipPrefab;
    public string coreID;
    public Image shell;
    private GameObject tooltip;
    public PlayerCore player;
    public int repCost;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(player.reputation >= repCost) {
            player.blueprint.coreShellSpriteID = coreID;
            player.Rebuild();
            CoreUpgraderScript.DrawScreen();
        } else Debug.Log("Not enough reputation!" + player.reputation + " " + repCost);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip = Instantiate(tooltipPrefab);
        tooltip.GetComponent<Image>().color = Color.grey;
        RectTransform rect = tooltip.GetComponent<RectTransform>();
        rect.position = eventData.position;
        rect.SetParent(transform.parent, true);
        rect.SetAsLastSibling();

        Text text = tooltip.transform.Find("Text").GetComponent<Text>();
        text.text = CoreUpgraderScript.GetDescription(coreID);
        
        rect.sizeDelta = new Vector2(text.preferredWidth + 16f, text.preferredHeight + 16);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //delete tooltip
        if (tooltip)
            Destroy(tooltip);
    }

    // Start is called before the first frame update
    void Start()
    {        
        shell.sprite = ResourceManager.GetAsset<Sprite>(coreID);
        shell.SetNativeSize();
        shell.rectTransform.anchoredPosition = -shell.sprite.pivot + shell.rectTransform.sizeDelta / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if(tooltip)
        {
            tooltip.transform.position = Input.mousePosition;
        }        
    }
}
