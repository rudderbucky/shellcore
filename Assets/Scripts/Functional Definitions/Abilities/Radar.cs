using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using static SectorManager;
using static TaskManager;

public class Radar : Ability
{
    private static int radarChain;
    public static int GetRadarChain()
    {
        return radarChain;
    }
    private static string nextSector;
    private static SectorLoadDelegate currAction;
    public static ObjectiveLocation location;
    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        abilityName = "Radar";
        description = "Marks unknown sectors on the map, or a random sector if all have been found.";
        ID = AbilityID.Radar;
        cooldownDuration = 10;
        energyCost = 50;
    }
    private GameObject radarPrefab;
    public override void ActivationCosmetic(Vector3 targetPos)
    {
        AudioManager.PlayClipByID("clip_activateability", transform.position);
        if (!radarPrefab)
        {
            radarPrefab = ResourceManager.GetAsset<GameObject>("radar_effect");
        }

        Instantiate(radarPrefab, transform);
        base.ActivationCosmetic(targetPos);
    }


    /// <summary>
    /// Returns the engine power to the original value
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();
    }

    /// <summary>
    /// Increases core engine power to speed up the core
    /// </summary>
    protected override void Execute()
    {
        ActivationCosmetic(transform.position);
        var ls = SectorManager.instance.sectors.Where(s => s.dimension == 0 && s != SectorManager.instance.current).ToList();
        int randInt = Random.Range(0, ls.Count);
        nextSector = ls[randInt].sectorName;
        AddObjectiveMarker(nextSector);
        if (currAction != null)
        {
            radarChain = 0;
            SectorManager.OnSectorLoad -= currAction;
        }
        currAction = (s) => { SectorCheck(s); };
        SectorManager.OnSectorLoad += currAction;
        base.Execute();
    }

    private void SectorCheck(string sectorName)
    {
        if (sectorName != nextSector)
        {
            return;
        }

        RemoveObjectiveMarker();
        radarChain += 1;
        SectorManager.OnSectorLoad -= currAction;
        currAction = null;
    }



    public static void AddObjectiveMarker(string sectorName)
    {
        Vector2 pos = Vector2.zero;
        int dim = 0;
        
        var sect = SectorManager.GetSectorByName(sectorName);
        if (sect)
        {
            var bounds = sect.bounds;
            pos = new Vector2(bounds.x + bounds.w / 2, bounds.y - bounds.h / 2);
            dim = sect.dimension;
        }


        var objectiveLocation = new TaskManager.ObjectiveLocation
        (
            pos,
            null,
            dim,
            null
        );
        objectiveLocation.color = Color.white;

        location = objectiveLocation;
        TaskManager.DrawObjectiveLocations();
    }
    public static void RemoveObjectiveMarker()
    {
        location = null;
        TaskManager.DrawObjectiveLocations();
    }
}
