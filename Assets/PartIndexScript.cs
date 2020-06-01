using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartIndexScript : MonoBehaviour
{
    public static PartIndexScript instance;

    public Dictionary<EntityBlueprint.PartInfo, GameObject> parts = new Dictionary<EntityBlueprint.PartInfo, GameObject>();
    public GameObject inventoryPrefab;

    public Transform[] contents;
    public GameObject[] texts;
    public GameObject infoBox;

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
            if(PlayerCore.Instance.cursave.partsObtained.Exists(x => CullToPartIndexValues(x).Equals(part))) 
            {
                button.status = PartStatus.Obtained;
                button.displayShiny = PlayerCore.Instance.cursave.partsObtained.Find(x => CullToPartIndexValues(x).Equals(part)).shiny;
            }
            else if(PlayerCore.Instance.cursave.partsSeen.Exists(x => CullToPartIndexValues(x).Equals(part))) button.status = PartStatus.Seen;
            else button.status = PartStatus.Unseen;
            button.infoBox = infoBox;
            button.Initialize();
            Debug.Log(PlayerCore.Instance.cursave.partsObtained.Count);
        }
        parts[part].GetComponent<PartIndexInventoryButton>().origins.Add(sectorName);
    }

    void OnEnable()
    {
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
                AttemptAddToPartsSeen(part);
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
        JsonUtility.FromJsonOverwrite(entity.blueprintJSON, blueprint);
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
