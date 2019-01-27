using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShipBuilder {

}
public class Yard : AirConstruct, IShipBuilder {
    protected override void Start() {
        category = EntityCategory.Station;
        base.Start();
    }
}
