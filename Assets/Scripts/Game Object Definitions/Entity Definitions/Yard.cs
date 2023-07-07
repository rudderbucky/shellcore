using UnityEngine;

public interface IShipBuilder
{
    BuilderMode GetBuilderMode();
}

public enum BuilderMode
{
    Yard,
    Trader,
    Workshop
}

public class Yard : AirConstruct, IShipBuilder
{
    public BuilderMode mode;

    public BuilderMode GetBuilderMode()
    {
        return mode;
    }

    private TractorBeam tractor;

    protected override void Start()
    {
        if (!tractor)
        {
            tractor = gameObject.AddComponent<TractorBeam>();
            tractor.owner = this;
            tractor.BuildTractor();
            tractor.SetEnergyEnabled(false);
        }

        Category = EntityCategory.Station;
        base.Start();
    }

    public static readonly int YardProximitySquared = 75;
    private static float lastPartTakenTime = 0;
    private static int partsTakenCombo = 0;

    protected override void Update()
    {
        if (!isDead)
        {
            foreach (WeaponAbility weapon in GetComponentsInChildren<WeaponAbility>())
            {
                weapon.Tick();
            }
        }

        base.Update();
        TargetManager.Enqueue(targeter);

        if (PlayerCore.Instance && FactionManager.IsAllied(faction, PlayerCore.Instance.faction))
        {
            RepairPlayerAndPartyMembers();
            GrabCollectiblesFromAllies();

            // Notify the player that their parts have been collected
            if (Yard.partsTakenCombo > 0 && Time.time - Yard.lastPartTakenTime > 1)
            {
                if (Yard.partsTakenCombo > 1)
                {
                    string message = string.Format("<color=lime>Your {0} parts have been added into your inventory.</color>", Yard.partsTakenCombo);
                    PassiveDialogueSystem.Instance.PushPassiveDialogue(GetComponent<Entity>().ID, message, 4);
                }
                else
                {
                    PassiveDialogueSystem.Instance.PushPassiveDialogue(GetComponent<Entity>().ID, "<color=lime>Your part has been added into your inventory.</color>", 4);
                }
                Yard.partsTakenCombo = 0;
            }
        }
    }

    public static void TakePart(Entity entity, TractorBeam tractor)
    {
        if (entity as Yard)
        {
            // Waits to see if it will get more parts
            Yard.lastPartTakenTime = Time.time;
            Yard.partsTakenCombo++;
        }
        else
        {
            PassiveDialogueSystem.Instance.PushPassiveDialogue(entity.ID, "<color=lime>Your part has been added into your inventory.</color>", 4);
        }
        var shellPart = tractor.GetTractorTarget().GetComponent<ShellPart>();
        var info = shellPart.info;
        info = ShipBuilder.CullSpatialValues(info);
        ShipBuilder.AddOriginToDictionary(shellPart);
        PlayerCore.Instance.cursave.partInventory.Add(info);
        PartIndexScript.AttemptAddToPartsObtained(info);
        PartIndexScript.AttemptAddToPartsSeen(info);
        if (NodeEditorFramework.Standard.YardCollectCondition.OnYardCollect != null)
        {
            NodeEditorFramework.Standard.YardCollectCondition.OnYardCollect.Invoke(info.partID, info.abilityID, shellPart.droppedSectorName);
        }
        Destroy(shellPart.gameObject);
    }

    private void GrabCollectiblesFromAllies()
    {
        var currentTarget = tractor.GetTractorTarget();

        if (currentTarget)
        {
            if (currentTarget && (transform.position - currentTarget.transform.position).sqrMagnitude <= 10)
            {
                if (currentTarget.GetComponent<ShellPart>())
                {
                    TakePart(GetComponent<Entity>(), tractor);
                }
                else if (currentTarget.GetComponent<Shard>())
                {
                    PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Your shard has been added into your stash.</color>", 4);
                    var shard = currentTarget.GetComponent<Shard>();
                    var tiers = new int[] { 1, 5, 20 };
                    PlayerCore.Instance.cursave.shards += tiers[shard.tier];
                    ShardCountScript.DisplayCount(PlayerCore.Instance.cursave.shards);
                    Destroy(shard.gameObject);
                }
            }
        }
        else
        {
            var player = PlayerCore.Instance;

            if ((transform.position - player.transform.position).sqrMagnitude < YardProximitySquared)
            {
                var playerTractor = player.GetComponentInChildren<TractorBeam>();

                if (playerTractor)
                {
                    var target = playerTractor.GetTractorTarget();

                    if (target && (target.GetComponent<ShellPart>() || target.GetComponent<Shard>()))
                    {
                        playerTractor.SetTractorTarget(null);
                        tractor.SetTractorTarget(target);

                        // Bad boy return here to prevent from looking into worker
                        // drones if already got something from the player.
                        return;
                    }
                }
            }
            
            foreach (Entity entity in player.GetUnitsCommanding())
            {
                if ((transform.position - entity.transform.position).sqrMagnitude > YardProximitySquared)
                    continue;
                
                var entityTractor = entity.GetComponentInChildren<TractorBeam>();

                if (!entityTractor)
                    continue;

                var target = entityTractor.GetTractorTarget();

                if (target && (target.GetComponent<ShellPart>() || target.GetComponent<Shard>()))
                {
                    entityTractor.SetTractorTarget(null);
                    tractor.SetTractorTarget(target);
                    break;
                }
            }
        }
    }

    private void RepairPlayerAndPartyMembers()
    {
        if ((transform.position - PlayerCore.Instance.transform.position).sqrMagnitude <= YardProximitySquared)
        {
            RepairShellCore(PlayerCore.Instance);

            // Warp party members to Yard so they can be healed too.
            // We heal them in a separate loop so they can also be healed
            // even if the player isn't neaby the Yard.
            foreach (var partyMember in PartyManager.instance.partyMembers)
            {
                if (!partyMember || partyMember.GetIsDead())
                    continue;

                // Only warp them if they're following the player?
                // if (partyMember.GetAI().getMode() != AirCraftAI.AIMode.Follow)
                //     continue;

                if ((transform.position - partyMember.transform.position).sqrMagnitude <= YardProximitySquared)
                    continue; // Already close enough
                
                if (partyMember.isYardRepairing || (!partyMember.HasPartsDamagedOrDestroyed() && !partyMember.HasShellOrCoreDamaged()))
                    continue; // Already repaired or repairing
                
                partyMember.Warp(transform.position + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2)));
            }
        }

        foreach (var partyMember in PartyManager.instance.partyMembers)
        {
            if (!partyMember || partyMember.GetIsDead())
                continue;
            
            if ((transform.position - partyMember.transform.position).sqrMagnitude <= YardProximitySquared)
                RepairShellCore(partyMember);
        }
    }

    private void RepairShellCore(ShellCore shellCore)
    {
        if (shellCore.isYardRepairing)
            return;

        if (shellCore.HasPartsDamagedOrDestroyed())
        {
            shellCore.StartYardRepairCoroutine();
        }
        else if (shellCore.HasShellOrCoreDamaged())
        {
            shellCore.HealToMax();
        }
    }
}
