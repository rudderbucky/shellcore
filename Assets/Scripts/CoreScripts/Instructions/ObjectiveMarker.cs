using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    public static void AddObjectiveMarker(string entityID, string sectorName, string missionName, string ID)
    {
        Vector2 pos = Vector2.zero;
        int dim = 0;
        Entity entity = null;
        foreach (var ent in AIData.entities)
        {
            if (!ent)
            {
                continue;
            }

            if (entityID != ent.ID)
            {
                continue;
            }

            entity = ent;
            pos = entity.transform.position;
            dim = SectorManager.instance.current.dimension;
        }

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
            missionName,
            dim,
            entity
        );

        CoreScriptsManager.instance.objectiveLocations.Add(ID, objectiveLocation);
        TaskManager.DrawObjectiveLocations();
    }
    public static void RemoveObjectiveMarker(string ID)
    {
        if (!CoreScriptsManager.instance.objectiveLocations.ContainsKey(ID)) return;
        CoreScriptsManager.instance.objectiveLocations.Remove(ID);
        TaskManager.DrawObjectiveLocations();
    }
}
