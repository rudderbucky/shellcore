using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartIndexScript : MonoBehaviour
{
    public static PartIndexScript instance;
    public WorldData.PartIndexData[] index;

    public Dictionary<EntityBlueprint.PartInfo, GameObject> parts = new Dictionary<EntityBlueprint.PartInfo, GameObject>();
    public GameObject inventoryPrefab;

    public Transform[] contents;
    public GameObject[] texts;
    public GameObject infoBox;
    public PartDisplayBase partDisplay;

    // Tallies how many parts the player has interacted with and how.
    // Stats numbers -
    // 0: number of shiny parts
    // 1: number of obtained parts
    // 2: number of seen parts
    // 3: number of total parts
    public Image[] statsBar;
    public int[] statsNumbers;
    public Text statsTotalTally;
    public enum PartStatus
    {
        Unseen,
        Seen,
        Obtained
    }

    ///
    /// Attempt to add a part into the index, check if the player obtained/saw it
    ///
    public void AttemptAddPart(EntityBlueprint.PartInfo part, string sectorName)
    {
        part = CullToPartIndexValues(part);
        if(!parts.ContainsKey(part))
        {
            var button = Instantiate(inventoryPrefab, contents[ResourceManager.GetAsset<PartBlueprint>(part.partID).size]).GetComponent<PartIndexInventoryButton>();
            parts.Add(part, button.gameObject);
            button.part = part;
            if(CheckPartObtained(part)) 
            {
                button.status = PartStatus.Obtained;
                button.displayShiny = PlayerCore.Instance.cursave.partsObtained.Find(x => CullToPartIndexValues(x).Equals(part)).shiny;

                // Update stats on 3 tallies, possibly the shiny tally
                if(button.displayShiny) statsNumbers[0]++;
                statsNumbers[1]++;
                statsNumbers[2]++;
            }
            else if(CheckPartSeen(part)) 
            {
                button.status = PartStatus.Seen;
                button.displayShiny = false;
                // Update only the seen tally
                statsNumbers[2]++;
            }
            else 
            {
                button.status = PartStatus.Unseen;
                button.displayShiny = false;
            }
            button.infoBox = infoBox;
            button.partDisplay = partDisplay;
            // Update total number
            statsNumbers[3]++;
        }
        parts[part].GetComponent<PartIndexInventoryButton>().origins.Add(sectorName);
    }

    public static bool CheckPartObtained(EntityBlueprint.PartInfo part)
    {
        part = CullToPartIndexValues(part);
        return PlayerCore.Instance.cursave.partsObtained.Exists(x => CullToPartIndexValues(x).Equals(part));
    }

    public static bool CheckPartSeen(EntityBlueprint.PartInfo part)
    {
        part = CullToPartIndexValues(part);
        return PlayerCore.Instance.cursave.partsSeen.Exists(x => CullToPartIndexValues(x).Equals(part));
    }

    void OnEnable()
    {
        statsNumbers = new int[] {0, 0, 0, 0};
        foreach(var content in contents)
        {
            for(int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
        parts.Clear();

        // player metadata
        if(PlayerCore.Instance.cursave.partsObtained == null || PlayerCore.Instance.cursave.partsObtained.Count == 0)
        {
            PlayerCore.Instance.cursave.partsObtained = new List<EntityBlueprint.PartInfo>();
            foreach(var part in PlayerCore.Instance.GetInventory())
            {
                AttemptAddToPartsObtained(part);
            }

            foreach(var part in PlayerCore.Instance.blueprint.parts)
            {
                AttemptAddToPartsObtained(part);
            }
        }
        
        if(PlayerCore.Instance.cursave.partsSeen == null || PlayerCore.Instance.cursave.partsSeen.Count == 0)
        {
            PlayerCore.Instance.cursave.partsSeen = new List<EntityBlueprint.PartInfo>();
            var partsSeen = PlayerCore.Instance.cursave.partsSeen;
            foreach(var part in PlayerCore.Instance.cursave.partsObtained)
            {
                AttemptAddToPartsSeen(CullToPartIndexValues(part));
            }
        }

        // index assembly

        foreach(var sector in SectorManager.instance.sectors)
        {
            foreach(var entity in sector.entities)
            {
                AttemptAddShellCoreParts(entity, sector.sectorName);
            }

            foreach(var backgroundSpawn in sector.backgroundSpawns)
            {
                AttemptAddShellCoreParts(backgroundSpawn.entity, sector.sectorName);
            }
        }

        for(int i = 0; i < contents.Length; i++)
        {
            texts[i].SetActive(contents[i].childCount > 0);
        }

        // Update tally graphic bar
        for(int i = 0; i < statsBar.Length; i++)
        {
            statsBar[i].rectTransform.sizeDelta = new Vector2(statsNumbers[i] * 800 / statsNumbers[3], 20);
        }

        // Just found out about string interpolation. Damn that stuff rocks.
        statsTotalTally.text = $"{statsNumbers[3]}";
    }

    public static void AttemptAddToPartsObtained(EntityBlueprint.PartInfo part)
    {
        var partsObtained = PlayerCore.Instance.cursave.partsObtained;
        if(!partsObtained.Exists(x => CullToPartIndexValues(x).Equals( CullToPartIndexValues(part) ) ))
        {
            partsObtained.Add(part);
        }
        else if(part.shiny)
        {
            partsObtained[partsObtained.FindIndex(x => CullToPartIndexValues(x).Equals(CullToPartIndexValues(part)))] = part;
        }
    }

    public static void AttemptAddToPartsSeen(EntityBlueprint.PartInfo part)
    {
        var partsSeen = PlayerCore.Instance.cursave.partsSeen;
        if(!partsSeen.Exists(x => CullToPartIndexValues(x).Equals( CullToPartIndexValues(part) ) ))
        {
            partsSeen.Add(part);
        }
    }

    ///
    /// Attempt to add all the ShellCore's parts into the index
    ///
    public void AttemptAddShellCoreParts(Sector.LevelEntity entity, string sectorName)
    {
        EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();

        // try parsing directly, if that fails try fetching the entity file
        try
        {
            JsonUtility.FromJsonOverwrite(entity.blueprintJSON, blueprint);
        }
        catch
        {
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText
                (SectorManager.instance.resourcePath + "\\Entities\\" + entity.blueprintJSON + ".json"), blueprint);
        }

        
        if(blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore && entity.faction == 1)
        {
            if(blueprint.parts != null)
            {
                foreach(var part in blueprint.parts)
                {
                    AttemptAddPart(part, sectorName);
                }
            }       
        }
    }

    public static EntityBlueprint.PartInfo CullToPartIndexValues(EntityBlueprint.PartInfo partToCull)
    {
        var part = new EntityBlueprint.PartInfo();
		part.partID = partToCull.partID;
		part.abilityID = partToCull.abilityID;
		if(part.abilityID != 10) part.secondaryData = null;
		part.tier = partToCull.tier;
		part.shiny = false;
		return part;
    }
}
