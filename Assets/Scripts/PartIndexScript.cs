using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartIndexScript : MonoBehaviour
{
    // The Part Index assumes this field is already set up by the Sector Manager upon reading world data.
    public static WorldData.PartIndexData[] index;

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
    public static PartIndexScript instance;

    public enum PartStatus
    {
        Unseen,
        Seen,
        Obtained
    }

    public static int GetNumberOfPartsObtained(bool percentEnabled)
    {
        int count = 0;
        int total = 0;
        foreach (var part in index)
        {
            var part2 = CullToPartIndexValues(part.part);
            if (CheckPartObtained(part2))
            {
                count++;
            }
            total++;
        }

        if (percentEnabled)
            return (int)((count / (double)total) * 100);
        else
            return count;
    }

    public static int GetNumberOfPartsSeen(bool percentEnabled)
    {
        int count = 0;
        int total = 0;
        foreach (var part in index)
        {
            var part2 = CullToPartIndexValues(part.part);
            if (CheckPartSeen(part2))
            {
                count++;
            }
            total++;
        }

        if (percentEnabled)
            return (int)((count / (double)total) * 100);
        else
            return count;
    }

    public void SetPartAsSeen(EntityBlueprint.PartInfo part)
    {
        if (!parts.ContainsKey(part)) return;
        var button = parts[part].GetComponent<PartIndexInventoryButton>();
        if (!button) return;
        if (button.status == PartStatus.Seen || button.status == PartStatus.Obtained) return;
        button.status = PartStatus.Seen;
        button.displayShiny = false;
        // Update only the seen tally
        statsNumbers[2]++;
        button.UpdateButton();
    }

    private void SetPartAsObtained(EntityBlueprint.PartInfo part)
    {
        if (!parts.ContainsKey(part)) return;
        var button = parts[part].GetComponent<PartIndexInventoryButton>();
        if (!button) return;
        if (button.status == PartStatus.Obtained) return;
        statsNumbers[1]++;
        statsNumbers[2]++;

        button.status = PartStatus.Obtained;
        button.displayShiny = PlayerCore.Instance.cursave.partsObtained.Find(x => CullToPartIndexValues(x).Equals(part)).shiny;

        // Update stats on 3 tallies, possibly the shiny tally
        if (button.displayShiny)
        {
            statsNumbers[0]++;
        }

        button.UpdateButton();
    }




    ///
    /// Attempt to add a part into the index, check if the player obtained/saw it
    ///
    public void AttemptAddPart(EntityBlueprint.PartInfo part, List<string> origins)
    {
        part = CullToPartIndexValues(part);
        if (!parts.ContainsKey(part))
        {
            var button = Instantiate(inventoryPrefab, contents[ResourceManager.GetAsset<PartBlueprint>(part.partID).size]).GetComponent<PartIndexInventoryButton>();
            parts.Add(part, button.gameObject);
            button.part = part;
            if (CheckPartObtained(part))
            {
                SetPartAsObtained(part);
            }
            else if (CheckPartSeen(part))
            {
                SetPartAsSeen(part);
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
            foreach (var origin in origins)
            {
                parts[part].GetComponent<PartIndexInventoryButton>().origins.Add(origin);
            }
        }
    }

    public static bool partsObtainedCheat;

    public static bool CheckPartObtained(EntityBlueprint.PartInfo part)
    {
        if (partsObtainedCheat)
        {
            return true;
        }

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
        instance = this;
        ResetContent();
        UpdateContent(null, null);

        var partsSeen = new List<EntityBlueprint.PartInfo>();
        foreach (var part in PlayerCore.Instance.cursave.partsSeen)
        {
            if (!partsSeen.Contains(CullToPartIndexValues(part)))
            {
                partsSeen.Add(CullToPartIndexValues(part));
            }
        }
        PlayerCore.Instance.cursave.partsSeen = partsSeen;

        Entity.OnEntityDeath += UpdateContent;
    }

    private void OnDisable()
    {
        Entity.OnEntityDeath -= UpdateContent;
        ResetContent();
    }

    private void ResetContent()
    {
        statsNumbers = new int[] { 0, 0, 0, 0 };
        foreach (var content in contents)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        parts.Clear();
    }



    public void UpdateContent(Entity _, Entity __)
    {
        if (index == null)
        {
            Debug.LogWarning("The Part Index cache has not been set up for this world. Please rewrite this world, it will rebuild automatically.");
            return;
        }

        // player metadata
        if (PlayerCore.Instance.cursave.partsObtained == null || PlayerCore.Instance.cursave.partsObtained.Count == 0)
        {
            PlayerCore.Instance.cursave.partsObtained = new List<EntityBlueprint.PartInfo>();
            foreach (var part in PlayerCore.Instance.GetInventory())
            {
                AttemptAddToPartsObtained(part);
            }

            foreach (var part in PlayerCore.Instance.blueprint.parts)
            {
                AttemptAddToPartsObtained(part);
            }
        }

        if (PlayerCore.Instance.cursave.partsSeen == null || PlayerCore.Instance.cursave.partsSeen.Count == 0)
        {
            PlayerCore.Instance.cursave.partsSeen = new List<EntityBlueprint.PartInfo>();
            var partsSeen = PlayerCore.Instance.cursave.partsSeen;
            foreach (var part in PlayerCore.Instance.cursave.partsObtained)
            {
                AttemptAddToPartsSeen(CullToPartIndexValues(part));
            }
        }

        if (attemptAddPartCoroutine != null)
        {
            StopCoroutine(attemptAddPartCoroutine);
            attemptAddPartCoroutine = null;
        }
        GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
        attemptAddPartCoroutine = AttemptAddPartHelper();
        StartCoroutine(attemptAddPartCoroutine);
    }

    private IEnumerator attemptAddPartCoroutine;

    private IEnumerator AttemptAddPartHelper()
    {
        int x = 0;
        // index assembly
        foreach (var partData in index)
        {
            AttemptAddPart(partData.part, partData.origins);
            x++;
            if (x >= 10)
            {
                x = 0;
                UpdateTextAndTally();
                yield return new WaitForEndOfFrame();
            }
        }

        UpdateTextAndTally();
        yield return null;
    }
    
    private void UpdateTextAndTally()
    {
        for (int i = 0; i < contents.Length; i++)
        {
            texts[i].SetActive(contents[i].childCount > 0);
        }

        UpdateTallyGraphic();
    }

    private void UpdateTallyGraphic()
    {
        // Update tally graphic bar
        if (statsNumbers[3] > 0)
        {
            for (int i = 0; i < statsBar.Length; i++)
            {
                statsBar[i].rectTransform.sizeDelta = new Vector2(statsNumbers[i] * 800 / statsNumbers[3], 20);
            }
        }
        else
        {
            // no parts in world, just hide the bars
            for (int i = 0; i < statsBar.Length; i++)
            {
                statsBar[i].gameObject.SetActive(false);
            }
        }

        // Just found out about string interpolation. Damn that stuff rocks.
        statsTotalTally.text = $"{statsNumbers[3]}";
    }

    public static void AttemptAddToPartsObtained(EntityBlueprint.PartInfo part)
    {
        var partsObtained = PlayerCore.Instance.cursave.partsObtained;
        if (!partsObtained.Exists(x => CullToPartIndexValues(x).Equals(CullToPartIndexValues(part))))
        {
            partsObtained.Add(CullToPartIndexValues(part));
            if (instance) instance.SetPartAsObtained(CullToPartIndexValues(part));
        }
        else if (part.shiny)
        {
            partsObtained[partsObtained.FindIndex(x => CullToPartIndexValues(x).Equals(CullToPartIndexValues(part)))] = part;
            if (instance) instance.SetPartAsObtained(CullToPartIndexValues(part));
        }
    }

    public static void AttemptAddToPartsSeen(EntityBlueprint.PartInfo part)
    {
        var partsSeen = PlayerCore.Instance.cursave.partsSeen;
        if (!partsSeen.Exists(x => CullToPartIndexValues(x).Equals(CullToPartIndexValues(part))))
        {
            partsSeen.Add(CullToPartIndexValues(part));
            if (instance) instance.SetPartAsSeen(CullToPartIndexValues(part));
        }
    }

    ///
    /// Culls unnecessary data in the passed PartInfo for adding into the Part Index.
    /// Notably, it nullifies secondary data if the part does not spawn drones.
    ///
    public static EntityBlueprint.PartInfo CullToPartIndexValues(EntityBlueprint.PartInfo partToCull)
    {
        var part = new EntityBlueprint.PartInfo();
        part.partID = partToCull.partID;
        part.abilityID = partToCull.abilityID;
        if (!ShipBuilder.CheckSecondaryDataPurge(partToCull))
        {
            part.secondaryData = partToCull.secondaryData;
            var data = DroneUtilities.GetDroneSpawnDataByShorthand(part.secondaryData);
            part.secondaryData = DroneUtilities.GetDefaultSecondaryDataByType(data.type);
        }

        part.tier = partToCull.tier;
        part.shiny = false;
        return part;
    }
}
