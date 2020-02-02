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
    public InputField sectorMusicID;
    public Toggle sectorMusicBool;
    public InputField x;
    public InputField y;
    public InputField w;
    public InputField h;
    public InputField colorR;
    public InputField colorG;
    public InputField colorB;
    
    Vector2 mousePos;

    void Start() 
    {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();
    }

    public void DisplayProperties(Sector sector) {
        if(!rectTransform) rectTransform = GetComponent<RectTransform>();
        currentSector = sector;
        gameObject.SetActive(true);
        mousePos = WorldCreatorCursor.GetMousePos();
        var pos = Camera.main.WorldToScreenPoint(mousePos);
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;

        type.value = (int)sector.type;
        sectorName.text = sector.sectorName;
        sectorMusicBool.isOn = sector.hasMusic;
        sectorMusicID.text = sector.musicID;

        x.text = currentSector.bounds.x + "";
        y.text = currentSector.bounds.y + "";
        w.text = currentSector.bounds.w + "";
        h.text = currentSector.bounds.h + "";
        colorR.text = currentSector.backgroundColor.r + "";
        colorG.text = currentSector.backgroundColor.g + "";
        colorB.text = currentSector.backgroundColor.b + "";
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
        currentSector.backgroundColor = SectorColors.colors[type.value];
        colorR.text = currentSector.backgroundColor.r + "";
        colorG.text = currentSector.backgroundColor.g + "";
        colorB.text = currentSector.backgroundColor.b + "";
    }

    public void UpdateName() 
    {
        currentSector.sectorName = sectorName.text;
    }

    public void UpdateMusic() 
    {
        currentSector.musicID = sectorMusicID.text;
    }

    public void UpdateMusicBool()
    {
        currentSector.hasMusic = sectorMusicBool.isOn;
    }

    public void UpdateColor()
    {
        currentSector.backgroundColor = new Color(float.Parse(colorR.text), float.Parse(colorG.text), float.Parse(colorB.text), 1);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
