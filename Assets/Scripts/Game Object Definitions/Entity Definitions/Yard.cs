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

    protected override void Start() {
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

        if((transform.position - PlayerCore.Instance.transform.position).sqrMagnitude <= 100)
        {
            var player = PlayerCore.Instance;
            if(player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>())
            {
                var info = player.GetTractorTarget().GetComponent<ShellPart>().info;
				info = ShipBuilder.CullSpatialValues(info);
				player.cursave.partInventory.Add(info);

				PartIndexScript.AttemptAddToPartsObtained(info);
				PartIndexScript.AttemptAddToPartsSeen(info);
				Destroy(player.GetTractorTarget().GetComponent<ShellPart>().gameObject);

                DialogueSystem.Instance.PushPassiveDialogue(ID, "<color=lime>Your part has been added into your inventory.</color>");
            }
        }
    }
}
