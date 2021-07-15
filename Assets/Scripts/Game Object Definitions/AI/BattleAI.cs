using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleAI : AIModule
{
    public enum BattleState
    {
        Attack,
        Defend,
        Collect,
        Fortify,
        ReinforceGround
    }

    class AITarget
    {
        public AITarget(Entity entity, float significance)
        {
            this.entity = entity;
            this.significance = significance;
        }

        public Entity entity;
        public float significance; // 1 for outpost, 2 for outpost with rock, 3 for base

        public float influence; // based on the amount and types of turrets around
        // public bool collecting; // if there's an energy rock, is it being collected by the owner
        // public bool underAttack; // is enemy attacking this target
    }

    BattleState state = BattleState.Attack;

    List<Entity> carriers;
    ShellCore shellcore;

    float nextSearchTime; //timer to reduce the frequency of heavy search functions
    float nextStateCheckTime; //timer to reduce the frequency of heavy search functions
    string mostNeeded = null; 

    Entity primaryTarget;
    Entity fortificationTarget;
    Turret attackTurret;

    EnergyRock collectTarget = null;
    bool findNewTarget = false;

    Draggable waitingDraggable;

    List<AITarget> AITargets = new List<AITarget>();
    Dictionary<EnergyRock, Turret> harvesterTurrets = new Dictionary<EnergyRock, Turret>();

    public BattleState GetState()
    {
        return state;
    }

    public void OrderModeChange(BattleState state)
    {
        this.state = state;
        nextStateCheckTime = 15;

        // TODO: prioritize damaged carriers over other carriers
        if (state == BattleState.Defend && carriers.Count > 0)
        {
            ai.aggression = AirCraftAI.AIAggression.KeepMoving; // Get to the home base asap
            primaryTarget = carriers[0];
        }
    }

    public override void Init()
    {
        carriers = new List<Entity>();
        harvesterTurrets = new Dictionary<EnergyRock, Turret>();

        Entity[] targetEntities = BattleZoneManager.getTargets();
        if (targetEntities == null)
        {
            Debug.LogError("Battle zone target list not initialized");
            ai.setMode(AirCraftAI.AIMode.Inactive);
            return;
        }

        for (int i = 0; i < targetEntities.Length; i++)
        {
            if (targetEntities[i] is ICarrier)
            {
                if (targetEntities[i].faction == craft.faction)
                {
                    carriers.Add(targetEntities[i]);
                }
            }
        }

        if (craft is ShellCore)
        {
            shellcore = craft as ShellCore;
        }
        else
        {
            Debug.LogError("Battle zone AI should only be used by shellcores!");
        }

        foreach (IOwnable ownable in shellcore.GetUnitsCommanding())
        {
            var turret = ownable as Turret;
            if (turret && turret.entityName == "Harvester Turret")
            {
                foreach (var rock in AIData.energyRocks)
                {
                    if (!harvesterTurrets.ContainsKey(rock) &&
                        Vector3.SqrMagnitude(rock.transform.position - turret.transform.position) <= 200)
                    {
                        harvesterTurrets.Add(rock, turret);
                        break;
                    }
                }
            }
        }

        nextSearchTime = Time.time;
        nextStateCheckTime = Time.time;

        for (int i = 0; i < AIData.vendors.Count; i++)
        {
            int rockCount = 0;
            for (int j = 0; j < AIData.energyRocks.Count; j++)
            {
                if ((AIData.energyRocks[j].transform.position - AIData.vendors[i].transform.position).sqrMagnitude < 100)
                {
                    rockCount++;
                }
            }

            AITargets.Add(new AITarget(AIData.vendors[i], rockCount + 1f));
        }

        for (int i = 0; i < carriers.Count; i++)
        {
            AITargets.Add(new AITarget(carriers[i], 100f));
        }
    }

    //TEMP:
    BattleState pState;

    public override void StateTick()
    {
        pState = state;
        List<EnergyRock> deadRocks = new List<EnergyRock>();
        foreach (var kvp in harvesterTurrets)
        {
            if (!kvp.Value || kvp.Value.GetIsDead())
            {
                deadRocks.Add(kvp.Key);
            }
        }

        foreach (var key in deadRocks)
        {
            harvesterTurrets.Remove(key);
        }

        if (nextStateCheckTime < Time.time) // Don't change state every tick
        {
            ai.aggression = AirCraftAI.AIAggression.FollowInRange; // usually, allow chasing after enemies

            for (int i = 0; i < carriers.Count; i++)
            {
                if (carriers[i] && carriers[i].GetHealth()[0] < carriers[i].GetMaxHealth()[0] * 0.3f)
                {
                    // if base under attack -> defend
                    if (AirCraftAI.getEnemyCountInRange(carriers[i].transform.position, 45f, craft.faction) > 0)
                    {
                        if ((carriers[i].transform.position - craft.transform.position).sqrMagnitude > 1500f)
                        {
                            ai.aggression = AirCraftAI.AIAggression.KeepMoving; // Get to the home base asap
                            primaryTarget = carriers[i];
                        }
                        else
                        {
                            primaryTarget = null;
                            // TODO: find the most dangerous unit in the area, set as primary target
                        }

                        state = BattleState.Defend;
                    }
                    else
                    {
                        state = BattleState.Collect;
                    }

                    break;
                }
            }

            var turretIsHarvester = shellcore.GetTractorTarget() &&
                                    shellcore.GetTractorTarget().GetComponent<Turret>()
                                    && shellcore.GetTractorTarget().GetComponent<Turret>().entityName == "Harvester Turret";
            // if population is nearly capped, attack
            if (shellcore.GetTotalCommandLimit() < shellcore.GetUnitsCommanding().Count + 1) // TODO: OR if enemy base is weak
            {
                state = BattleState.Attack;
            }
            else if (turretIsHarvester)
            {
                state = BattleState.Collect;
            }
            else if (shellcore.GetPower() >= 300)
            {
                bool enemyGround = false;
                for (int j = 0; j < AIData.entities.Count; j++)
                {
                    if (AIData.entities[j].faction != craft.faction && AIData.entities[j].Terrain == Entity.TerrainType.Ground)
                    {
                        enemyGround = true;
                        break;
                    }
                }
                if (enemyGround){
                    state = BattleState.ReinforceGround;
                    primaryTarget = null;
                }
                else {
                    state = BattleState.Fortify;
                    primaryTarget = null;
                }
                
            }
            // if there's no need for more population space, try to create turrets to protect owned outposts and stations
            else
            {
                if (state != BattleState.Collect)
                {
                    collectTarget = null;
                }

                if ((shellcore.GetTractorTarget() != null && shellcore.GetTractorTarget().GetComponent<Turret>() != null
                                                          && shellcore.GetHealth()[0] > shellcore.GetMaxHealth()[0] * 0.1f) || harvesterTurrets.Count >= Mathf.Min(5, AIData.energyRocks.Count))
                {
                    state = BattleState.Attack;
                }
                else
                {
                    state = BattleState.Collect;
                }
            }
            Debug.Log(state);

            nextStateCheckTime = Time.time + 1f;
        }

        if (pState != state)
        {
            //Debug.LogFormat("Faction {0} Shellcore changed state to: {1}", craft.faction, state);
        }
    }

    public override void ActionTick()
    {
        if (waitingDraggable != null && waitingDraggable)
        {
            ai.movement.SetMoveTarget(waitingDraggable.transform.position, 100f);

            Vector2 delta = waitingDraggable.transform.position - craft.transform.position;
            if (ai.movement.targetIsInRange() && shellcore.GetTractorTarget() != null && shellcore.GetTractorTarget().gameObject.GetComponent<EnergySphereScript>() == null)
            {
                shellcore.SetTractorTarget(waitingDraggable);
            }
        }

        var turretIsHarvester = shellcore.GetTractorTarget() &&
                                shellcore.GetTractorTarget().GetComponent<Turret>()
                                && shellcore.GetTractorTarget().GetComponent<Turret>().entityName == "Harvester Turret";

        switch (state)
        {
            case BattleState.Attack:
                if (shellcore.GetTractorTarget() == null)
                {
                    if (attackTurret == null)
                    {
                        Turret t = null;
                        float minDistance = float.MaxValue;
                        for (int i = 0; i < AIData.entities.Count; i++)
                        {
                            if (AIData.entities[i] != null && AIData.entities[i] &&
                                AIData.entities[i] is Turret &&
                                AIData.entities[i].faction == craft.faction &&
                                AIData.entities[i].GetComponentInChildren<WeaponAbility>() != null &&
                                AIData.entities[i].GetComponentInChildren<WeaponAbility>().GetID() != 16)
                            {
                                float d = (AIData.entities[i].transform.position - craft.transform.position).sqrMagnitude;
                                if (d < minDistance)
                                {
                                    t = AIData.entities[i] as Turret;
                                    minDistance = d;
                                }
                            }
                        }

                        attackTurret = t;
                    }
                    else
                    {
                        float d = (attackTurret.transform.position - shellcore.transform.position).sqrMagnitude;
                        if (d < 150)
                        {
                            shellcore.SetTractorTarget(attackTurret.GetComponent<Draggable>());
                        }
                    }
                }

                // go to nearest enemy construct, attack units / turrets if in visual range
                if ((primaryTarget == null && nextSearchTime < Time.time) || nextSearchTime < Time.time - 3f)
                {
                    // get nearest construct
                    primaryTarget = AirCraftAI.getNearestEntity<AirConstruct>(craft, true); //TODO: Exclude turrets?
                    nextSearchTime = Time.time + 1f;

                    //if(primaryTarget)
                    //    Debug.Log("AggroTarget: " + primaryTarget.name + " Factions: " + primaryTarget.faction + " - " + craft.faction);
                }

                if (primaryTarget != null)
                {
                    ai.movement.SetMoveTarget(primaryTarget.transform.position);
                    //craft.MoveCraft((primaryTarget.transform.position - craft.transform.position).normalized);
                }

                //TODO: AI Attack:
                // action sequences
                // Use existing turrets:
                // -Drag torpedo turrets to enemy bunkers
                // -Drag siege turrets to outposts
                // ground attack location = own bunker location
                // drag tanks from one platform to another "LandPlatformGenerator.getEnemiesOnPlatform(Vector2 platformLocation, int faction)"?
                // how to react to different pressures on land and air?
                break;
            case BattleState.Defend:
                // destroy enemy units around base, ignore everything outside siege range
                if (primaryTarget && !primaryTarget.GetIsDead())
                {
                    ai.movement.SetMoveTarget(primaryTarget.transform.position);
                }

                // buy a turret matching the biggest threat's element, if possible
                break;
            case BattleState.Collect:
                // go from outpost to outpost, (also less fortified enemy outposts [count enemy units nearby {TODO}]) and collect energy
                if (findNewTarget || collectTarget == null)
                {
                    // Find new target
                    float minD = float.MaxValue;
                    EnergyRock targetRock = null;
                    int maxEnergy = -1;

                    for (int i = 0; i < AIData.energyRocks.Count; i++)
                    {
                        if (AirCraftAI.getEnemyCountInRange(AIData.energyRocks[i].transform.position, 10f, craft.faction) > 2)
                        {
                            continue;
                        }

                        int energy = 0;
                        for (int j = 0; j < AIData.energySpheres.Count; j++)
                        {
                            if ((AIData.energySpheres[j].transform.position - AIData.energyRocks[i].transform.position).sqrMagnitude < 16)
                            {
                                energy++;
                            }
                        }

                        float d = (craft.transform.position - AIData.energyRocks[i].transform.position).sqrMagnitude;
                        if ((maxEnergy < energy || d * 1.5f < minD || (maxEnergy == energy && d < minD)) && AIData.energyRocks[i] != collectTarget)
                        {
                            minD = d;
                            maxEnergy = energy;
                            targetRock = AIData.energyRocks[i];
                        }
                    }

                    collectTarget = targetRock;

                    if (collectTarget != null)
                    {
                        findNewTarget = false;
                    }

                    //Debug.LogFormat("Faction {0} collect target: {1}", craft.faction, collectTarget);
                }

                if (collectTarget != null)
                {
                    ai.movement.SetMoveTarget(collectTarget.transform.position);
                    if (ai.movement.targetIsInRange())
                    {
                        if (harvesterTurrets.ContainsKey(collectTarget) &&
                            (!harvesterTurrets[collectTarget] || harvesterTurrets[collectTarget].GetIsDead()))
                        {
                            harvesterTurrets.Remove(collectTarget);
                        }

                        if (turretIsHarvester && !harvesterTurrets.ContainsKey(collectTarget))
                        {
                            harvesterTurrets.Add(collectTarget, shellcore.GetTractorTarget().GetComponent<Turret>());
                            shellcore.SetTractorTarget(null);
                        }

                        findNewTarget = true;
                    }
                }

                break;
            case BattleState.Fortify:
                // TODO: place turrets
                // set primary target to an outpost with least defending turrets
                if (fortificationTarget == null || !fortificationTarget)
                {
                    UpdateTargetInfluences();

                    float closestToZero = float.MaxValue;

                    for (int i = 0; i < AITargets.Count; i++)
                    {
                        if (Mathf.Abs(AITargets[i].influence) < closestToZero && AITargets[i].entity.faction == shellcore.faction)
                        {
                            fortificationTarget = AITargets[i].entity;
                        }
                    }
                }
                else if (attackTurret == null || !attackTurret)
                {
                    UpdateTargetInfluences();

                    float minDistance = float.MaxValue;

                    for (int i = 0; i < AIData.entities.Count; i++)
                    {
                        if (AIData.entities[i] is Turret)
                        {
                            float d = (craft.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                            float d2 = (fortificationTarget.transform.position - AIData.entities[i].transform.position).sqrMagnitude;
                            if (d < minDistance && d2 > 150f)
                            {
                                minDistance = d;
                                attackTurret = AIData.entities[i] as Turret;
                            }
                        }
                    }

                    if (attackTurret == null)
                    {
                        state = BattleState.Attack;
                        ActionTick();
                        nextStateCheckTime += 1f;
                        return;
                    }
                }
                else if (shellcore.GetTractorTarget() != attackTurret)
                {
                    ai.movement.SetMoveTarget(attackTurret.transform.position, 100f);
                    if (ai.movement.targetIsInRange())
                    {
                        var target = shellcore.GetTractorTarget();
                        if (target != null && target)
                        {
                            if (target.gameObject.GetComponent<EnergySphereScript>() == null)
                            {
                                shellcore.SetTractorTarget(attackTurret.GetComponent<Draggable>());
                            }
                        }
                    }
                }
                else
                {
                    Vector2 turretDelta = fortificationTarget.transform.position - attackTurret.transform.position;
                    Vector2 targetPosition = (Vector2)fortificationTarget.transform.position + turretDelta.normalized * 16f;
                    Vector2 delta = targetPosition - (Vector2)craft.transform.position;
                    if (turretDelta.sqrMagnitude < 16f)
                    {
                        shellcore.SetTractorTarget(null);
                    }
                }

                break;
            case BattleState.ReinforceGround:
            //head to nearest bunker to produce tanks
                float dist = float.MaxValue;
                int index = -1;
                for (int i = 0; i < AITargets.Count; i++)
                {
                    if (AITargets[i].entity.Terrain == Entity.TerrainType.Ground && AITargets[i].entity && AITargets[i].entity.faction == shellcore.faction && Vector2.SqrMagnitude(craft.transform.position - AITargets[i].entity.transform.position) < dist)
                    {
                        dist = Vector2.SqrMagnitude(craft.transform.position - AITargets[i].entity.transform.position);
                        index = i;
                    }
                }
                if (index != -1 && dist >= 100f){
                    ai.movement.SetMoveTarget(AITargets[index].entity.transform.position);
                    dist = Vector2.SqrMagnitude(craft.transform.position - AITargets[index].entity.transform.position);
                    Debug.Log("Moving to bunker");
                    Debug.Log(dist);
                }
            break;
            default:
                break;
        }

        // always drop harvester turrets on close energy rocks
        turretIsHarvester = shellcore.GetTractorTarget() &&
                            shellcore.GetTractorTarget().GetComponent<Turret>()
                            && shellcore.GetTractorTarget().GetComponent<Turret>().entityName == "Harvester Turret";
        if (turretIsHarvester)
        {
            var turret = shellcore.GetTractorTarget().GetComponent<Turret>();
            if (harvesterTurrets.ContainsValue(turret))
            {
                harvesterTurrets.Remove(harvesterTurrets.First(kvp => kvp.Value == turret).Key);
            }

            foreach (var rock in AIData.energyRocks)
            {
                if ((rock.transform.position - shellcore.transform.position).sqrMagnitude > 150f)
                {
                    continue;
                }

                if (harvesterTurrets.ContainsKey(rock) &&
                    (!harvesterTurrets[rock] || harvesterTurrets[rock].GetIsDead()))
                {
                    harvesterTurrets.Remove(rock);
                }

                if (!harvesterTurrets.ContainsKey(rock))
                {
                    harvesterTurrets.Add(rock, shellcore.GetTractorTarget().GetComponent<Turret>());
                    shellcore.SetTractorTarget(null);
                }
            }
        }

        int energyCount = 0;
        // always collect energy
        if (shellcore.GetTractorTarget() != null && shellcore.GetTractorTarget().gameObject.GetComponent<EnergySphereScript>() == null)
        {
            for (int i = 0; i < AIData.energySpheres.Count; i++)
            {
                if ((AIData.energySpheres[i].transform.position - shellcore.transform.position).sqrMagnitude < 150)
                {
                    energyCount++;
                    if (shellcore.GetTractorTarget() != null)
                    {
                        waitingDraggable = shellcore.GetTractorTarget();
                        shellcore.SetTractorTarget(null);
                    }
                }
            }
        }
        else if (shellcore.GetTractorTarget() == null && waitingDraggable != null)
        {
            for (int i = 0; i < AIData.energySpheres.Count; i++)
            {
                if ((AIData.energySpheres[i].transform.position - shellcore.transform.position).sqrMagnitude < 150)
                {
                    energyCount++;
                }
            }

            if (energyCount == 0)
            {
                shellcore.SetTractorTarget(waitingDraggable);
                if (shellcore.GetTractorTarget() == waitingDraggable)
                {
                    waitingDraggable = null;
                }
            }
        }

        // always buy more turrets/tanks
        if (shellcore.unitsCommanding.Count < shellcore.GetTotalCommandLimit() && energyCount == 0 || state == BattleState.ReinforceGround)
        {
            for (int i = 0; i < AIData.vendors.Count; i++)
            {
                if ((AIData.vendors[i].transform.position - craft.transform.position).sqrMagnitude <= 100f && AIData.vendors[i].faction == craft.faction)
                {
                    IVendor vendor = AIData.vendors[i] as IVendor;

                    if (vendor.GetVendingBlueprint() == null)
                    {
                        continue;
                    }

                    int itemIndex = -1;
                    int ownGroundStation = 0;
                    int ownTank = 0;
                    int enemyTank = 0;
                    int enemyGroundStation = 0;
                    for (int j = 0; j < AIData.entities.Count; j++)
                    {
                        if (FactionManager.IsAllied(AIData.entities[j].faction, craft.faction) && AIData.entities[j].Terrain == Entity.TerrainType.Ground && AIData.entities[j].category == Entity.EntityCategory.Station)
                        {
                            ownGroundStation += 1;
                        }
                        if (!FactionManager.IsAllied(AIData.entities[j].faction, craft.faction) && AIData.entities[j].Terrain == Entity.TerrainType.Ground && AIData.entities[j].category == Entity.EntityCategory.Station)
                        {
                            enemyGroundStation += 1;
                        }
                        if (FactionManager.IsAllied(AIData.entities[j].faction, craft.faction) && AIData.entities[j].Terrain == Entity.TerrainType.Ground && AIData.entities[j].category == Entity.EntityCategory.Unit)
                        {
                            ownTank += 1;
                        }
                        if (!FactionManager.IsAllied(AIData.entities[j].faction, craft.faction) && AIData.entities[j].Terrain == Entity.TerrainType.Ground && AIData.entities[j].category == Entity.EntityCategory.Unit)
                        {
                            enemyTank += 1;
                        }
                    }
                    if (mostNeeded == null){
                    if (enemyTank > ownTank){
                        if ((shellcore.GetPower() >= 150 && Random.Range(0,5) < 4) || Random.Range(0,3) == 1){
                            if (Random.Range(0,2) == 0){
                                mostNeeded = ("Beam Tank");
                            }
                            else {
                                mostNeeded = ("Laser Tank");
                            }
                        }
                        else if (shellcore.GetPower() >= 100 || Random.Range(0,2) == 1){
                            mostNeeded = ("Bullet Tank");
                        }
                        else {
                            mostNeeded = ("Speeder Tank");
                        }
                    }
                    else if (enemyGroundStation + enemyTank > ownTank){
                        if (shellcore.GetPower() >= 150 || Random.Range(0,3) == 1){
                            if (Random.Range(0,3) == 0){
                                mostNeeded = ("Beam Tank");
                            }
                            else {
                                mostNeeded = ("Siege Tank");
                            }
                        }
                        else if (shellcore.GetPower() >= 100 || Random.Range(0,2) == 1){
                            mostNeeded = ("Bullet Tank");
                        }
                        else {
                            mostNeeded = ("Speeder Tank");
                        }
                    }
                    else{
                        if ((shellcore.GetPower() >= 200 && Random.Range(0,4) < 3) || Random.Range(0,2) == 1){
                            mostNeeded = ("Missile Tank");
                        }
                        else {
                            mostNeeded = ("Laser Tank");
                        }
                    }
                    }

                    if (state == BattleState.Attack)
                    {
                        if (vendor.GetVendingBlueprint().items[0].entityBlueprint.intendedType == EntityBlueprint.IntendedType.Turret){
                            bool ownGroundExists = false;
                            for (int j = 0; j < AIData.entities.Count; j++)
                            {
                                if (AIData.entities[j].faction == craft.faction && AIData.entities[j].Terrain == Entity.TerrainType.Ground)
                                {
                                    ownGroundExists = true;
                                    break;
                                }
                            }
                            else if (shellcore.GetPower() >= 100)
                            {
                                // Attack & enemy holds all ground
                                itemIndex = vendor.GetVendingBlueprint().getItemIndex("Torpedo Turret");
                            }
                            else
                            {
                                if (shellcore.GetPower() >= 200)
                                {
                                    itemIndex = vendor.GetVendingBlueprint().getItemIndex("Missile Turret");
                                }
                                else if(shellcore.GetPower() >= 100)
                                {
                                    itemIndex = vendor.GetVendingBlueprint().getItemIndex("Defense Turret");
                                }
                            }
                        }
                        else if (vendor.GetVendingBlueprint().items[0].entityBlueprint.intendedType == EntityBlueprint.IntendedType.Tank){
                            itemIndex = vendor.GetVendingBlueprint().getItemIndex(mostNeeded);
                        }
                    }
                    if (state == BattleState.ReinforceGround){
                        itemIndex = vendor.GetVendingBlueprint().getItemIndex(mostNeeded);
                    }
                    if (itemIndex == -1)
                    {
                        if (harvesterTurrets.Count < Mathf.Min(5, AIData.energyRocks.Count)
                            && shellcore.GetPower() >= 100)
                        {
                            itemIndex = vendor.GetVendingBlueprint().getItemIndex("Harvester Turret");
                            foreach (var turret in harvesterTurrets.Values)
                            {
                                if (turret && Vector3.SqrMagnitude(turret.transform.position - shellcore.transform.position) <= 200)
                                {
                                    itemIndex = -1;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < vendor.GetVendingBlueprint().items.Count; j++)
                            {
                                if (itemIndex != -1 && vendor.GetVendingBlueprint().items[j].cost <= vendor.GetVendingBlueprint().items[itemIndex].cost && vendor.GetVendingBlueprint().items[0].entityBlueprint.intendedType != EntityBlueprint.IntendedType.Tank) // more expensive => better (TODO: choose based on the situation)
                                {
                                    if (itemIndex != -1 && vendor.GetVendingBlueprint().items[j].cost <= vendor.GetVendingBlueprint().items[itemIndex].cost) // more expensive => better (TODO: choose based on the situation)
                                    {
                                        continue;
                                    }

                                if (vendor.GetVendingBlueprint().items[j].entityBlueprint.name != mostNeeded && vendor.GetVendingBlueprint().items[0].entityBlueprint.intendedType == EntityBlueprint.IntendedType.Tank) //TODO: get turret / tank attack category from somewhere else
                                    continue;

                                itemIndex = j;
                            }
                        }
                    }

                    if (itemIndex != -1)
                    {
                        Debug.Log(mostNeeded);
                        if (vendor.GetVendingBlueprint().items[itemIndex].cost <= shellcore.GetPower()){
                            mostNeeded = null;
                        }
                        else {
                            break;
                        }
                        var ent = VendorUI.BuyItem(shellcore, itemIndex, (AIData.vendors[i] as IVendor));
                        if(itemIndex == vendor.GetVendingBlueprint().getItemIndex("Harvester Turret"))
                        {
                            EnergyRock closestRock = null;
                            foreach (var rock in AIData.energyRocks.FindAll(e => !harvesterTurrets.ContainsKey(e)))
                            {
                                if (closestRock == null || Vector2.SqrMagnitude(rock.transform.position - shellcore.transform.position)
                                    < Vector2.SqrMagnitude(closestRock.transform.position - shellcore.transform.position))
                                {
                                    closestRock = rock;
                                }
                            }

                            harvesterTurrets.Add(closestRock, ent as Turret);
                            shellcore.SetTractorTarget(ent.GetComponent<Draggable>());
                        }

                        break;
                    }
                }
            }
        }
    }

    void UpdateTargetInfluences()
    {
        for (int i = 0; i < AITargets.Count; i++)
        {
            var t = AITargets[i];
            if (t.entity == null || t.entity.GetIsDead())
            {
                Debug.LogWarning("AI Warning: AI target null or dead!");
                continue;
            }

            t.influence = 0f;
            for (int j = 0; j < AIData.entities.Count; j++)
            {
                if (AIData.entities[j] is Turret)
                {
                    if ((AIData.entities[j].transform.position - t.entity.transform.position).sqrMagnitude < 150f)
                    {
                        t.influence += FactionManager.IsAllied(AIData.entities[j].faction, t.entity.faction) ? 1f : -1f;
                    }
                }
            }
        }
    }

    bool enemyGroundTargets(bool allEntities)
    {
        Entity[] targets = allEntities ? AIData.entities.ToArray() : BattleZoneManager.getTargets();
        for (int i = 0; i < targets.Length; i++)
        {
            if (!FactionManager.IsAllied(targets[i].faction, craft.faction) && targets[i].Terrain == Entity.TerrainType.Ground)
            {
                return true;
            }
        }

        return false;
    }
}
