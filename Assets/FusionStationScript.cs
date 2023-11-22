using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

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
    [SerializeField]
    private Text fusePartsButtonText;

    void OnDisable()
    {
        ShardCountScript.StickySlideOut();
    }

    private void AddOrIncrement(EntityBlueprint.PartInfo part)
    {
        part = ShipBuilder.CullSpatialValues(part);
        if (buttons.ContainsKey(part)) 
        {
            buttons[part].IncrementCount();
            return;
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

    void OnEnable()
    {

        ShardCountScript.StickySlideIn();
        part1.part = new EntityBlueprint.PartInfo();
        part2.part = new EntityBlueprint.PartInfo();
        part1.partDisplayBase = partDisplayBase;
        part2.partDisplayBase = partDisplayBase;

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
            if (part.shiny) continue;
            AddOrIncrement(part);
        }

        partDisplayBase.SetInactive();
        for (int i = 0; i < contentTexts.Length; i++)
        {
            contentTexts[i].SetActive(contentsArray[i].childCount > 0);
        }
    }

    protected override void Update()
    {
        base.Update();
        if (!ReadyToFuse())
        {
            fusePartsButtonText.text = "Click 2 parts to fuse (Shift click clears part)".ToUpper();
        }
        else
        {
            fusePartsButtonText.text = $"Fuse parts ({GetFinalCost()} fusion energy)".ToUpper();
        }
    }

    public void SetSelectedPart(EntityBlueprint.PartInfo info)
    {
        if (string.IsNullOrEmpty(part1.part.partID))
        {
            part1.part = ShipBuilder.CullSpatialValues(info);
            part1.Restart();
        }
        else if (string.IsNullOrEmpty(part2.part.partID))
        {
            part2.part = ShipBuilder.CullSpatialValues(info);
            part2.Restart();
        }
    }

    private int GetSize(EntityBlueprint.PartInfo info)
    {
        return ResourceManager.GetAsset<PartBlueprint>(info.partID).size;
    }

    public void CreateFusionEnergy()
    {
        if (PlayerCore.Instance.cursave.gas < 100)
        {
            return;
        }
        if (PlayerCore.Instance.cursave.shards < 10)
        {
            return;
        }
        PlayerCore.Instance.cursave.gas -= 100;
        PlayerCore.Instance.cursave.shards -= 10;
        PlayerCore.Instance.cursave.fusionEnergy += 10;
        ShardCountScript.DisplayCount();
    }

    private int GetFinalCost()
    {
        var sizeSum = Mathf.Abs(GetSize(part1.part) - GetSize(part2.part));
        var finalCost = 10 + sizeSum * 10 + part2.part.tier * 5;
        return finalCost;
    }

    private bool ReadyToFuse()
    {
        return !string.IsNullOrEmpty(part1.part.partID) && !string.IsNullOrEmpty(part2.part.partID);
    }

    public void Fuse()
    {
        if (!ReadyToFuse())
        {
            return;
        }

        if (!buttons.ContainsKey(part1.part) || !buttons.ContainsKey(part2.part)
            || buttons[part1.part].GetCount() <= 0 || buttons[part2.part].GetCount() <= 0)
        {
            return;
        }

        var finalCost = GetFinalCost();
        if (PlayerCore.Instance.cursave.fusionEnergy < finalCost)
        {
            return;
        }


        
        PlayerCore.Instance.cursave.fusionEnergy -= finalCost;
        ShardCountScript.DisplayCount();

        var pi = new EntityBlueprint.PartInfo();
        pi.partID = part1.part.partID;
        pi.abilityID = part2.part.abilityID;
        pi.tier = part2.part.tier;
        pi.secondaryData = part2.part.secondaryData;
        pi = ShipBuilder.CullSpatialValues(pi);
        finalPart.part = pi;
        finalPart.Restart();

        buttons[part1.part].DecrementCount();
        if (buttons[part1.part].GetCount() <= 0)
        {
            part1.part = new EntityBlueprint.PartInfo();
            part1.Restart();
        }
        buttons[part2.part].DecrementCount();
        if (buttons[part2.part].GetCount() <= 0)
        {
            part2.part = new EntityBlueprint.PartInfo();
            part2.Restart();
        }
        AddOrIncrement(pi);
        PartIndexScript.AttemptAddToPartsObtained(pi);
        PartIndexScript.AttemptAddToPartsSeen(pi);

    }
}
