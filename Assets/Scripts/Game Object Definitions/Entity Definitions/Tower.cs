using System.Collections.Generic;
using UnityEngine;

public class Tower : GroundConstruct
{   
    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
    }

    protected override void Update()
    {
        GetComponent<SpriteRenderer>().color = FactionManager.GetFactionColor(faction);
        GetComponent<SpriteRenderer>().material = transform.Find("Shell Sprite").GetComponent<SpriteRenderer>().material;

        float rate = 45 * Time.deltaTime;
        transform.Rotate(new Vector3(0, 0, rate));
        transform.Find("Shell Sprite").Rotate(new Vector3(0, 0, -rate));


        if (!isDead && GetComponentInChildren<PassiveAbility>())
        {
            GetComponentInChildren<PassiveAbility>().Tick();
        }
        base.Update();
    }
}
