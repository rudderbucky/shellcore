﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SectorPropertyDisplay : MonoBehaviour
{
    public RectTransform rectTransform;
    Sector currentSector;
    Vector2 sectorCenter;
    public Dropdown type;
    public Dropdown particles;
    public Dropdown tiles;
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
    public GameObject deleteButton;
    public GameObject insertBGSpawnsButton;
    public GameObject clearBGSpawnsButton;
    public GameObject parseBGSpawnsButton;

    public List<(InputField, Dropdown)> bgSpawnInputFields = new List<(InputField, Dropdown)>();
    public List<int> factionIDs;
    public Transform bgSpawnScrollContents;
    public RectTransform mainContents;
    Vector2 mousePos;
    public List<InputField> shardCounts = new List<InputField>();

    bool opening = false;
    bool editingDefaults = false;

    void Start()
    {
        if (!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public void SetDefaults()
    {
        opening = true;
        editingDefaults = true;

        if (!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.gameObject.SetActive(true);
        rectTransform.position = new Vector2(Screen.width / 2, Screen.height / 2);

        type.value = 0;
        sectorMusicBool.isOn = PlayerPrefs.GetInt("WCSectorPropertyDisplay_defaultMusicOn", 1) == 1 ? true : false;
        sectorMusicID.text = PlayerPrefs.GetString("WCSectorPropertyDisplay_defaultMusic0", WCGeneratorHandler.GetDefaultMusic(Sector.SectorType.Neutral));
        colorR.text =
            PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultR0",
                SectorColors.colors[0].r) + "";
        colorB.text =
            PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultB0",
                SectorColors.colors[0].b) + "";
        colorG.text =
            PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultG0",
                SectorColors.colors[0].g) + "";

        particles.value = PlayerPrefs.GetInt("WCSectorPropertyDisplay_defaultParticles", 0);
        tiles.value = PlayerPrefs.GetInt("WCSectorPropertyDisplay_defaultTiles", 0);

        sectorName.transform.parent.gameObject.SetActive(false);
        waveSet.transform.parent.gameObject.SetActive(false);
        x.transform.parent.gameObject.SetActive(false);
        y.transform.parent.gameObject.SetActive(false);
        w.transform.parent.gameObject.SetActive(false);
        h.transform.parent.gameObject.SetActive(false);
        colorR.transform.parent.gameObject.SetActive(true);
        colorG.transform.parent.gameObject.SetActive(true);
        colorB.transform.parent.gameObject.SetActive(true);
        bgSpawnScrollContents.gameObject.SetActive(false);
        insertBGSpawnsButton.SetActive(false);
        clearBGSpawnsButton.SetActive(false);
        parseBGSpawnsButton.SetActive(false);
        deleteButton.SetActive(false);
        for (int i = 0; i < shardCounts.Count; i++)
        {
            shardCounts[i].gameObject.SetActive(false);
        }

        opening = false;
    }

    public void ReflectCurrentSector(bool xAxis)
    {
        WorldCreatorCursor.instance.SymmetryCopy(currentSector, xAxis);
    }

    public void DisplayProperties(Sector sector)
    {
        opening = true;

        if (!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        currentSector = sector;
        rectTransform.gameObject.SetActive(true);
        mousePos = WorldCreatorCursor.GetMousePos();
        var pos = Camera.main.WorldToScreenPoint(mousePos);
        pos *= UIScalerScript.GetScale();
        pos += new Vector3(300, 0);
        rectTransform.anchoredPosition = pos;

        type.value = (int)sector.type;
        sectorName.text = sector.sectorName;
        sectorMusicBool.isOn = sector.hasMusic;
        sectorMusicID.text = sector.musicID;
        particles.value = (int)sector.rectangleEffectSkin;
        tiles.value = (int)sector.backgroundTileSkin;

        sectorName.transform.parent.gameObject.SetActive(true);
        waveSet.transform.parent.gameObject.SetActive(true);
        x.transform.parent.gameObject.SetActive(true);
        y.transform.parent.gameObject.SetActive(true);
        w.transform.parent.gameObject.SetActive(true);
        h.transform.parent.gameObject.SetActive(true);
        colorR.transform.parent.gameObject.SetActive(true);
        colorG.transform.parent.gameObject.SetActive(true);
        colorB.transform.parent.gameObject.SetActive(true);
        bgSpawnScrollContents.gameObject.SetActive(true);
        insertBGSpawnsButton.SetActive(true);
        clearBGSpawnsButton.SetActive(true);
        parseBGSpawnsButton.SetActive(true);
        deleteButton.SetActive(true);
        for (int i = 0; i < shardCounts.Count; i++)
        {
            shardCounts[i].gameObject.SetActive(true);
        }


        waveSet.text = sector.waveSetPath;

        x.text = currentSector.bounds.x.ToString();
        y.text = currentSector.bounds.y.ToString();
        w.text = currentSector.bounds.w.ToString();
        h.text = currentSector.bounds.h.ToString();
        colorR.text = currentSector.backgroundColor.r.ToString();
        colorG.text = currentSector.backgroundColor.g.ToString();
        colorB.text = currentSector.backgroundColor.b.ToString();
        for (int i = 0; i < shardCounts.Count; i++)
        {
            shardCounts[i].text = currentSector.shardCountSet[i].ToString();
        }

        opening = false;
        UpdateBGSpawns();
    }

    void Update()
    {
        if (!editingDefaults)
        {
            var pos = Camera.main.WorldToScreenPoint(mousePos);
            pos *= UIScalerScript.GetScale();
            pos += new Vector3(300, 0);
            rectTransform.anchoredPosition = pos;

            x.text = currentSector.bounds.x.ToString();
            y.text = currentSector.bounds.y.ToString();
            w.text = currentSector.bounds.w.ToString();
            h.text = currentSector.bounds.h.ToString();
        }
    }

    public void UpdateType()
    {
        if (opening)
        {
            return;
        }

        if (editingDefaults)
        {
            sectorMusicID.text =
                PlayerPrefs.GetString($"WCSectorPropertyDisplay_defaultMusic{type.value}",
                    WCGeneratorHandler.GetDefaultMusic((Sector.SectorType)type.value));

            colorR.text =
                PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultR{type.value}", SectorColors.colors[type.value].r).ToString();
            colorB.text =
                PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultB{type.value}", SectorColors.colors[type.value].b).ToString();
            colorG.text =
                PlayerPrefs.GetFloat($"WCSectorPropertyDisplay_defaultG{type.value}", SectorColors.colors[type.value].g).ToString();
            return;
        }

        currentSector.type = (Sector.SectorType)type.value;
        currentSector.backgroundColor = WorldCreatorCursor.GetDefaultColor((Sector.SectorType)type.value);
        colorR.text = currentSector.backgroundColor.r.ToString();
        colorG.text = currentSector.backgroundColor.g.ToString();
        colorB.text = currentSector.backgroundColor.b.ToString();
    }

    public void UpdateName()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.sectorName = sectorName.text;
    }

    public void UpdateMusic()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.musicID = sectorMusicID.text;
    }

    public void UpdateMusicBool()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.hasMusic = sectorMusicBool.isOn;
    }

    public void UpdateColor()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.backgroundColor = new Color(float.Parse(colorR.text), float.Parse(colorG.text), float.Parse(colorB.text), 1);
    }

    public void UpdateParticles()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.rectangleEffectSkin = (RectangleEffectSkin)particles.value;
    }

    public void UpdateTiles()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.backgroundTileSkin = (BackgroundTileSkin)tiles.value;
    }

    public void UpdateShardCounts()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        for (int i = 0; i < shardCounts.Count; i++)
        {
            currentSector.shardCountSet[i] = int.Parse(shardCounts[i].text);
        }
    }

    public void Hide()
    {
        rectTransform.gameObject.SetActive(false);
        if (editingDefaults)
        {
            PlayerPrefs.SetInt("WCSectorPropertyDisplay_defaultMusicOn", sectorMusicBool.isOn ? 1 : 0);
            if (sectorMusicID.text != "")
            {
                PlayerPrefs.SetString($"WCSectorPropertyDisplay_defaultMusic{type.value}", sectorMusicID.text);
            }

            PlayerPrefs.SetInt("WCSectorPropertyDisplay_defaultParticles", particles.value);
            PlayerPrefs.SetInt("WCSectorPropertyDisplay_defaultTiles", tiles.value);
            if (colorR.text != "")
            {
                PlayerPrefs.SetFloat($"WCSectorPropertyDisplay_defaultR{type.value}", float.Parse(colorR.text));
            }

            if (colorG.text != "")
            {
                PlayerPrefs.SetFloat($"WCSectorPropertyDisplay_defaultG{type.value}", float.Parse(colorG.text));
            }

            if (colorB.text != "")
            {
                PlayerPrefs.SetFloat($"WCSectorPropertyDisplay_defaultB{type.value}", float.Parse(colorB.text));
            }
        }

        editingDefaults = false;
    }

    public void AddBGSpawn()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        AddBGSpawn(null, 1);
    }

    public void AddBGSpawn(string text = null, int faction = 1)
    {
        if (opening || editingDefaults)
        {
            return;
        }

        var field = Instantiate(bgSpawnInputFieldPrefab, bgSpawnScrollContents).GetComponentInChildren<InputField>();
        var drop = field.transform.parent.GetComponentInChildren<Dropdown>();
        ItemPropertyDisplay.AddCustomFactionsToDropdown(drop);
        bgSpawnInputFields.Add((field, drop));
        field.text = text;
        drop.value = faction;
        Canvas.ForceUpdateCanvases();
        bgSpawnScrollContents.GetComponent<RectTransform>().sizeDelta = new Vector2(100, bgSpawnScrollContents.GetComponent<VerticalLayoutGroup>().minHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainContents);
    }

    public void ClearBGSpawns()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        foreach (var field in bgSpawnInputFields)
        {
            Destroy(field.Item1.transform.parent.gameObject);
        }

        bgSpawnInputFields.Clear();
    }

    public void TryParseBGSpawns()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        List<Sector.LevelEntity> levelEntities = new List<Sector.LevelEntity>();
        foreach (var field in bgSpawnInputFields)
        {
            if (string.IsNullOrEmpty(field.Item1.text))
            {
                continue;
            }

            var item = ItemHandler.instance.items.Find((it) => { return it.assetID == field.Item1.text; });

            if (item != null)
            {
                Sector.LevelEntity ent = new Sector.LevelEntity();

                // you can choose to give any object a custom name
                if (!string.IsNullOrEmpty(item.name))
                {
                    ent.name = item.name;
                }
                else
                {
                    ent.name = item.obj.name;
                }

                if (factionIDs != null)
                {
                    ent.faction = factionIDs[field.Item2.value]; // maybe change this later
                }
                ent.assetID = item.assetID;
                levelEntities.Add(ent);
            }
            else
            {
                // TODO: Reused code from WCSiegeWaveHandler, fix
                Sector.LevelEntity ent = new Sector.LevelEntity();
                ent.assetID = "shellcore_blueprint";
                ent.faction = field.Item2.value; // maybe change this later
                try
                {
                    EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
                    JsonUtility.FromJsonOverwrite(field.Item1.text, blueprint);
                    blueprint.intendedType = EntityBlueprint.IntendedType.ShellCore; // for good measure :)

                    ent.name = blueprint.entityName;
                    ent.blueprintJSON = JsonUtility.ToJson(blueprint);
                }
                catch (System.Exception e)
                {
                    // try and see if the name is an indirect reference
                    var path = System.IO.Path.Combine(Application.streamingAssetsPath, "EntityPlaceholder");
                    if (System.IO.Directory.GetFiles(path).Contains<string>(System.IO.Path.Combine(path, field.Item1.text + ".json")))
                    {
                        ent.name = "ShellCore";
                        ent.blueprintJSON = field.Item1.text;
                    }
                    else
                    {
                        Debug.LogWarning(e);
                        continue;
                    }
                }

                levelEntities.Add(ent);
            }
        }

        currentSector.backgroundSpawns = new Sector.BackgroundSpawn[levelEntities.Count];
        var i = 0;
        foreach (var ent in levelEntities)
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
        if (opening || editingDefaults)
        {
            return;
        }

        factionIDs = new List<int>();
        ItemPropertyDisplay.UpdateFactionIDList(factionIDs);
        ClearBGSpawns();
        foreach (var bgSpawn in currentSector.backgroundSpawns)
        {
            if (bgSpawn.entity.assetID != "shellcore_blueprint"
                && ItemHandler.instance.items.Exists((item) => { return item.assetID == bgSpawn.entity.assetID; }))
            {
                AddBGSpawn(bgSpawn.entity.assetID, bgSpawn.entity.faction);
            }
            else
            {
                AddBGSpawn(bgSpawn.entity.blueprintJSON, bgSpawn.entity.faction);
            }
        }
    }

    public void UpdateWaveSet()
    {
        if (opening || editingDefaults)
        {
            return;
        }

        currentSector.waveSetPath = waveSet.text;
    }
}
