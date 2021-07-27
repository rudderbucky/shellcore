using UnityEngine;

public class Rocket : Torpedo
{
    protected override void Awake()
    {
        base.Awake(); // base awake
        ID = AbilityID.Rocket;
        terrain = Entity.TerrainType.Air;
        category = Entity.EntityCategory.Station;
        survivalTime = 2.5F;
        range = bulletSpeed * survivalTime;
        bonusDamageType = typeof(AirConstruct);
    }
}
