public class CoreUpgrader : AirConstruct
{
    protected override void Start()
    {
        Category = EntityCategory.Station;
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        TickAbilitiesAsStation();
    }
}
