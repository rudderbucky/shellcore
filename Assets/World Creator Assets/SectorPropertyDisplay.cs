using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SectorPropertyDisplay : MonoBehaviour
{
    public RectTransform rectTransform;
    Sector currentSector;
    Vector2 sectorCenter;
    public Dropdown type;
    public InputField sectorName;

    void Start() {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    public void DisplayProperties(Sector sector) {
        currentSector = sector;
        sectorCenter = new Vector2(currentSector.bounds.x + currentSector.bounds.h / 2, currentSector.bounds.y + currentSector.bounds.w / 2);
        gameObject.SetActive(true);
        var pos = Camera.main.WorldToScreenPoint(sectorCenter);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;

        type.value = (int)sector.type;
        sectorName.text = sector.name;
    }

    void Update() {
        var pos = Camera.main.WorldToScreenPoint(sectorCenter);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;
    }
    public void UpdateType() 
    {
        currentSector.type = (Sector.SectorType)type.value;
    }

    public void UpdateName() 
    {
        currentSector.name = sectorName.text;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
