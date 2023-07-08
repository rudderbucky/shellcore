﻿using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Missile : WeaponAbility
{
    public GameObject missilePrefab;
    public static readonly int missileDamage = 1000;

    protected override void Awake()
    {
        base.Awake();
        damage = missileDamage;
        description = $"Homing projectile that deals {damage} damage.";
        abilityName = "Missile";
        ID = AbilityID.Missile;
        cooldownDuration = 5F;
        range = 20;
        energyCost = 150;
        terrain = Entity.TerrainType.Air;
        category = Entity.EntityCategory.Unit;
        bonusDamageType = typeof(ShellCore);
    }

    protected override void Start()
    {
        missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
        base.Start();
    }

    protected override bool Execute(Vector3 victimPos)
    {
        AudioManager.PlayClipByID("clip_bullet2", transform.position);
        if (missilePrefab == null)
        {
            missilePrefab = ResourceManager.GetAsset<GameObject>("missile_prefab");
        }

        var missile = Instantiate(missilePrefab, transform.position, Quaternion.identity);
        var script = missile.GetComponent<MissileScript>();
        script.owner = GetComponentInParent<Entity>();
        script.SetTarget(targetingSystem.GetTarget());
        script.SetCategory(type == WeaponDiversityType.Torpedo ? Entity.EntityCategory.All : category);
        script.SetTerrain(type == WeaponDiversityType.Torpedo ? Entity.TerrainType.Ground : terrain);
        script.faction = Core.faction;
        script.SetDamage(GetDamage());
        script.StartSurvivalTimer(3);
        script.missileColor = part && part.info.shiny ? FactionManager.GetFactionShinyColor(Core.faction) : new Color(0.8F, 1F, 1F, 0.9F);

        if (SceneManager.GetActiveScene().name != "SampleScene" || MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
        {
            missile.GetComponent<NetworkProjectileWrapper>().enabled = false;
            missile.GetComponent<NetworkObject>().enabled = false;
        }

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost))
        {
            missile.GetComponent<NetworkObject>().Spawn();
        }
        base.Execute(victimPos);
        return true;
    }
}
