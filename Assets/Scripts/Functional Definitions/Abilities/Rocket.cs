using UnityEngine;

public class Rocket : Torpedo
{
    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Rocket;
        terrain = Entity.TerrainType.Air;
        category = Entity.EntityCategory.Station;
        bonusDamageType = typeof(AirConstruct);
    }
}
