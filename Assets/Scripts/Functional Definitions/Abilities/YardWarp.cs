using UnityEngine;
/// <summary>
/// Heals all allies in range
/// </summary>
public class YardWarp : Ability
{
    const float range = 10;

    public override float GetRange()
    {
        return range;
    }

    protected override void Awake()
    {
        base.Awake(); // base awake
        // hardcoded values here
        ID = AbilityID.YardWarp;
        energyCost = 1000;
        cooldownDuration = 0.01f;
    }

    /// <summary>
    /// Heals all nearby allies
    /// </summary>
    protected override void Execute()
    {
        TractorBeam tractor = Core.GetComponent<TractorBeam>();
        if (tractor.GetTractorTarget() != null && tractor.GetTractorTarget().GetComponent<ShellPart>())
        {
            if (FactionManager.IsAllied(Core.faction,PlayerCore.Instance.faction)){
            PassiveDialogueSystem.Instance.PushPassiveDialogue(Core.ID, "<color=lime>Your part has been added into your inventory.</color>", 4);
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
            else {
                Core.TakeEnergy(-energyCost);
                var shellPart = tractor.GetTractorTarget().GetComponent<ShellPart>();
                PassiveDialogueSystem.Instance.PushPassiveDialogue(Core.ID, "<color=red>Your part has been added into your inventory. Sike, mine now!.</color>", 4);
                Destroy(shellPart.gameObject);
            }
        }
        else{
            Core.TakeEnergy(-energyCost);
            Draggable part = null;
            float dist = range * range; //Max distance of new tractor beam
            for (int i = 0; i < AIData.strayParts.Count; i++)
            {
                float d = (AIData.strayParts[i].transform.position - Core.transform.position).sqrMagnitude;
                Draggable target = AIData.strayParts[i].GetComponent<Draggable>();
                if (d < dist && target && !target.dragging)
                {
                    dist = d;
                    part = target;
                }
            }
            if (part != null)
            {
                tractor.SetTractorTarget(part);
            }
        }
    }
}
