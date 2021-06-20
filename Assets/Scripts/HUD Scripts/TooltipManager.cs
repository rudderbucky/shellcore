using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    [SerializeField]
    private GameObject tooltipPrefab;
    private Transform tooltipTransform;
    private Dictionary<Rect, string> boundsDict = new Dictionary<Rect, string>();
    void Awake()
    {
        if(!tooltipPrefab) tooltipPrefab = ResourceManager.GetAsset<GameObject>("tooltip_prefab");
    }

    void Update()
    {
        foreach(var kvp in boundsDict)
        {
            if(kvp.Key.Contains(Input.mousePosition))
            {
                SetTooltip(kvp.Value);
                return;
            }
        }
        if(tooltipTransform) Destroy(tooltipTransform.gameObject);
    }

    // Adds the rect into the scanning area for tooltips. If the rect is already present, it simply adjusts the tooltip string
    public void AddBounds(Rect bound, string tooltipString)
    {
        if(!boundsDict.ContainsKey(bound))
            boundsDict.Add(bound, tooltipString);
        else boundsDict[bound] = tooltipString;
    }

    // Moves the tooltip to the correct position and displays the text
    private void SetTooltip(string displayText, int scale=1)
    {
        if(!tooltipTransform) tooltipTransform = Instantiate(tooltipPrefab, transform.parent).GetComponent<RectTransform>();
        tooltipTransform.position = Input.mousePosition;                                      
        var text = tooltipTransform.GetComponentInChildren<Text>();
        tooltipTransform.localScale = text.rectTransform.localScale = new Vector3(scale, 1, 1); 
        text.text = 
            $"{displayText}".ToUpper();
        tooltipTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(text.preferredWidth + 16, text.preferredHeight + 16);
    }
}
