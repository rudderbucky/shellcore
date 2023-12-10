using System.Collections;
using UnityEngine;
using static Entity;

public class BombScript : MonoBehaviour
{
    private Transform target; // target of missile
    public Entity owner;
    private float damage; // damage bomb projectile should deal
    public Color bombColor;
    private Entity.EntityCategory category;
    private Entity.TerrainType terrain;
    public EntityFaction faction; // faction of projectile
    public static GameObject missPrefab;
    public static GameObject hitPrefab;
    public static readonly float explosionRadius = 3f;
    private static GameObject explosionCirclePrefab;
    private float timeInstantiated;
    private float fuseTime = 1.5F;

    private static void GetPrefabs()
    {
        if (!hitPrefab)
        {
            hitPrefab = ResourceManager.GetAsset<GameObject>("bullet_hit_prefab");
        }
        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            var script = explosionCirclePrefab.AddComponent<DrawCircleScript>();
            script.timeMin = 0F;
            explosionCirclePrefab.SetActive(false);
            script.SetStartColor(new Color(0.8F, 1F, 1F, 0.9F));
            script.color = new Color(0.8F, 1F, 1F, 0.9F);
        }

    }

    // Use this for initialization
    void Start()
    {
        timeInstantiated = Time.time;

        GetComponent<SpriteRenderer>().color = bombColor;
        ParticleSystem.MainModule mainModule = GetComponentInChildren<ParticleSystem>().main;
        mainModule.startColor = bombColor;

        GetPrefabs();

        SectorManager.OnSectorLoad += SectorLoaded;
    }

    /// <summary>
    /// Set the target of the missile projectile
    /// </summary>
    /// <param name="target">The transform of the to-be-set target</param>
    public void SetTarget(Transform target)
    {
        this.target = target; // set target
    }

    /// <summary>
    /// Set the damage of the missile projectile
    /// </summary>
    /// <param name="damage">damage</param>
    public void SetDamage(float damage)
    {
        this.damage = damage; // set damage
    }

    public void SetCategory(Entity.EntityCategory category)
    {
        this.category = category;
    }

    public void SetTerrain(Entity.TerrainType terrain)
    {
        this.terrain = terrain;
    }

    public void StartSurvivalTimer(float time)
    {
        StartCoroutine(DestroyTimer(time));
    }

    IEnumerator DestroyTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if (missPrefab)
            Instantiate(missPrefab, transform.position, Quaternion.Euler(0, 0,
                0 * Mathf.Rad2Deg));
        Destroy(gameObject);
    }

    public bool CheckCategoryCompatibility(IDamageable entity)
    {
        return (category == Entity.EntityCategory.All || category == entity.GetCategory()) && (terrain == Entity.TerrainType.All || terrain == entity.GetTerrain());
    }

    bool fired = false;

    private void Update()
    {
        if (Time.time <= timeInstantiated + fuseTime || fired) return;
        foreach (var ent in AIData.entities)
        {
            var craft = ent.GetComponent<Entity>(); // check if it has a craft component
            if (craft == null || craft.GetIsDead()) continue;
            if (craft as PlayerCore && DevConsoleScript.spectateEnabled) continue;
            if (FactionManager.IsAllied(faction, craft.faction)) continue;
            if (craft.GetInvisible()) continue;
            if (!CheckCategoryCompatibility(craft)) continue;
            if (owner && (craft.GetTransform() == owner.transform)) continue;
            if (Vector2.SqrMagnitude(craft.transform.position - transform.position) > explosionRadius * explosionRadius) continue;

            var residue = craft.TakeShellDamage(damage, 0, owner); // deal the damage to the target, no shell penetration
                                                                    // if the shell is low, damage the part
            craft.TakeCoreDamage(residue);
            if (fired) continue;
            fired = true;
            ActivationCosmetic(transform.position);
            if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide)
            {
                MasterNetworkAdapter.instance.BombExplosionClientRpc(transform.position);
            }
            Destroy(gameObject); // bullet has collided with a target, delete immediately
        }
    }

    public static void ActivationCosmetic(Vector3 position)
    {
        GetPrefabs();
        AudioManager.PlayClipByID("clip_bombexplosion", position);
        GameObject tmp = Instantiate(explosionCirclePrefab, position, Quaternion.identity); // instantiate circle explosion
        tmp.SetActive(true);
        tmp.transform.position = position;
        tmp.GetComponent<DrawCircleScript>().Initialize();
        for (int i = 0; i < 15; i++)
        {
            Instantiate(hitPrefab, position + new Vector3(Random.Range(-explosionRadius, explosionRadius),
                Random.Range(-explosionRadius, explosionRadius)), Quaternion.identity).transform.localScale *= 2;
        }
    }

    void SectorLoaded(string sector)
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        SectorManager.OnSectorLoad -= SectorLoaded;
    }

}
