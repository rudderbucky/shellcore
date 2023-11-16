using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FusionStationScript : GUIWindowScripts
{
    [SerializeField]
    private FusionStationSelectionScript part1;
    [SerializeField]
    private FusionStationSelectionScript part2;
    [SerializeField]
    private FusionStationSelectionScript finalPart;
    [SerializeField]
    private Transform[] contentsArray; // holds scroll view sub-sections by part size
    [SerializeField]
    private GameObject[] contentTexts;
    [SerializeField]
    private FusionStationInventoryScript buttonPrefab;
    private Dictionary<EntityBlueprint.PartInfo, FusionStationInventoryScript> buttons = new Dictionary<EntityBlueprint.PartInfo, FusionStationInventoryScript>();
    [SerializeField]
    private PartDisplayBase partDisplayBase;

    void OnEnable()
    {
        foreach (var a in contentsArray)
        {
            for (int i = 0; i < a.childCount; i++)
            {
                Destroy(a.GetChild(i).gameObject);
            }
        }
        buttons.Clear();

        foreach (var part in PlayerCore.Instance.GetInventory())
        {
            if (buttons.ContainsKey(part)) 
            {
                buttons[part].IncrementCount();
                continue;
            }
            int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
            FusionStationInventoryScript dictInvButton = Instantiate(buttonPrefab,
            contentsArray[size]).GetComponent<FusionStationInventoryScript>();
            dictInvButton.IncrementCount();
            dictInvButton.partDisplayBase = partDisplayBase;
            dictInvButton.part = part;
            dictInvButton.fusionStationScript = this;
            buttons.Add(part, dictInvButton);
        }

        partDisplayBase.SetInactive();
        for (int i = 0; i < contentTexts.Length; i++)
        {
            contentTexts[i].SetActive(contentsArray[i].childCount > 0);
        }
    }

    public void SetSelectedPart(EntityBlueprint.PartInfo info)
    {
        if (string.IsNullOrEmpty(part1.part.partID))
        {
            part1.part = info;
            part1.Restart();
        }
        else if (string.IsNullOrEmpty(part2.part.partID))
        {
            part2.part = info;
            part2.Restart();
        }
    }

    public void Fuse()
    {
        var pi = new EntityBlueprint.PartInfo();
        pi.partID = part1.part.partID;
        pi.abilityID = part2.part.abilityID;
        pi.tier = part2.part.tier;
        pi.secondaryData = part2.part.secondaryData;
        finalPart.part = pi;
        finalPart.Restart();
    }
}
