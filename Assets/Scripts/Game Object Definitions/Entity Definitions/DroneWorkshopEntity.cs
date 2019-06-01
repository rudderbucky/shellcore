using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneWorkshopEntity : AirConstruct
{
    protected override void Start() {
        category = EntityCategory.Station;
        base.Start();
    }
}
