using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    Transform GetTransform();
    float[] GetHealth();
    float[] GetMaxHealth();
    Dialogue GetDialogue();
    bool GetIsDead();
    string GetName();
    string GetID();
    int GetFaction();
    bool GetInvisible();
}

public interface IDamageable : ITargetable
{
    float TakeShellDamage(float damage, float shellPiercingFactor, Entity lastDamagedBy);
    Entity.TerrainType GetTerrain();
    Entity.EntityCategory GetCategory();
}

public class ShardRock : MonoBehaviour, IDamageable
{
    float[] currentHealths = new float[] { 500, 0, 0 };
    float[] maxHealths = new float[] { 500, 500, 0 };
    public bool dead = false;
    private GameObject explosionCirclePrefab;
    public Sprite[] shardSprites;
    public Sprite[] tierSprites;
    public GameObject shard;
    public static List<Shard> shards = new List<Shard>();
    public int tier = 0;
    public string ID;

    public bool LocationBasedShard
    {
        get { return ID != null; }
    }

    public void Start()
    {
        BuildRock();
        if (!transform.Find("Minimap Image")) //This was copied from the energy rock script, haven't figured a different method to show it on the minimap. And I don't know how long this will last. -FoeFear
        {
            GameObject childObject = new GameObject("Minimap Image");
            childObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = childObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ResourceManager.GetAsset<Sprite>("minimap_sprite");
            renderer.transform.localScale = new Vector3(0.7F, 0.7F);
            childObject.AddComponent<MinimapLockRotationScript>().Initialize();
        }
    }

    private Color[] rockColors = new Color[]
    {
        new Color32(51, 153, 204, 255),
        new Color32(153, 204, 51, 255),
        new Color32(204, 51, 153, 255)
    };

    private void BuildRock()
    {
        var rend = GetComponent<SpriteRenderer>();
        rend.sprite = tierSprites[tier];
        rend.color = Color.white;
        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            explosionCirclePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            var script = explosionCirclePrefab.AddComponent<DrawCircleScript>();
            script.timeMin = 0F;
            explosionCirclePrefab.SetActive(false);
            script.SetStartColor(rockColors[tier]);
            script.color = rockColors[tier];
        }
    }

    public float[] GetHealth()
    {
        return currentHealths;
    }

    public float[] GetMaxHealth()
    {
        return maxHealths;
    }

    public float TakeShellDamage(float damage, float shellPiercingFactor, Entity lastDamagedBy)
    {
        if (lastDamagedBy.GetFaction() != PlayerCore.Instance.GetFaction()) return 0;
        currentHealths[0] -= damage;
        currentHealths[0] = Mathf.Max(0, currentHealths[0]);
        return 0;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public Dialogue GetDialogue()
    {
        return null;
    }

    public int GetFaction()
    {
        return 2;
    }

    public bool GetIsDead()
    {
        return dead;
    }

    public string GetName()
    {
        return name;
    }

    public Entity.TerrainType GetTerrain()
    {
        return Entity.TerrainType.Air;
    }

    public Entity.EntityCategory GetCategory()
    {
        return Entity.EntityCategory.Unit;
    }

    private void OnDeath()
    {
        dead = true;
        currentHealths[0] = 0;
        Destroy(gameObject, 0.75F);
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.enabled = false;
        }

        AudioManager.PlayClipByID("clip_explosion1", transform.position);
        GameObject tmp = Instantiate(explosionCirclePrefab); // instantiate circle explosion
        tmp.SetActive(true);
        tmp.transform.SetParent(transform, false);
        tmp.GetComponent<DrawCircleScript>().Initialize();
        Destroy(tmp, 1); // destroy explosions after 1 second
        for (int i = 0; i < 3; i++)
        {
            var rend = Instantiate(shard, null, false).GetComponent<SpriteRenderer>();
            rend.color = rockColors[tier];
            rend.transform.position = transform.position;
            rend.sprite = shardSprites[Random.Range(0, 3)];
            if (i == 0)
            {
                rend.gameObject.AddComponent<Draggable>();
            }

            var shardComp = rend.GetComponent<Shard>();
            shardComp.Detach();
            if (!LocationBasedShard)
            {
                shardComp.SetCollectible(i == 0);
            }

            if (!LocationBasedShard)
            {
                AIData.rockFragments.Add(shardComp.GetComponent<Draggable>());
            }

            if (PlayerCore.Instance)
            {
                PlayerCore.Instance.cursave.locationBasedShardsFound.Add(ID);
            }

            shardComp.tier = tier;
        }

        if (LocationBasedShard && PlayerCore.Instance)
        {
            var tiers = new int[] { 1, 5, 20 };
            PlayerCore.Instance.cursave.shards += tiers[tier];
            ShardCountScript.DisplayCount(PlayerCore.Instance.cursave.shards);
        }
    }

    void Update()
    {
        if (currentHealths[0] <= 0 && !dead)
        {
            OnDeath();
        }
    }

    public string GetID()
    {
        return "-1";
    }

    public bool GetInvisible()
    {
        return false;
    }
}
