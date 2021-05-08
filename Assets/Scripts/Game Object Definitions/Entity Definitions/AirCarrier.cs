using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICarrier : IOwner
{
    Vector3 GetSpawnPoint();
    bool GetIsInitialized();
}

public class AirCarrier : AirConstruct, ICarrier {
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
        coreAlertThreshold = maxHealth[1];
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
                if(!(active is SpawnDrone) || enemyTargetFound) active.Tick(1);
            }


            base.Update();
            TargetManager.Enqueue(targeter);
        }
    }

    public Draggable GetTractorTarget() {
        return null;
    }
    public override void TakeCoreDamage(float amount){
        base.TakeCoreDamage(amount);
        if (currentHealth[1] < coreAlertThreshold && FactionManager.IsAllied(0, faction)) {
            if(currentHealth[1] < 1){
                shellAlertThreshold = -99999;
                PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Carrier has been destroyed! You're on your last life now!</color>");
            }
            else if(currentHealth[1] < (maxHealth[1] / 2)){
                shellAlertThreshold = 1;
                PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Carrier is about to die! Kill the attacking enemies NOW!</color>");
            }
            else{
                shellAlertThreshold = (maxHealth[1] / 2);
                PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Carrier is taking Core damage! We won't last much longer!</color>");
            }
        }
    }
    public override float TakeShellDamage(float amount, float shellPiercingFactor, Entity lastDamagedBy){
        //this is bad code but idk how to do better
        float residue = base.TakeShellDamage(amount,shellPiercingFactor,lastDamagedBy);
        if(currentHealth[0] < shellAlertThreshold && FactionManager.IsAllied(0, faction)){
            if(currentHealth[0] < 1){
                shellAlertThreshold = -99999;
                PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Carrier shell is down! We're taking Core damage now!</color>");
            }
            else if(currentHealth[0] < (maxHealth[0] / 2)){
                shellAlertThreshold = 1;
                PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Carrier shell is heavily damaged! Take out those enemies before they bring down the Carrier!</color>");
            }
            else{
                shellAlertThreshold = (maxHealth[0] / 2);
                PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Carrier is taking damage! Defend your Carrier or you won't have anywhere to respawn!</color>");
            }
        }
        return residue;
    }
}
