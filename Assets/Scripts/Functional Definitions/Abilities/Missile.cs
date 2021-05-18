using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : WeaponAbility {

    public GameObject missilePrefab;
    public static readonly int missileDamage = 1000;
    protected override void Awake()
    {
        base.Awake();
        damage = missileDamage;
        description = "Homing projectile that deals " + damage + " damage.";
        abilityName = "Missile";
        ID = AbilityID.Missile;
        cooldownDuration = 5F;
        range = 25;
        energyCost = 150;
        terrain = Entity.TerrainType.Air;
        category = Entity.EntityCategory.All;
        bonusDamageType = typeof(ShellCore);
    }

    protected override void Start() {
        missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
        base.Start();
    }
    protected override bool Execute(Vector3 victimPos)
    {
        AudioManager.PlayClipByID("clip_bullet2", transform.position);
        if (missilePrefab == null)
            missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
        var missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        var script = missile.GetComponent<MissileScript>();
        script.owner = GetComponentInParent<Entity>();
        script.SetTarget(targetingSystem.GetTarget());
        script.SetCategory(category);
        script.SetTerrain(terrain);
        script.faction = Core.faction;
        script.SetDamage(GetDamage());
        script.StartSurvivalTimer(3);
        script.missileColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);
        base.Execute(victimPos);
        return true;
    }
}
