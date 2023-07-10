using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WaveBuilder : GUIWindowScripts
{
    [SerializeField]
    private Transform waveContents;

    [SerializeField]
    private Transform waveSpawnContents;

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private WaveSet waveSet;
    [SerializeField]
    private SiegeWave currentSiegeWave;
    private SiegeEntity currentSiegeEntity;
    [SerializeField]
    private InputField jsonInputField;
    [SerializeField]
    private InputField timeInputField;
    [SerializeField]
    private InputField flagNameInputField;
    [SerializeField]
    private Dropdown factionDropdown;
    [SerializeField]
    private Button existingButton;
    private Color32 selectedColor = new Color32(100,150,100,255);
    private Color32 unselectedColor = new Color32(100,100,100,255);
    public static WaveBuilder instance;
    
    

    void OnEnable()
    {
        ClearWaves();
        waveSet = new WaveSet();
        instance = this;
        UpdateFactions();
    }

    public void UpdateFactions()
    {
        ItemPropertyDisplay.AddCustomFactionsToDropdown(factionDropdown);
    }

    void OnDisable()
    {
        ClearWaves();
    }

    void ClearWaves()
    {
        for (int i = 0; i < waveContents.childCount; i++)
        {
            Destroy(waveContents.GetChild(i).gameObject);
        }

        currentSiegeWave = null;

        ClearWaveSpawns();
    }

    void ClearWaveSpawns()
    {

        for (int i = 0; i < waveSpawnContents.childCount; i++)
        {
            Destroy(waveSpawnContents.GetChild(i).gameObject);
        }
        currentSiegeEntity = null;
        existingButton = null;
    }

    public void AddWave()
    {
        var waves = new List<SiegeWave>();
        if (waveSet.waves != null)
            waves = new List<SiegeWave>(waveSet.waves);
        var x = new SiegeWave();
        waves.Add(x);
        waveSet.waves = waves.ToArray();
        ReadWaves(waveSet);
        currentSiegeWave = x;
    }

    private void AddSpawnToTable(SiegeEntity spawn)
    {
        var button = Instantiate(buttonPrefab, waveSpawnContents.transform).GetComponent<Button>();
        var text = GetDescriptor(spawn);
        button.GetComponentInChildren<Text>().text = text;
        button.onClick.AddListener(() => 
        {
            if (Input.GetKey(KeyCode.LeftShift) && currentSiegeWave != null)
            {
                currentSiegeWave.entities.Remove(spawn);
                if (currentSiegeEntity == spawn) currentSiegeEntity = null;
                Destroy(button.gameObject);
                return;
            }
            var doNotSetModify = spawn == GetCurrentSiegeEntity();
            ResetSelectedWaveSpawn();
            if (doNotSetModify) return;
            existingButton = button;
            existingButton.GetComponent<Image>().color = selectedColor;
            SetModifyWaveSpawnFields(spawn);
        });
    }

    public void SetWaveSpawnContents(SiegeWave wave)
    {
        if (wave == null) return;
        ClearWaveSpawns();
        if (wave.entities == null) wave.entities = new List<SiegeEntity>();
        foreach (var spawn in wave.entities)
        {
            AddSpawnToTable(spawn);
        }
        
        currentSiegeWave = wave;
    }

    private SiegeEntity GetCurrentSiegeEntity()
    {
        return currentSiegeEntity;
    }

    private void ResetSelectedWaveSpawn()
    {
        currentSiegeEntity = null;
        if (existingButton) 
        {
            existingButton.GetComponent<Image>().color = unselectedColor;
        }
        existingButton = null;
    }

    public void SetModifyWaveSpawnFields(SiegeEntity siegeEntity)
    {
        if (currentSiegeEntity == siegeEntity)
        {
            ResetSelectedWaveSpawn();
            return;
        }
        jsonInputField.text = (siegeEntity.entity.assetID == "shellcore_blueprint" ? siegeEntity.entity.blueprintJSON : siegeEntity.entity.assetID);
        timeInputField.text = siegeEntity.timeSinceWaveStartToSpawn+"";
        flagNameInputField.text = siegeEntity.flagName;
        factionDropdown.value = siegeEntity.entity.faction;
        currentSiegeEntity = siegeEntity;
    }

    private string GetDescriptor(SiegeEntity entity)
    {
        return $"{entity.timeSinceWaveStartToSpawn}/{entity.flagName+""}/{(entity.entity.assetID == "shellcore_blueprint" ? entity.entity.blueprintJSON : entity.entity.assetID) }/{entity.entity.faction}";
    }

    public void AddOrModifyWaveSpawn()
    {
        var entity = TryParseFields((jsonInputField, timeInputField, flagNameInputField, factionDropdown));
        if (entity == null) return;
        var text = GetDescriptor(entity);
        if (currentSiegeEntity != null)
        {
            currentSiegeEntity.entity = entity.entity;
            currentSiegeEntity.flagName = entity.flagName;
            currentSiegeEntity.timeSinceWaveStartToSpawn = entity.timeSinceWaveStartToSpawn;
            if (existingButton) 
                existingButton.GetComponentInChildren<Text>().text = text;
            ResetSelectedWaveSpawn();
        }
        else if (currentSiegeWave != null) // existingButton = null
        {
            if (currentSiegeWave.entities == null) currentSiegeWave.entities = new List<SiegeEntity>();
            currentSiegeWave.entities.Add(entity);
            AddSpawnToTable(entity);
        }
        else 
            Debug.LogWarning(3);
    }

    private SiegeEntity TryParseFields((InputField, InputField, InputField, Dropdown) field)
    {
        if (string.IsNullOrEmpty(field.Item1.text))
        {
            return null;
        }

        SiegeEntity siegeEntity = new SiegeEntity();
        Sector.LevelEntity ent = new Sector.LevelEntity();
        var item = ItemHandler.instance.items.Find((it) => { return it.assetID == field.Item1.text; });

        if (item != null)
        {
            // you can choose to give any object a custom name
            if (!string.IsNullOrEmpty(item.name))
            {
                ent.name = item.name;
            }
            else
            {
                ent.name = item.obj.name;
            }

            ent.faction = field.Item4.value; // maybe change this later
            Debug.Log(ent.faction);
            ent.assetID = item.assetID;
        }
        else
        {
            ent.assetID = "shellcore_blueprint";
            ent.faction = field.Item4.value; // maybe change this later
            try
            {
                EntityBlueprint blueprint = SectorManager.TryGettingEntityBlueprint(field.Item1.text, false);
                if (!string.IsNullOrEmpty(blueprint.entityName) && blueprint.entityName != "Unnamed")
                {
                    ent.name = blueprint.entityName;
                }
                else
                {
                    ent.name = "ShellCore";
                }
                ent.blueprintJSON = field.Item1.text;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        siegeEntity.entity = ent;
        float.TryParse(field.Item2.text, out siegeEntity.timeSinceWaveStartToSpawn);
        siegeEntity.flagName = field.Item3.text;

        return siegeEntity;
    }


    public void ParseWaves(string path)
    {
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(waveSet));
    }

    public void ReadWaves(WaveSet waves)
    {
        ClearWaves();
        waveSet = waves;
        int i = 0;
        foreach (var wave in waves.waves)
        {
            i++;
            var button = Instantiate(buttonPrefab, waveContents.transform).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = $"Wave {i}";
            button.onClick.AddListener(() => 
            {
                if (Input.GetKey(KeyCode.LeftShift) && waveSet != null)
                {
                    var wv = new List<SiegeWave>(waveSet.waves);
                    wv.Remove(wave);
                    waves.waves = wv.ToArray();
                    Destroy(button.gameObject);
                    if (wave == currentSiegeWave)
                    {
                        ClearWaveSpawns();
                        currentSiegeWave = null;
                    }
                    return;
                }
                SetWaveSpawnContents(wave);
            });
        }
    }
}
