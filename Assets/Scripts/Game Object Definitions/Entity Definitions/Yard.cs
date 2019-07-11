using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShipBuilder {
    BuilderMode GetBuilderMode();
}

public enum BuilderMode {
    Yard,
    Trader,
    Workshop
}

public class Yard : AirConstruct, IShipBuilder {

    public BuilderMode mode;
    public BuilderMode GetBuilderMode() {
        return mode;
    }

    protected override void Start() {
        category = EntityCategory.Station;
        base.Start();
    }

    protected override void Update()
    {
        targeter.GetTarget(true);
        if(!isDead)
            foreach(WeaponAbility weapon in GetComponentsInChildren<WeaponAbility>()) {
                weapon.Tick(null);
            }
        base.Update();
    }
}
