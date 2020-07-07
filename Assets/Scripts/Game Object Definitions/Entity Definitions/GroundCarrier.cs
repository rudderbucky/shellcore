using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCarrier : GroundConstruct, ICarrier {

    int intrinsicCommandLimit = 0;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    public SectorManager GetSectorManager() {
        return sectorMngr;
    }
    public bool GetIsInitialized()
    {
        return initialized;
    }

    public Vector3 GetSpawnPoint()
    {
        var tmp = transform.position;
        tmp.y -= 3;
        return tmp; 
    }
    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
        initialized = true;
    }

    public List<IOwnable> GetUnitsCommanding()
    {
        return unitsCommanding;
    }

    public int GetTotalCommandLimit()
    {
        if (sectorMngr)
        {
            return intrinsicCommandLimit + sectorMngr.GetExtraCommandUnits(faction);
        }
        else return intrinsicCommandLimit;
    }

    protected override void Update()
    {
        if (initialized)
        {
            foreach (ActiveAbility active in GetComponentsInChildren<ActiveAbility>())
            {
                active.Tick(1);
            }
            base.Update();
        }
    }

    public Draggable GetTractorTarget() {
        return null;
    }
}
