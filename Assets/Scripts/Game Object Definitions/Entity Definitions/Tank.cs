using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : GroundCraft
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (isOnGround)
        {
            targeter.GetTarget(true);
            if (!isDead && GetComponentInChildren<WeaponAbility>() && !draggable.dragging)
            {
                GetComponentInChildren<WeaponAbility>().Tick(null);
            }
            drive();
        }
    }

    protected virtual void drive()
    {
        isImmobile = !isOnGround;

        Vector3 direction = LandPlatformGenerator.getDirection(transform.position);

        if (direction != Vector3.zero && LandPlatformGenerator.CheckOnGround(transform.position + direction * 0.5f))
        {
            MoveCraft(direction);
        }
    }
}
