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

        category = EntityCategory.Station;
        base.Start();
    }

    public static readonly int YardProximitySquared = 75;

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
            if ((transform.position - PlayerCore.Instance.transform.position).sqrMagnitude <= YardProximitySquared)
            {
                var player = PlayerCore.Instance;
                foreach (var partyMember in PartyManager.instance.partyMembers)
                {
                    if ((transform.position - partyMember.transform.position).sqrMagnitude > YardProximitySquared)
                        partyMember.Warp(transform.position + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2)));
                }


                if (!player.HasRepaired)
                {
                    if (!player.IsFullyRepaired() && !player.GetIsDead())
                    {
                        player.repairFinalized = false;
                        StartCoroutine(player.StartYardRepair());
                    }
                }
                else if (player.repairFinalized)
                {
                    player.HealToMax();
                }

                player.HasRepaired = true;
                if (player.GetTractorTarget() && (player.GetTractorTarget().GetComponent<ShellPart>()
                                             || player.GetTractorTarget().GetComponent<Shard>()) && !tractor.GetTractorTarget())
                {
                    tractor.SetTractorTarget(player.GetTractorTarget());
                    player.SetTractorTarget(null);
                }
            }

            foreach (var partyMember in PartyManager.instance.partyMembers)
            {
                if (partyMember && partyMember.GetAI().getMode() == AirCraftAI.AIMode.Follow &&
                (transform.position - partyMember.transform.position).sqrMagnitude <= YardProximitySquared)
                {
                    if (!partyMember.HasRepaired)
                    {
                        if (!partyMember.IsFullyRepaired() && !partyMember.GetIsDead())
                        {
                            partyMember.repairFinalized = false;
                            StartCoroutine(partyMember.StartYardRepair());
                        }
                    }
                    else if (partyMember.repairFinalized)
                    {
                        partyMember.HealToMax();
                    }
                    partyMember.HasRepaired = true;
                }
            }

            if (tractor.GetTractorTarget() && (transform.position - tractor.GetTractorTarget().transform.position).sqrMagnitude <= 10)
            {
                if (tractor.GetTractorTarget().GetComponent<ShellPart>())
                {
                    PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Your part has been added into your inventory.</color>", 4);
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
                else if (tractor.GetTractorTarget().GetComponent<Shard>())
                {
                    PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Your shard has been added into your stash.</color>", 4);
                    var shard = tractor.GetTractorTarget().GetComponent<Shard>();
                    var tiers = new int[] { 1, 5, 20 };
                    PlayerCore.Instance.shards += tiers[shard.tier];
                    ShardCountScript.DisplayCount(PlayerCore.Instance.shards);
                    Destroy(shard.gameObject);
                }
            }
        }
    }
}
