using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICarrier : IOwner
{
    Vector3 GetSpawnPoint();
    bool GetIsInitialized();
}

public class AirCarrier : AirConstruct, ICarrier {

    int intrinsicCommandLimit = 0;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();
    private bool initialized;

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

    public SectorManager GetSectorManager() {
        return sectorMngr;
    }
    protected override void Update()
    {
        if (initialized)
        {
            foreach (ActiveAbility active in GetComponentsInChildren<ActiveAbility>())
            {
                active.Tick("activate");
            }
            base.Update();
        }
    }

    public Draggable GetTractorTarget() {
        return null;
    }
}
