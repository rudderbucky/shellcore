using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : AirCraft {

    protected override void Start()
    {
        base.Start();
        maxHealth[0] = 10;
        currentHealth[0] = 10;
        currentHealth[1] = maxHealth[1] = 1;
        currentHealth[2] = maxHealth[2] = 2500;
        enginePower = 0;
    }

    public override void RemovePart(ShellPart part) {
        Destroy(part.gameObject);
    }

    protected override void Update()
    {
        base.Update();
        //targeter.GetTarget(true);
        MainBullet[] bullets = GetComponentsInChildren<MainBullet>();
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].Tick();
        }
    }
    protected override void OnDeath()
    {
        faction = faction == 1 ? 0 : 1;
        targeter.SetTarget(null);
        for (int i = 0; i < parts.Count; i++)
        {
            RemovePart(parts[i]);
        }
        Start();
    }
}
