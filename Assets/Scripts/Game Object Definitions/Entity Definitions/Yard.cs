using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShipBuilder {
    BuilderMode GetBuilderMode();
}

public enum BuilderMode {
    Yard,
    Trader,
    Workshop
}

public class Yard : AirConstruct, IShipBuilder {

    public BuilderMode mode;
    public BuilderMode GetBuilderMode() {
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

    protected override void Update()
    {
        targeter.GetTarget(true);
        if(!isDead)
            foreach(WeaponAbility weapon in GetComponentsInChildren<WeaponAbility>()) {
                weapon.Tick(0);
            }
        base.Update();

        if (FactionManager.IsAllied(faction, 0))
        {
            if ((transform.position - PlayerCore.Instance.transform.position).sqrMagnitude <= 75)
            {
                var player = PlayerCore.Instance;
                if (player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>() && !tractor.GetTractorTarget())
                {
                    tractor.SetTractorTarget(player.GetTractorTarget());
                    player.SetTractorTarget(null);
                }
            }
            if (tractor.GetTractorTarget() && (transform.position - tractor.GetTractorTarget().transform.position).sqrMagnitude <= 10)
            {
                if (tractor.GetTractorTarget().GetComponent<ShellPart>())
                {
                    PassiveDialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Your part has been added into your inventory.</color>");
                    var info = tractor.GetTractorTarget().GetComponent<ShellPart>().info;
                    info = ShipBuilder.CullSpatialValues(info);
                    ShipBuilder.AddOriginToDictionary(tractor.GetTractorTarget().GetComponent<ShellPart>());
                    PlayerCore.Instance.cursave.partInventory.Add(info);

                    PartIndexScript.AttemptAddToPartsObtained(info);
                    PartIndexScript.AttemptAddToPartsSeen(info);
                    Destroy(tractor.GetTractorTarget().GetComponent<ShellPart>().gameObject);
                }
            }
        }
    }
}
