using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreUpgrader : AirConstruct {
    protected override void Start() {
        category = EntityCategory.Station;
        base.Start();
    }
}
