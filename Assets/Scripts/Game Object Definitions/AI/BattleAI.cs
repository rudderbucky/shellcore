using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static VendingBlueprint.Item;

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
        public AITarget(Transform transform, float significance)
        {
            this.transform = transform;
            this.significance = significance;
        }

        public Transform transform;
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
    AIEquivalent? mostNeeded;

    Entity primaryTarget;
    Entity fortificationTarget;
    Turret attackTurret;

    EnergyRock collectTarget = null;
    bool findNewTarget = false;

    Draggable waitingDraggable;

    List<AITarget> AITargets = new List<AITarget>();
    Dictionary<EnergyRock, Turret> harvesterTurrets = new Dictionary<EnergyRock, Turret>();

    int _ownGroundStation = 0;
    int _ownTank = 0;
    int _enemyTank = 0;
    int _enemyGroundStation = 0;

    public void OnEntityDeath()
    {
        collectTarget = null;
        findNewTarget = true;
    }

    public BattleState GetState()
    {
        return state;
    }

    public void OrderModeChange(BattleState state)
    {
        this.state = state;
        nextStateCheckTime = Time.time + 15;

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
        collectTarget = null;

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
                if (FactionManager.IsAllied(targetEntities[i].faction, craft.faction))
                {
                    carriers.Add(targetEntities[i]);
                }
            }
        }

        if (craft is ShellCore shellCore)
        {
            shellcore = shellCore;
        }
        else
        {
            Debug.LogError("Battle zone AI should only be used by shellcores!");
        }

        foreach (IOwnable ownable in shellcore.GetUnitsCommanding())
        {
            if (ownable is Turret turret && turret.entityName == "Harvester Turret")
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
            if (AIData.vendors[i] == null || AIData.vendors[i].Equals(null)) continue;
            if (AIData.vendors[i] is TowerBase towerBase)
            {
                AITargets.Add(new AITarget(towerBase.GetTransform(), 50f));
                continue;
            }

            int rockCount = 0;
            for (int j = 0; j < AIData.energyRocks.Count; j++)
            {
                if (!AIData.energyRocks[j]) continue;
                if ((AIData.energyRocks[j].transform.position - AIData.vendors[i].GetPosition()).sqrMagnitude < 100)
                {
                    rockCount++;
                }
            }

            AITargets.Add(new AITarget(AIData.vendors[i].GetTransform(), rockCount + 1f));
        }

        for (int i = 0; i < carriers.Count; i++)
        {
            if (!carriers[i]) continue;
            AITargets.Add(new AITarget(carriers[i].GetTransform(), 100f));
        }
        AITargets.Sort((t1, t2) => (int)(t1.significance - t2.significance));
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
                    if (AirCraftAI.getEnemyCountInRange(carriers[i].transform.position, 45f, craft.faction.factionID) > 0)
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
            else if (AIData.vendors.Exists(v => v is TowerBase tbase && !tbase.TowerActive()) && shellcore.GetPower() >= 300)
            {
                state = BattleState.ReinforceGround;
            }
            else if (shellcore.GetPower() >= 300)
            {
                bool enemyGround = false;
                for (int j = 0; j < AIData.entities.Count; j++)
                {
                    if (AIData.entities[j].faction.factionID != craft.faction.factionID && AIData.entities[j].Terrain == Entity.TerrainType.Ground)
                    {
                        enemyGround = true;
                        break;
                    }
                }
                if (enemyGround)
                {
                    state = BattleState.ReinforceGround;
                    primaryTarget = null;
                }
                else
                {
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

                if ((shellcore.GetTractorTarget() != null && shellcore.GetTractorTarget().GetComponent<Turret>() != null && !turretIsHarvester
                                                          && ((SectorManager.instance.GetCurrentType() == Sector.SectorType.BattleZone
                                                                || SectorManager.instance.GetCurrentType() == Sector.SectorType.SiegeZone) ||
                                                            (shellcore.GetHealth()[0] > shellcore.GetMaxHealth()[0] * 0.1f))) 
                                                            || harvesterTurrets.Count >= Mathf.Min(1, AIData.energyRocks.Count))
                {
                    state = BattleState.Attack;
                }
                else
                {
                    state = BattleState.Collect;
                }
            }

            nextStateCheckTime = Time.time + 1f;
        }

        if (pState != state)
        {
            //Debug.LogFormat("Faction {0} Shellcore changed state to: {1}", craft.faction, state);
        }
    }

    private void AttackBattleState()
    {
        bool moveToTank = false;
        if (shellcore.GetTractorTarget() == null)
        {
            moveToTank = AttemptFindTank();
            if (attackTurret == null)
            {
                Turret t = null;
                float minDistance = float.MaxValue;
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (AIData.entities[i] != null && AIData.entities[i] &&
                        AIData.entities[i] is Turret &&
                        AIData.entities[i].faction.factionID == craft.faction.factionID &&
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

        if (primaryTarget != null && (!moveToTank || Vector2.SqrMagnitude(shellcore.transform.position - primaryTarget.transform.position) * 2 < Vector2.SqrMagnitude((Vector2)ai.movement.GetTarget() - (Vector2)primaryTarget.transform.position) + Vector2.SqrMagnitude((Vector2)ai.movement.GetTarget() - (Vector2)shellcore.transform.position)))
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
    }

    private void CollectBattleState(bool turretIsHarvester)
    {
        // go from outpost to outpost, (also less fortified enemy outposts [count enemy units nearby {TODO}]) and collect energy
        if (findNewTarget || collectTarget == null)
        {
            // Find new target
            float minD = float.MaxValue;
            EnergyRock targetRock = null;
            int maxEnergy = -1;

            for (int i = 0; i < AIData.energyRocks.Count; i++)
            {
                if (AirCraftAI.getEnemyCountInRange(AIData.energyRocks[i].transform.position, 10f, craft.faction.factionID) > 2)
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
                    state = BattleState.Attack;
                }

                findNewTarget = true;
            }
        }

    }

    private void DefendBattleState()
    {
        // destroy enemy units around base, ignore everything outside siege range
        if (primaryTarget && !primaryTarget.GetIsDead())
        {
            ai.movement.SetMoveTarget(primaryTarget.transform.position);
        }
    }

    void UpdateTargetInfluences()
    {
        /*
        for (int i = 0; i < AITargets.Count; i++)
        {
            var t = AITargets[i];
            if (!t.transform)
            {
                continue;
            }

            var ent = t.transform.GetComponent<Entity>();
            if (!ent || ent.GetIsDead()) continue;
            t.influence = 0f;
            for (int j = 0; j < AIData.entities.Count; j++)
            {
                if (AIData.entities[j] is Turret turret)
                {
                    if ((turret.transform.position - t.transform.transform.position).sqrMagnitude < 150f)
                    {
                        t.influence += FactionManager.IsAllied(turret.faction, ent.faction) ? 1f : -1f;
                    }
                }
            }
        }
        */
    }

    private void FortifyBattleState()
    {
        // TODO: place turrets
        // set primary target to an outpost with least defending turrets
        if (fortificationTarget == null || !fortificationTarget)
        {
            UpdateTargetInfluences();

            // float closestToZero = float.MaxValue;
            if (AITargets == null) return;
            for (int i = 0; i < AITargets.Count; i++)
            {
                if (AITargets[i] == null || !AITargets[i].transform) continue;
                var ent = AITargets[i].transform.GetComponent<Entity>();
                if (!ent) continue;
                if (ent.faction.factionID == shellcore.faction.factionID)
                {
                    fortificationTarget = ent;
                    break;
                }
            }
        }
        else if (attackTurret == null || !attackTurret)
        {
            UpdateTargetInfluences();

            float minDistance = float.MaxValue;

            for (int i = 0; i < AIData.entities.Count; i++)
            {
                if (AIData.entities[i] is Turret turret && turret.entityName != "Harvester Turret")
                {
                    float d = (craft.transform.position - turret.transform.position).sqrMagnitude;
                    float d2 = (fortificationTarget.transform.position - turret.transform.position).sqrMagnitude;
                    if (d < minDistance && d2 > 150f)
                    {
                        minDistance = d;
                        attackTurret = turret;
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
        else if (attackTurret && shellcore.GetTractorTarget() != attackTurret.GetComponent<Draggable>())
        {
            ai.movement.SetMoveTarget(attackTurret.transform.position, 100f);
            if (ai.movement.targetIsInRange())
            {
                var target = shellcore.GetTractorTarget();
                if (target != null && target && target.gameObject)
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
            ai.movement.SetMoveTarget(fortificationTarget.transform.position, 5f);
            Vector2 turretDelta = fortificationTarget.transform.position - attackTurret.transform.position;
            Vector2 targetPosition = (Vector2)fortificationTarget.transform.position + turretDelta.normalized * 16f;
            Vector2 delta = targetPosition - (Vector2)craft.transform.position;
            if (turretDelta.sqrMagnitude < 16f)
            {
                shellcore.SetTractorTarget(null);
                state = BattleState.Attack;
                ActionTick();
                nextStateCheckTime += 1f;
            }
        }

    }

    private void ReinforceGroundBattleState()
    {
        // The AI should be greedy with buying towers since they're free
        // Otherwise head to nearest bunker to produce tanks
        float dist = float.MaxValue;
        int index = -1;
        bool foundTowerBase = false;
        for (int i = 0; i < AITargets.Count; i++)
        {
            if (!AITargets[i].transform) continue;
            var towerBase = AITargets[i].transform.GetComponent<TowerBase>();
            if (towerBase && !towerBase.TowerActive() && !foundTowerBase)
            {
                foundTowerBase = true;
                dist = Vector2.SqrMagnitude(craft.transform.position - AITargets[i].transform.position);
                index = i;
                continue;
            }
            var bunker = AITargets[i].transform.GetComponent<Bunker>();
            if (((towerBase && !towerBase.TowerActive()) || 
                (!foundTowerBase && bunker && bunker.faction.factionID == shellcore.faction.factionID)) && 
                Vector2.SqrMagnitude(craft.transform.position - AITargets[i].transform.transform.position) < dist)
            {
                dist = Vector2.SqrMagnitude(craft.transform.position - AITargets[i].transform.position);
                index = i;
            }
        }
        if (index != -1 && dist >= 100f)
        {
            ai.movement.SetMoveTarget(AITargets[index].transform.position);
        }
        /*else{
            AttemptFindTank();
        }*/
    }
    



    private void UpdateWaitingDraggable()
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
    }

    private void AttemptDropHarvesterTurret()
    {
        bool turretIsHarvester = shellcore.GetTractorTarget() &&
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
                if ((rock.transform.position - shellcore.GetTractorTarget().GetComponent<Turret>().transform.position).sqrMagnitude > 125f)
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
                    break;
                }
            }
        }
    }

    private int AttemptCollectEnergy()
    {
        int energyCount = 0;
        if (shellcore.GetTractorTarget() && shellcore.GetTractorTarget().gameObject.GetComponent<EnergySphereScript>() == null)
        {
            for (int i = 0; i < AIData.energySpheres.Count; i++)
            {
                if ((AIData.energySpheres[i].transform.position - shellcore.transform.position).sqrMagnitude < 150)
                {
                    energyCount++;
                    var target = shellcore.GetTractorTarget();
                    if (!(target && target.GetComponent<Turret>() is Turret turret && turret.entityName == "Harvester Turret"))
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
        return energyCount;
    }

    private bool GetItemEnabled(AIEquivalent item)
    {
        return !shellcore.GetAI().vendingItemEnabled.ContainsKey(item) || shellcore.GetAI().vendingItemEnabled[item];
    }


    private void EvaluateMostNeededTank(int numEnemyTanks, int numOwnTanks, int numEnemyGroundStations, VendingBlueprint blueprint)
    {
        List<AIEquivalent> availableTanks = new();
        foreach (var item in blueprint.items)
        {
            // Enum values <= MissileTank are tanks
            if (item.equivalentTo <= AIEquivalent.MissileTank 
                && GetItemEnabled(item.equivalentTo)
                && item.cost <= shellcore.GetPower())
            {
                availableTanks.Add(item.equivalentTo);
            }
        }

        bool manyEnemies = numEnemyTanks > numOwnTanks;
        bool siegeRequired = (2 * numEnemyGroundStations + numEnemyTanks > numOwnTanks);

        List<(AIEquivalent, float)> pool = new();
        if (availableTanks.Contains(AIEquivalent.SpeederTank))
        {
            pool.Add((AIEquivalent.SpeederTank, 1f));
        }
        if (availableTanks.Contains(AIEquivalent.BulletTank))
        {
            pool.Add((AIEquivalent.BulletTank, 1.5f));
        }
        if (availableTanks.Contains(AIEquivalent.BeamTank))
        {
            pool.Add((AIEquivalent.BeamTank, 1.5f));
        }
        if (availableTanks.Contains(AIEquivalent.LaserTank) && !manyEnemies && !siegeRequired)
        {
            pool.Add((AIEquivalent.LaserTank, 3f));
        }
        if (availableTanks.Contains(AIEquivalent.MissileTank) && !manyEnemies && !siegeRequired)
        {
            pool.Add((AIEquivalent.MissileTank, 6f));
        }
        if (availableTanks.Contains(AIEquivalent.SiegeTank) && siegeRequired)
        {
            pool.Add((AIEquivalent.SiegeTank, 10f));
        }
        if (pool.Count == 0 && availableTanks.Count > 0)
        {
            pool.AddRange(availableTanks.Select(t => (t, 1f)));
        }

        float totalWeight = pool.Sum(t => t.Item2);
        float choice = Random.Range(0, totalWeight);
        foreach (var (tank, weight) in pool)
        {
            if (choice < weight)
            {
                mostNeeded = tank;
                break;
            }
            choice -= weight;
        }
    }

    void CountUnits()
    {
        _ownGroundStation = 0;
        _ownTank = 0;
        _enemyTank = 0;
        _enemyGroundStation = 0;


        foreach (var ent in AIData.entities)
        {
            if (FactionManager.IsAllied(ent.faction, craft.faction))
            {
                if (ent.Terrain == Entity.TerrainType.Ground && ent.Category == Entity.EntityCategory.Station)
                {
                    _ownGroundStation += 1;
                }
                if (ent.Terrain == Entity.TerrainType.Ground && ent.Category == Entity.EntityCategory.Unit)
                {
                    _ownTank += 1;
                }
            }
            else
            {
                if (ent.Terrain == Entity.TerrainType.Ground && ent.Category == Entity.EntityCategory.Station)
                {
                    _enemyGroundStation += 1;
                }
                if (ent.Terrain == Entity.TerrainType.Ground && ent.Category == Entity.EntityCategory.Unit)
                {
                    _enemyTank += 1;
                }
            }
        }
    }

    private void AttemptBuyUnits(int energyCount)
    {
        if ((energyCount > 0 && state != BattleState.ReinforceGround) || shellcore.unitsCommanding.Count >= shellcore.GetTotalCommandLimit()) return;
        CountUnits();
        for (int i = 0; i < AIData.vendors.Count; i++)
        {
            if ((AIData.vendors[i].GetPosition() - craft.transform.position).sqrMagnitude > 100f) continue;
            IVendor vendor = AIData.vendors[i] as IVendor;
            var blueprint = vendor.GetVendingBlueprint();
            if (blueprint == null)
            {
                continue;
            }

            int itemIndex = -1;
            if (vendor is TowerBase towerBase && !towerBase.TowerActive())
            {
                int healIndex = blueprint.getItemIndex(AIEquivalent.HealerTower);
                int energyIndex = blueprint.getItemIndex(AIEquivalent.EnergyTower);
                int speedIndex = blueprint.getItemIndex(AIEquivalent.SpeedTower);

                int healCost = healIndex != -1 ? blueprint.items[healIndex].cost : int.MaxValue;
                int energyCost = energyIndex != -1 ? blueprint.items[energyIndex].cost : int.MaxValue;
                int speedCost = speedIndex != -1 ? blueprint.items[speedIndex].cost : int.MaxValue;

                if (shellcore.GetRegens()[0] < shellcore.GetMaxHealth()[0] / 5 && shellcore.HealAuraStacks < 1 && shellcore.GetPower() >= healCost)
                {
                    itemIndex = healIndex;
                }
                else if (shellcore.GetRegens()[2] < shellcore.GetMaxHealth()[2] / 5 && shellcore.EnergyAuraStacks < 1 && shellcore.GetPower() >= energyCost)
                {
                    itemIndex = energyIndex;
                }
                else if (shellcore.GetPower() >= speedCost)
                {
                    itemIndex = speedIndex;
                }

                if (itemIndex != -1)
                {
                    BuyFromVendor(itemIndex, vendor);
                    break;
                }
            }

            if (vendor.NeedsAlliedFaction() && !FactionManager.IsAllied(vendor.GetFaction(), craft.GetFaction()))
            {
                continue;
            }

            if (mostNeeded == null)
            {
                EvaluateMostNeededTank(_enemyTank, _ownTank, _enemyGroundStation, blueprint);
            }

            if (state == BattleState.Attack)
            {
                // Item eq enum index >= 6: blueprint contains turrets
                if (blueprint.items[0].equivalentTo >= AIEquivalent.TorpedoTurret)
                {
                    bool ownGroundExists = false;
                    for (int j = 0; j < AIData.entities.Count; j++)
                    {
                        if (FactionManager.IsAllied(AIData.entities[j].faction, craft.faction) && AIData.entities[j].Terrain == Entity.TerrainType.Ground)
                        {
                            ownGroundExists = true;
                            break;
                        }
                    }
                    if (!ownGroundExists && IsEnemyGroundTargetPresent(true) && GetItemEnabled(AIEquivalent.TorpedoTurret))
                    {
                        int torpedoTurretIndex = blueprint.getItemIndex(AIEquivalent.TorpedoTurret);
                        int torpedoTurretCost = torpedoTurretIndex != -1 ? blueprint.items[torpedoTurretIndex].cost : int.MaxValue;
                        if (shellcore.GetPower() >= torpedoTurretCost)
                        {
                            itemIndex = torpedoTurretIndex;
                        }
                    }
                    if (GetItemEnabled(AIEquivalent.MissileTurret) && itemIndex == -1)
                    {
                        int missileTurretIndex = blueprint.getItemIndex(AIEquivalent.MissileTurret);
                        int missileTurretCost = missileTurretIndex != -1 ? blueprint.items[missileTurretIndex].cost : int.MaxValue;
                        if (shellcore.GetPower() >= missileTurretCost)
                        {
                            itemIndex = missileTurretIndex;
                        }
                    }
                    if (GetItemEnabled(AIEquivalent.DefenseTurret) && itemIndex == -1)
                    {
                        int defenseTurretIndex = blueprint.getItemIndex(AIEquivalent.DefenseTurret);
                        int defenseTurretCost = defenseTurretIndex != -1 ? blueprint.items[defenseTurretIndex].cost : int.MaxValue;
                        if (shellcore.GetPower() >= defenseTurretCost)
                        {
                            itemIndex = defenseTurretIndex;
                        }
                    }
                }
                // Else, a tank has possibly been selected
                else if (mostNeeded.HasValue)
                {
                    itemIndex = blueprint.getItemIndex(mostNeeded.Value);
                }
            }
            if (state == BattleState.ReinforceGround && mostNeeded.HasValue)
            {
                itemIndex = blueprint.getItemIndex(mostNeeded.Value);
            }
            if (itemIndex == -1)
            {
                itemIndex = EvaluateItemIndexIfNoPriority(vendor);
            }


            if (itemIndex != -1)
            {
                BuyFromVendor(itemIndex, vendor);
                break;
            }
        }
    }

    private int EvaluateItemIndexIfNoPriority(IVendor vendor)
    {
        var blueprint = vendor.GetVendingBlueprint();
        int itemIndex = -1;
        int harvesterTurretIndex = blueprint.getItemIndex(AIEquivalent.HarvesterTurret);
        int harvesterTurretCost = harvesterTurretIndex != -1 ? blueprint.items[harvesterTurretIndex].cost : int.MaxValue;
        if (harvesterTurrets.Count < Mathf.Min(5, AIData.energyRocks.Count)
                    && shellcore.GetPower() >= harvesterTurretCost  && GetItemEnabled(AIEquivalent.HarvesterTurret))
        {
            itemIndex = harvesterTurretIndex;
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
            for (int j = 0; j < blueprint.items.Count; j++)
            {
                if (itemIndex != -1 && blueprint.items[j].cost <= blueprint.items[itemIndex].cost && blueprint.items[0].entityBlueprint.intendedType != EntityBlueprint.IntendedType.Tank) // more expensive => better (TODO: choose based on the situation)
                {
                    if (itemIndex != -1 && blueprint.items[j].cost <= blueprint.items[itemIndex].cost) // more expensive => better (TODO: choose based on the situation)
                    {
                        continue;
                    }

                    if (blueprint.items[j].equivalentTo != mostNeeded && blueprint.items[0].entityBlueprint.intendedType == EntityBlueprint.IntendedType.Tank) //TODO: get turret / tank attack category from somewhere else
                        continue;

                    if (!GetItemEnabled(blueprint.items[j].equivalentTo))
                        continue;
                        
                    itemIndex = j;
                }
            }
        }
        return itemIndex;
    }

    private void BuyFromVendor(int itemIndex, IVendor vendor)
    {
        if (vendor.GetVendingBlueprint().items[itemIndex].cost <= shellcore.GetPower())
        {
            mostNeeded = null;
        }
        else
        {
            return;
        }
        var ent = VendorUI.BuyItem(shellcore, itemIndex, vendor);
        if (itemIndex == vendor.GetVendingBlueprint().getItemIndex(AIEquivalent.HarvesterTurret))
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
    }

    public override void ActionTick()
    {
        UpdateWaitingDraggable();

        var tractorTarget = shellcore.GetTractorTarget();
        var turretIsHarvester = tractorTarget &&
                                tractorTarget.GetComponent<Turret>()
                                && tractorTarget.GetComponent<Turret>().entityName == "Harvester Turret";

        AttemptMoveTank();
        var hasTank = tractorTarget && tractorTarget.GetComponent<Tank>();
        if (!hasTank && craft && !craft.GetIsDead())
        switch (state)
        {
            case BattleState.Attack:
                AttackBattleState();
                break;
            case BattleState.Defend:
                DefendBattleState();
                // buy a turret matching the biggest threat's element, if possible
                break;
            case BattleState.Collect:
                CollectBattleState(turretIsHarvester);
                break;
            case BattleState.Fortify:
                FortifyBattleState();
                break;
            case BattleState.ReinforceGround:
                ReinforceGroundBattleState();
                break;
            default:
                break;
        }

        // always drop harvester turrets on close energy rocks
        AttemptDropHarvesterTurret();

        if (!hasTank)
        {
            // always collect energy
            int energyCount = AttemptCollectEnergy();

            // always buy more turrets/tanks
            AttemptBuyUnits(energyCount);
        }


    }

    private bool AttemptFindTank()
    {
        if(shellcore.GetTractorTarget() == null && FindTankDropoffFlag() != null)
        {
            var pickupTargetFlag = FindTankPickupFlag();
            if (pickupTargetFlag && Vector2.SqrMagnitude(pickupTargetFlag.transform.position - craft.transform.position) > 250F)
            {
                ai.movement.SetMoveTarget(pickupTargetFlag.transform.position, 1F);
                return true;
            }
            else if (pickupTargetFlag)
            {
                foreach (var tank in AIData.tanks)
                {
                    if (!FactionManager.IsAllied(tank.faction, craft.faction)) continue;
                    if (Vector2.SqrMagnitude(tank.transform.position - pickupTargetFlag.transform.position) > 25) continue;
                    shellcore.SetTractorTarget(tank.GetComponentInChildren<Draggable>());
                    break;
                }
            }
        }
        return false;
    }

    private void AttemptMoveTank()
    {

        var hasTank = shellcore.GetTractorTarget() && shellcore.GetTractorTarget().GetComponent<Tank>();

        Flag tankPickupDropoffFlag = null;
        if (hasTank)
            tankPickupDropoffFlag = FindTankDropoffFlag();

        if (tankPickupDropoffFlag && Vector2.SqrMagnitude(tankPickupDropoffFlag.transform.position - shellcore.GetTractorTarget().transform.position) > 1F)
        {
            ai.movement.SetMoveTarget(tankPickupDropoffFlag.transform.position + (tankPickupDropoffFlag.transform.position - shellcore.GetTractorTarget().transform.position), 1F);
        }
        else if (tankPickupDropoffFlag)
        {
            tankPickupDropoffFlag = null;
            shellcore.SetTractorTarget(null);
        }
    }

    private Flag FindTankPickupFlag()
    {
        foreach (var flag in AIData.flags.OrderBy(x => Vector2.SqrMagnitude(x.transform.position - craft.transform.position)))
        {
            if (flag.name != $"tankpickup{craft.faction.factionID}") continue;
            foreach (var tank in AIData.tanks)
            {
                if (!FactionManager.IsAllied(tank.faction.factionID, craft.faction.factionID)) continue;
                if (Vector2.SqrMagnitude(tank.transform.position - flag.transform.position) > 25) continue;
                return flag;
            }
        }
        return null;
    }

    private Flag FindTankDropoffFlag()
    {
        foreach (var flag in AIData.flags.OrderBy(x => Vector2.SqrMagnitude(x.transform.position - craft.transform.position)))
        {
            if (flag.name != $"tankdropoff{craft.faction.factionID}") continue;
            int alliedPresence = 0;
            int enemyPresence = 0;
            foreach(var entity in AIData.entities)
            {
                if(Vector2.SqrMagnitude(flag.transform.position - entity.transform.position) > 25) continue;
                if(!(entity as GroundConstruct)) continue;
                if(entity == shellcore || (shellcore && shellcore.GetTractorTarget() && entity == shellcore.GetTractorTarget().GetComponent<Entity>())) continue;
                if(entity.faction.factionID == craft.faction.factionID){alliedPresence++;}
                else{enemyPresence++;}
            }
            if(alliedPresence <= enemyPresence) return flag;
        }
        return null;
    }

    private bool IsEnemyGroundTargetPresent(bool allEntities)
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
