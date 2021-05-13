using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCarrier : GroundConstruct, ICarrier {
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
        coreAlertThreshold = maxHealth[1] * 0.75f;
        shellAlertThreshold = maxHealth[0];
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
        else return intrinsicCommandLimit;
    }

    public SectorManager GetSectorManager() {
        return sectorMngr;
    }
    protected override void Update()
    {
        if (initialized)
        {
            var enemyTargetFound = false;
            if(BattleZoneManager.getTargets() != null && BattleZoneManager.getTargets().Length > 0)
            {
                foreach(var target in BattleZoneManager.getTargets())
                {
                    if(!FactionManager.IsAllied(target.faction, faction) && !target.GetIsDead())
                    {
                        enemyTargetFound = true;
                        break;
                    }
                }
            } 

            foreach (ActiveAbility active in GetComponentsInChildren<ActiveAbility>())
            {
                if(!(active is SpawnDrone) || enemyTargetFound) 
                {
                    active.Tick(1);
                }
            }


            base.Update();
        }
    }

    public Draggable GetTractorTarget() {
        return null;
    }
    public override void TakeCoreDamage(float amount){
        base.TakeCoreDamage(amount);
        if (currentHealth[1] < coreAlertThreshold && FactionManager.IsAllied(0, faction)) {
            if(currentHealth[1] < 1){
                coreAlertThreshold = -99999;
                PlayerCore.Instance.alerter.showMessage("Base has been destroyed!", "clip_alert");
            }
            else if(currentHealth[1] < (maxHealth[1] / 2)){
                coreAlertThreshold -= (maxHealth[1] * 0.2f);
                PlayerCore.Instance.alerter.showMessage("Base is about to be destroyed!", "clip_alert");
            }
            else{
                coreAlertThreshold -= (maxHealth[1] * 0.2f);
                PlayerCore.Instance.alerter.showMessage("Base Core is under attack!", "clip_alert");
            }
        }
    }
    public override float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy){
        //this is bad code but idk how to do better
        float residue = base.TakeShellDamage(amount,shellPiercingFactor,lastDamagedBy);
        if(currentHealth[0] < shellAlertThreshold && FactionManager.IsAllied(0, faction)){
            if(currentHealth[0] < 1){
                shellAlertThreshold = -99999;
                PlayerCore.Instance.alerter.showMessage("Base shell has been destroyed!", "clip_alert");
            }
            else if (currentHealth[0] < maxHealth[0] / 2){
                shellAlertThreshold -= (maxHealth[0] * 0.2f);
                PlayerCore.Instance.alerter.showMessage("Base shell is taking heavy damage!", "clip_alert");
            }
            else{
                shellAlertThreshold -= (maxHealth[0] * 0.2f);
                PlayerCore.Instance.alerter.showMessage("Base is under attack!", "clip_alert");
            }
        }
        return residue;
    }
}
