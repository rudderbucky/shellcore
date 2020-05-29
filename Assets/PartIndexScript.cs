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

    public enum PartStatus
    {
        Unseen,
        Seen,
        Obtained
    }

    public void AttemptAddPart(EntityBlueprint.PartInfo part)
    {
        part = CullToPartIndexValues(part);

        if(!parts.ContainsKey(part))
        {
            var button = Instantiate(inventoryPrefab, contents[ResourceManager.GetAsset<PartBlueprint>(part.partID).size]).GetComponent<PartIndexInventoryButton>();
            parts.Add(part, button.gameObject);
            button.part = part;
            if(PlayerCore.Instance.cursave.partsObtained.Exists(x => CullToPartIndexValues(x).Equals(part))) button.status = PartStatus.Obtained;
            else if(PlayerCore.Instance.cursave.partsSeen.Exists(x => CullToPartIndexValues(x).Equals(part))) button.status = PartStatus.Seen;
            else button.status = PartStatus.Unseen;
            button.Initialize();
            Debug.Log(PlayerCore.Instance.cursave.partsObtained.Count);
        }
    }

    void OnEnable()
    {
        if(PlayerCore.Instance.cursave.partsObtained == null || PlayerCore.Instance.cursave.partsObtained.Count == 0)
        {
            PlayerCore.Instance.cursave.partsObtained = new List<EntityBlueprint.PartInfo>();
            var partsObtained = PlayerCore.Instance.cursave.partsObtained;
            foreach(var part in PlayerCore.Instance.GetInventory())
            {
                if(!partsObtained.Contains(CullToPartIndexValues(part)))
                    partsObtained.Add(CullToPartIndexValues(part));
            }

            foreach(var part in PlayerCore.Instance.blueprint.parts)
            {
                if(!partsObtained.Contains(CullToPartIndexValues(part)))
                    partsObtained.Add(CullToPartIndexValues(part));
            }
        }

        if(PlayerCore.Instance.cursave.partsSeen == null || PlayerCore.Instance.cursave.partsSeen.Count == 0)
        {
            PlayerCore.Instance.cursave.partsSeen = new List<EntityBlueprint.PartInfo>();
            var partsSeen = PlayerCore.Instance.cursave.partsSeen;
            foreach(var part in PlayerCore.Instance.cursave.partsObtained)
            {
                if(!partsSeen.Contains(CullToPartIndexValues(part)))
                    partsSeen.Add(CullToPartIndexValues(part));
            }
        }

        foreach(var sector in SectorManager.instance.sectors)
        {
            foreach(var entity in sector.entities)
            {
                AttemptAddShellCoreParts(entity);
            }

            foreach(var backgroundSpawn in sector.backgroundSpawns)
            {
                AttemptAddShellCoreParts(backgroundSpawn.entity);
            }
        }

        for(int i = 0; i < contents.Length; i++)
        {
            texts[i].SetActive(contents[i].childCount > 0);
        }
    }

    public void AttemptAddShellCoreParts(Sector.LevelEntity entity)
    {
        EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(entity.blueprintJSON, blueprint);
        if(blueprint.intendedType == EntityBlueprint.IntendedType.ShellCore && entity.faction == 1)
        {
            if(blueprint.parts != null)
            {
                foreach(var part in blueprint.parts)
                {
                    AttemptAddPart(part);
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
		part.shiny = partToCull.shiny;
		return part;
    }
}
