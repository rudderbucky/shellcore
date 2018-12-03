using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outpost : AirConstruct {

    protected override void Start()
    {
        base.Start();
        currentHealth[0] = currentHealth[1] = maxHealth[0] = maxHealth[1] = 20;
    }
    protected override void Awake()
    {
        base.Awake();
        currentHealth[0] = maxHealth[0] = currentHealth[1] = maxHealth[1] = 10;
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
