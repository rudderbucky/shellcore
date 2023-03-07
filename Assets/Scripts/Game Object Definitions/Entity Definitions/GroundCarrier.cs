using System.Collections.Generic;
using UnityEngine;

public class GroundCarrier : GroundConstruct, ICarrier
{
    private float coreAlertThreshold;
    private float shellAlertThreshold;

    int intrinsicCommandLimit = 0;
    public List<IOwnable> unitsCommanding = new List<IOwnable>();

    public bool GetIsInitialized()
    {
        return initialized;
    }

    public Vector3 GetSpawnPoint()
    {
        var tmp = transform.position;
        tmp.y -= 3;
        return tmp;
    }

    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
        initialized = true;
        coreAlertThreshold = maxHealth[1] * 0.8f;
        shellAlertThreshold = maxHealth[0] * 0.8f;
    }

    public List<IOwnable> GetUnitsCommanding()
    {
        return unitsCommanding;
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

    public SectorManager GetSectorManager()
    {
        return sectorMngr;
    }

    protected override void Update()
    {
        if (initialized)
        {
            TickAbilitiesAsStation();
            base.Update();
            TargetManager.Enqueue(targeter);

            if (!SectorManager.instance.carriers.ContainsKey(faction)
                || (SectorManager.instance.carriers[faction] == null || SectorManager.instance.carriers[faction].Equals(null))
                || SectorManager.instance.carriers[faction].GetIsDead())
            {
                if (!GetIsDead())
                {
                    SectorManager.instance.carriers[faction] = this;
                }
            }
        }
    }

    public Draggable GetTractorTarget()
    {
        return null;
    }

    public int GetIntrinsicCommandLimit()
    {
        return intrinsicCommandLimit;
    }

    public void SetIntrinsicCommandLimit(int val)
    {
        intrinsicCommandLimit = val;
    }

    public override void TakeCoreDamage(float amount)
    {
        base.TakeCoreDamage(amount);
        if (PlayerCore.Instance && PlayerCore.Instance.alerter && currentHealth[1] < coreAlertThreshold && FactionManager.IsAllied(0, faction))
        {
            int temp = (int)(Mathf.Floor((currentHealth[1] / maxHealth[1]) * 5) + 1) * 20;
            coreAlertThreshold -= (maxHealth[1] * 0.2f);
            PlayerCore.Instance.alerter.showMessage($"Carrier is at {temp}% core", "clip_alert");
        }
    }

    public override float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy)
    {
        //this is bad code but idk how to do better
        float residue = base.TakeShellDamage(amount, shellPiercingFactor, lastDamagedBy);
        if (PlayerCore.Instance && PlayerCore.Instance.alerter && currentHealth[0] < shellAlertThreshold && FactionManager.IsAllied(0, faction))
        {
            int temp = (int)(Mathf.Floor((currentHealth[0] / maxHealth[0]) * 5) + 1) * 20;
            shellAlertThreshold -= (maxHealth[0] * 0.2f);
            PlayerCore.Instance.alerter.showMessage($"Carrier is at {temp}% shell", "clip_alert");
        }

        return residue;
    }
}
