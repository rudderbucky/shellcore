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
        GetComponentsInChildren<SubcanvasSortingOrder>(true).ToList().ForEach(x => x.Initialize());
    }

    public void AddEntity(string name, float time, string flagName, int faction)
    {
        var gObj = Instantiate(waveEntityPrefab, content);
        var inField1 = gObj.GetComponentsInChildren<InputField>()[0];
        var inField2 = gObj.GetComponentsInChildren<InputField>()[1];
        var inField3 = gObj.GetComponentsInChildren<InputField>()[2];
        var dropdown = gObj.GetComponentInChildren<Dropdown>();
        ItemPropertyDisplay.AddCustomFactionsToDropdown(dropdown);

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
        }

        return wave;
    }
}
