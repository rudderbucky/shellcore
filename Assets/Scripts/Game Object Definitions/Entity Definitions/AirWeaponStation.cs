using System.Collections.Generic;

public class AirWeaponStation : AirConstruct, IOwner
{
    int intrinsicCommandLimit = 0;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();
    public int GetIntrinsicCommandLimit()
    {
        return intrinsicCommandLimit;
    }

    public SectorManager GetSectorManager()
    {
        return sectorMngr;
    }

    public int GetTotalCommandLimit()
    {
        if (sectorMngr)
        {
            return intrinsicCommandLimit + sectorMngr.GetExtraCommandUnits(faction);
        }
        else
        {
            return intrinsicCommandLimit;
        }
    }

    public Draggable GetTractorTarget()
    {
        return null;
    }

    public List<IOwnable> GetUnitsCommanding()
    {
        return unitsCommanding;
    }

    public void SetIntrinsicCommandLimit(int val)
    {
        intrinsicCommandLimit = val;
    }

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
