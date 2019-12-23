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
    public InputField x;
    public InputField y;
    public InputField w;
    public InputField h;



    void Start() {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();
        Hide();
    }


    Vector2 mousePos;
    public void DisplayProperties(Sector sector) {
        currentSector = sector;
        gameObject.SetActive(true);
        mousePos = WorldCreatorCursor.GetMousePos();
        var pos = Camera.main.WorldToScreenPoint(mousePos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;

        type.value = (int)sector.type;
        sectorName.text = sector.name;
        x.text = currentSector.bounds.x + "";
        y.text = currentSector.bounds.y + "";
        w.text = currentSector.bounds.w + "";
        h.text = currentSector.bounds.h + "";
    }

    void Update() {
        var pos = Camera.main.WorldToScreenPoint(mousePos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;

        x.text = currentSector.bounds.x + "";
        y.text = currentSector.bounds.y + "";
        w.text = currentSector.bounds.w + "";
        h.text = currentSector.bounds.h + "";
    }
    public void UpdateType() 
    {
        currentSector.type = (Sector.SectorType)type.value;
    }

    public void UpdateName() 
    {
        currentSector.sectorName = sectorName.text;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
