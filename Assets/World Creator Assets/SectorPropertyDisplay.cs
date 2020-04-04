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
    public InputField waveSet;
    public GameObject bgSpawnInputFieldPrefab;
    public List<(InputField, Dropdown)> bgSpawnInputFields = new List<(InputField, Dropdown)>();
    public Transform bgSpawnScrollContents;
    public RectTransform mainContents;
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
        waveSet.text = JsonUtility.ToJson(sector.waveSet);

        x.text = currentSector.bounds.x + "";
        y.text = currentSector.bounds.y + "";
        w.text = currentSector.bounds.w + "";
        h.text = currentSector.bounds.h + "";
        colorR.text = currentSector.backgroundColor.r + "";
        colorG.text = currentSector.backgroundColor.g + "";
        colorB.text = currentSector.backgroundColor.b + "";
        UpdateBGSpawns();
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

    public void Hide() 
    {
        gameObject.SetActive(false);
    }

    public void AddBGSpawn()
    {
        AddBGSpawn(null, 1);
    }

    public void AddBGSpawn(string text = null, int faction = 1) 
    {
        var field = Instantiate(bgSpawnInputFieldPrefab, bgSpawnScrollContents).GetComponentInChildren<InputField>();
        var drop = field.transform.parent.GetComponentInChildren<Dropdown>();
        bgSpawnInputFields.Add((field, drop));
        field.text = text;
        drop.value = faction;
        Canvas.ForceUpdateCanvases();
        bgSpawnScrollContents.GetComponent<RectTransform>().sizeDelta = new Vector2(100, bgSpawnScrollContents.GetComponent<VerticalLayoutGroup>().minHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainContents);
    }

    public void ClearBGSpawns() 
    {
        foreach(var field in bgSpawnInputFields)
        {
            Destroy(field.Item1.transform.parent.gameObject);
        }
        bgSpawnInputFields.Clear();
    }

    public void TryParseBGSpawns()
    {
        List<Sector.LevelEntity> levelEntities = new List<Sector.LevelEntity>();
        foreach(var field in bgSpawnInputFields)
        {
            if(field.Item1.text == null || field.Item1.text == "") continue;
            var item = ItemHandler.instance.items.Find((it) => {return it.assetID == field.Item1.text;});

            if(item != null)
            {
                Sector.LevelEntity ent = new Sector.LevelEntity();

                // you can choose to give any object a custom name
                if(item.name != null && item.name != "")
                    ent.name = item.name;
                else ent.name = item.obj.name;
                ent.faction = field.Item2.value; // maybe change this later
                ent.assetID = item.assetID;
                levelEntities.Add(ent);
            }
            else
            {
                try
                {
                    EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
                    JsonUtility.FromJsonOverwrite(field.Item1.text, blueprint);
                    blueprint.intendedType = EntityBlueprint.IntendedType.ShellCore; // for good measure :)

                    Sector.LevelEntity ent = new Sector.LevelEntity();
                    ent.name = blueprint.entityName;
                    ent.assetID = "shellcore_blueprint";
                    ent.blueprintJSON = JsonUtility.ToJson(blueprint);
                    ent.faction = field.Item2.value; // maybe change this later
                    levelEntities.Add(ent);
                } 
                catch(System.Exception e)
                {
                    Debug.LogWarning(e);
                    continue;
                }
            }
        }

        currentSector.backgroundSpawns = new Sector.BackgroundSpawn[levelEntities.Count];
        var i = 0;
        foreach(var ent in levelEntities)
        {
            currentSector.backgroundSpawns[i].entity = ent;
            currentSector.backgroundSpawns[i].timePerSpawn = 8;
            currentSector.backgroundSpawns[i].radius = 15;
            i++;
        }

        UpdateBGSpawns();
    }

    public void UpdateBGSpawns()
    {
        ClearBGSpawns();
        foreach(var bgSpawn in currentSector.backgroundSpawns)
        {
            if(bgSpawn.entity.assetID != "shellcore_blueprint" 
                && ItemHandler.instance.items.Exists((item) => {return item.assetID == bgSpawn.entity.assetID;}))
            {
                AddBGSpawn(bgSpawn.entity.assetID, bgSpawn.entity.faction);
            }
            else AddBGSpawn(bgSpawn.entity.blueprintJSON, bgSpawn.entity.faction);
        }
    }

    public void UpdateWaveSet()
    {
        currentSector.waveSet = JsonUtility.FromJson<WaveSet>(waveSet.text);
    }
}
