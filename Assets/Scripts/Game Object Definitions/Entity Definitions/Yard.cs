using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShipBuilder {
    BuilderMode GetBuilderMode();
    List<EntityBlueprint.PartInfo> GetInventory();
}

public enum BuilderMode {
    Yard,
    Trader
}
public class Yard : AirConstruct, IShipBuilder {

    public BuilderMode mode;
    public List<EntityBlueprint.PartInfo> inventory;
    public List<EntityBlueprint.PartInfo> GetInventory() {
        return inventory;
    }
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
