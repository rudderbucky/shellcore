using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SkirmishOption
{
    public int creditLimit;
    public string mapDescription;
    public string entityID;
    public string sectorName;
    public bool clearParty;
}

// This class is used to control the window used to select and play Skirmish maps. Maps are defined by the class above
// Theoretically entityID and sectorName serve only as warp points making this a glorified warp interface
public class SkirmishMenu : GUIWindowScripts
{
    public List<SkirmishOption> options;

    [SerializeField]
    private Transform listContents;

    [SerializeField]
    private GameObject optionButton;

    [SerializeField]
    private Text description;

    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Text creditLimitText;

    private SkirmishOption currentOption;

    public static SkirmishMenu instance;

    public void Start()
    {
        exitOnPlayerRange = true;
        instance = this;
    }

    public override void Activate()
    {
        foreach (Transform child in listContents)
        {
            Destroy(child.gameObject);
        }

        currentOption = null;
        nameText.text = description.text = creditLimitText.text = "";

        foreach (var option in options)
        {
            var curOpt = option;
            var button = Instantiate(optionButton, listContents).GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = curOpt.sectorName;
            button.onClick.AddListener(() =>
            {
                currentOption = option;
                LoadMap();
                description.text = currentOption.mapDescription;
                nameText.text = currentOption.sectorName;
                if (currentOption.creditLimit < PlayerCore.Instance.GetBuildValue())
                {
                    creditLimitText.text = $"<color=red>SHIP CREDIT LIMIT: {currentOption.creditLimit} (CURRENTLY INELIGIBLE)</color>";
                }
                else
                {
                    creditLimitText.text = $"SHIP CREDIT LIMIT: {currentOption.creditLimit}";
                }
            });
        }

        base.Activate();
    }

    public void LoadMap()
    {
        var sector = SectorManager.GetSectorByName(currentOption.sectorName);
        if (sector == null)
        {
            return;
        }

        List<Sector> sectors = new List<Sector>() { sector };
        GetComponentInChildren<MapMakerScript>().redraw(sectors, 1, sector.dimension, true);
    }

    public void ActivateCurrentOption()
    {
        //Debug.LogError(PlayerCore.Instance.currentHealth[0]);
        if (currentOption == null || currentOption.creditLimit < PlayerCore.Instance.GetBuildValue())
        {
            return;
        }

        if (currentOption.clearParty)
            PartyManager.instance.ClearParty(false);
        Flag.FindEntityAndWarpPlayer(currentOption.sectorName, currentOption.entityID);
        //Debug.LogError(PlayerCore.Instance.currentHealth[0]);
        CloseUI();
    }
}
