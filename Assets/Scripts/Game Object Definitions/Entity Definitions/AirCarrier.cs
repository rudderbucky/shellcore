using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICarrier
{
    Vector3 GetSpawnPoint();
    bool GetIsInitialized();
    int GetFaction();
}

public class AirCarrier : AirConstruct, ICarrier {
    private bool initialized;

    public bool GetIsInitialized()
    {
        return initialized;
    }
    public int GetFaction()
    {
        return faction;
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
}
