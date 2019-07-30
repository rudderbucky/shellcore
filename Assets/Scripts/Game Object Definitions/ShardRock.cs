using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable {
    Transform GetTransform();
    float[] GetHealth();
    float[] GetMaxHealth();
    Dialogue GetDialogue();
    bool GetIsDead();
    string GetName();
    int GetFaction();
}

public interface IDamageable : ITargetable {
    float TakeShellDamage(float damage, float shellPiercingFactor, Entity lastDamagedBy);
    Entity.TerrainType GetTerrain();
    Entity.EntityCategory GetCategory();
}

public class ShardRock : MonoBehaviour, IDamageable
{
    float[] currentHealths = new float[] {500, 0, 0};
    float[] maxHealths = new float[] {500, 500, 0};
    public bool dead = false;
    private GameObject explosionCirclePrefab;
    public Sprite[] shardSprites;
    public Sprite[] tierSprites;
    public GameObject shard;
    public int tier = 1;
    public void Start() {
        BuildRock();
    }

    private Color[] rockColors = new Color[] {
        new Color32((byte)51, (byte)153, (byte)204, (byte)255),
        new Color32((byte)153, (byte)204, (byte)51, (byte)255),
        new Color32((byte)204, (byte)51, (byte)153, (byte)255),
    };

    private void BuildRock() {
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
        currentHealths[0] -= damage;
        return 0;
    }

    public Transform GetTransform() {
        return transform;
    }

    public Dialogue GetDialogue() {
        return null;
    }

    public int GetFaction() {
        return 2;
    }

    public bool GetIsDead() {
        return dead;
    }

    public string GetName() {
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

    private void OnDeath() {
        dead = true;
        Destroy(gameObject, 0.75F);
        foreach(SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>()) {
            renderer.enabled = false;
        }
        ResourceManager.PlayClipByID("clip_explosion1", transform.position);
        GameObject tmp = Instantiate(explosionCirclePrefab); // instantiate circle explosion
        tmp.SetActive(true);
        tmp.transform.SetParent(transform, false);
        tmp.GetComponent<DrawCircleScript>().Initialize();
        Destroy(tmp, 1); // destroy explosions after 1 second
        for(int i = 0; i < 3; i++) {
            var rend = Instantiate(shard, null, false).GetComponent<SpriteRenderer>();
            rend.color = rockColors[tier];
            rend.transform.position = transform.position;   
            rend.sprite = shardSprites[Random.Range(0, 3)];
            rend.gameObject.AddComponent<Draggable>();
            var shardComp = rend.GetComponent<Shard>();
            shardComp.Detach();
            shardComp.SetCollectible(i == 0);
            shardComp.tier = tier;
        }
    }
    void Update() {
        if(currentHealths[0] <= 0 && !dead) OnDeath();
    }
}
