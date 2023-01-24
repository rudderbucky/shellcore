using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WCSiegeWaveHandler : MonoBehaviour
{
    public GameObject waveEntityPrefab;
    public RectTransform content;
    private List<(InputField, InputField, InputField, Dropdown)> waveEntities = new List<(InputField, InputField, InputField, Dropdown)>();
    public Button exit;

    public void AddEntity()
    {
        AddEntity("", 0, "", 1);
    }

    void OnDisable()
    {
        //Debug.LogWarning("Closes Wave Builder."); //Fix?
    }

    public void Initialize(List<SiegeEntity> entities)
    {
        foreach (var ent in entities)
        {
            if (ent.entity.assetID == "shellcore_blueprint")
            {
                AddEntity(ent.entity.blueprintJSON, ent.timeSinceWaveStartToSpawn, ent.flagName, ent.entity.faction);
            }
            else
            {
                AddEntity(ent.entity.assetID, ent.timeSinceWaveStartToSpawn, ent.flagName, ent.entity.faction);
            }
        }
    }

    public void AddEntity(string name, float time, string flagName, int faction)
    {
        var gObj = Instantiate(waveEntityPrefab, content);
        var inField1 = gObj.GetComponentsInChildren<InputField>()[0];
        var inField2 = gObj.GetComponentsInChildren<InputField>()[1];
        var inField3 = gObj.GetComponentsInChildren<InputField>()[2];
        var dropdown = gObj.GetComponentInChildren<Dropdown>();

        inField1.text = name;
        inField2.text = time + "";
        inField3.text = flagName;
        dropdown.value = faction;

        var button = gObj.GetComponentInChildren<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(
            () =>
            {
                waveEntities.Remove((inField1, inField2, inField3, dropdown));
                Destroy(inField1.transform.parent.gameObject);
                Destroy(inField2.transform.parent.gameObject);
                Destroy(inField3.transform.parent.gameObject);
                Destroy(dropdown.transform.parent.gameObject);
            }
        ));

        //ItemPropertyDisplay.AddCustomFactionsToDropdown(dropdown); //Causes factions to reset back to 0
        waveEntities.Add((inField1, inField2, inField3, dropdown));
        Canvas.ForceUpdateCanvases();
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(100, content.GetComponent<VerticalLayoutGroup>().minHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public SiegeWave Parse()
    {
        SiegeWave wave = new SiegeWave();
        wave.entities = new List<SiegeEntity>();
        foreach (var item in waveEntities)
        {
            wave.entities.Add(TryParseFields(item));
        }

        return wave;
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
                if (System.IO.Directory.GetFiles(path).Contains<string>(System.IO.Path.Combine(path , field.Item1.text + ".json")))
                {
                    ent.name = "ShellCore";
                    ent.blueprintJSON = field.Item1.text;
                }
                else
                {
                    Debug.LogWarning(e);
                    return null;
                }
            }
        }

        siegeEntity.entity = ent;
        float.TryParse(field.Item2.text, out siegeEntity.timeSinceWaveStartToSpawn);
        siegeEntity.flagName = field.Item3.text;

        return siegeEntity;
    }
}
