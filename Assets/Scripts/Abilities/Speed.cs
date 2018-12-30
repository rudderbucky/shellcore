using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : PassiveAbility {

    protected override void Awake()
    {
        ID = 13;
    }

    protected override void Execute()
    {
        (Core as Craft).enginePower += 50;
    }
}
