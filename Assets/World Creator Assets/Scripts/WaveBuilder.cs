using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WaveBuilder : GUIWindowScripts
{
    public static WaveBuilder instance;
    [SerializeField] private Transform waveContents;
    [SerializeField] private Transform waveSpawnContents;
    [SerializeField] private GameObject buttonPrefab;
    
    [SerializeField] private InputField jsonInputField;
    [SerializeField] private InputField timeInputField;
    [SerializeField] private InputField flagNameInputField;
    [SerializeField] private Dropdown factionDropdown;

    [SerializeField] private Button selectedSpawnButton;
    [SerializeField] private Button selectedWaveButton;
    private Color32 selectedColor = new Color32(100, 150, 100, 255);
    private Color32 unselectedColor = new Color32(100, 100, 100, 255);

    [SerializeField] private WaveSet waveSet;
    [SerializeField] private SiegeWave currentSiegeWave;
    private SiegeEntity currentSiegeEntity;

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
        selectedSpawnButton = null;
    }

    /*public void ClearSelectedSpawns()
    {
        AudioManager.PlayClipByID("clip_unload");
        ClearWaveSpawns();
    }*/

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

    private SiegeEntity GetCurrentSiegeEntity()
    {
        return currentSiegeEntity;
    }

    public void ResetSelectedWave()
    {
        if (selectedWaveButton != null)
        {
            selectedWaveButton.GetComponent<Image>().color = unselectedColor;
            selectedWaveButton = null;
        }
    }

    public void DeselectWave()
    {
        ClearWaveSpawns();
        ResetSelectedWaveSpawn();
        ResetSelectedWave();
    }

    public void RemoveSelectedWave()
    {
        if (currentSiegeWave == null || selectedWaveButton == null)
            return;

        var wv = new List<SiegeWave>(waveSet.waves);
        wv.Remove(currentSiegeWave);
        waveSet.waves = wv.ToArray();
        Destroy(selectedWaveButton.gameObject);
        ClearWaveSpawns();
        currentSiegeWave = null;
    }

    // Button for creating/editing spawns
    public void AddOrModifyWaveSpawn()
    {
        var entity = TryParseFields((jsonInputField, timeInputField, flagNameInputField, factionDropdown));
        if (entity == null) 
            return;
        var text = GetDescriptor(entity);
        if (currentSiegeEntity != null)
        {
            currentSiegeEntity.entity = entity.entity;
            currentSiegeEntity.flagName = entity.flagName;
            currentSiegeEntity.timeSinceWaveStartToSpawn = entity.timeSinceWaveStartToSpawn;
            if (selectedSpawnButton)
                selectedSpawnButton.GetComponentInChildren<Text>().text = text;
            ResetSelectedWaveSpawn();
        }
        else if (currentSiegeWave != null) // selectedSpawnButton = null
        {
            if (currentSiegeWave.entities == null) 
                currentSiegeWave.entities = new List<SiegeEntity>();
            currentSiegeWave.entities.Add(entity);
            AddSpawnToTable(entity);
        }
        else 
            Debug.LogWarning(3);
    }

    public void SetModifyWaveSpawnFields(SiegeEntity siegeEntity)
    {
        if (currentSiegeEntity == siegeEntity)
        {
            ResetSelectedWaveSpawn();
            return;
        }
        jsonInputField.text = (siegeEntity.entity.assetID == "shellcore_blueprint" ? siegeEntity.entity.blueprintJSON : siegeEntity.entity.assetID);
        timeInputField.text = siegeEntity.timeSinceWaveStartToSpawn + "";
        flagNameInputField.text = siegeEntity.flagName;
        factionDropdown.value = siegeEntity.entity.faction;
        currentSiegeEntity = siegeEntity;
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
                if (currentSiegeEntity == spawn)
                    currentSiegeEntity = null;
                Destroy(button.gameObject);
                return;
            }

            var doNotSetModify = spawn == GetCurrentSiegeEntity();
            ResetSelectedWaveSpawn();
            if (doNotSetModify)
                return;

            selectedSpawnButton = button;
            selectedSpawnButton.GetComponent<Image>().color = selectedColor;
            SetModifyWaveSpawnFields(spawn);
        });
    }

    public void SetWaveSpawnContents(SiegeWave wave)
    {
        if (wave == null)
            return;
        ClearWaveSpawns();
        if (wave.entities == null)
            wave.entities = new List<SiegeEntity>();
        foreach (var spawn in wave.entities)
        {
            AddSpawnToTable(spawn);
        }

        currentSiegeWave = wave;
    }

    public void ResetSelectedWaveSpawn()
    {
        currentSiegeEntity = null;
        if (selectedSpawnButton != null)
        {
            selectedSpawnButton.GetComponent<Image>().color = unselectedColor;
            selectedSpawnButton = null;
        }
    }

    public void RemoveSelectedWaveSpawn()
    {
        if (currentSiegeEntity == null || currentSiegeWave == null || selectedSpawnButton == null)
            return;

        currentSiegeWave.entities.Remove(currentSiegeEntity);
        Destroy(selectedSpawnButton.gameObject);
        selectedSpawnButton = null;
    }

    private string GetDescriptor(SiegeEntity entity)
    {
        return $"{entity.timeSinceWaveStartToSpawn}/{entity.flagName + ""}/{(entity.entity.assetID == "shellcore_blueprint" ? entity.entity.blueprintJSON : entity.entity.assetID)}/{entity.entity.faction}";
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

    // Saves everything
    public void ParseWaves(string path)
    {
        System.IO.File.WriteAllText(path, JsonUtility.ToJson(waveSet));
    }

    // Button for creating waves
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

                ResetSelectedWave();

                selectedWaveButton = button;
                selectedWaveButton.GetComponent<Image>().color = selectedColor;
                SetWaveSpawnContents(wave);
            });
        }
    }
}
