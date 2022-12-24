using UnityEngine;

public class SpeederMissile : WeaponAbility
{
    public GameObject MissilePrefab;
    public static readonly int missileDamage = 1000;

    protected override void Awake()
    {
        base.Awake();
        damage = missileDamage;
        description = $"Homing projectile that deals {damage} damage.";
        abilityName = "Speeder Missile";
        ID = AbilityID.SpeederMissile;
        cooldownDuration = 5F;
        range = 20;
        energyCost = 150;
        terrain = Entity.TerrainType.Air;
        category = Entity.EntityCategory.Unit;
        bonusDamageType = typeof(ShellCore);
    }

    protected override void Start()
    {
        MissilePrefab = ResourceManager.GetAsset<GameObject>("speeder_missile_prefab");
        base.Start();
    }

    protected override bool Execute(Vector3 victimPos)
    {
        AudioManager.PlayClipByID("clip_bullet2", transform.position);
        if (MissilePrefab == null)
        {
            MissilePrefab = ResourceManager.GetAsset<GameObject>("speeder_missile_prefab");
        }

        var missile = Instantiate(MissilePrefab, transform.position, Quaternion.identity);
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
