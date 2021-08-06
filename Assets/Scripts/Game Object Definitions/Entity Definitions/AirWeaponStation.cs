public class AirWeaponStation : AirConstruct
{
    // Use this for initialization
    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        TickAbilitiesAsStation();
    }
}
