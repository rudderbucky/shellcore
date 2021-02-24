using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    private Transform target; // target of missile
    public Entity owner;
    private float damage; // damage bomb projectile should deal
    public Color bombColor;
    private Entity.EntityCategory category;
    private Entity.TerrainType terrain;
    public int faction; // faction of projectile
    public GameObject missPrefab;
    public GameObject hitPrefab;
    public readonly float explosionRadius = 10f;
    private GameObject explosionCirclePrefab;

     // Use this for initialization
    void Start () {
        if (!GetComponent<Collider2D>()) // no collider? no problem
        {
            var collider = gameObject.AddComponent<CircleCollider2D>(); // add collider component
            collider.radius = explosionRadius;
            Debug.Log(collider.radius + " " + explosionRadius);
            collider.isTrigger = true; // set trigger
        }

        GetComponent<SpriteRenderer>().color = bombColor;
        ParticleSystem.MainModule mainModule = GetComponentInChildren<ParticleSystem>().main; 
        mainModule.startColor = bombColor;

        if (!explosionCirclePrefab)
        {
            explosionCirclePrefab = new GameObject("Explosion Circle");
            explosionCirclePrefab.transform.SetParent(transform, false);
            LineRenderer lineRenderer = explosionCirclePrefab.AddComponent<LineRenderer>();
            lineRenderer.material = ResourceManager.GetAsset<Material>("white_material");
            var script = explosionCirclePrefab.AddComponent<DrawCircleScript>();
            script.timeMin = 0F;
            explosionCirclePrefab.SetActive(false);
            script.SetStartColor(bombColor);
            script.color = bombColor;
        }
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
        Instantiate(missPrefab, transform.position, Quaternion.Euler(0, 0,
                0 * Mathf.Rad2Deg));
        Destroy(gameObject);
    }

    public bool CheckCategoryCompatibility(IDamageable entity)
    {
        return (category == Entity.EntityCategory.All || category == entity.GetCategory()) && (terrain == Entity.TerrainType.All || terrain == entity.GetTerrain());
    }

    bool fired = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.transform.root; // grab collision, get the topmost GameObject of the hierarchy, which would have the craft component
        var craft = hit.GetComponent<IDamageable>(); // check if it has a craft component
        if (craft != null && !craft.GetIsDead()) // check if the component was obtained
        {
            if (!FactionManager.IsAllied(faction, craft.GetFaction()) && CheckCategoryCompatibility(craft) && (!owner || (craft.GetTransform() != owner.transform)))
            {
                var residue = craft.TakeShellDamage(damage, 0, owner); // deal the damage to the target, no shell penetration
                                                        // if the shell is low, damage the part

                ShellPart part = collision.transform.GetComponent<ShellPart>();
                if (part)
                {
                    part.TakeDamage(residue); // damage the part
                }
                if(!fired)
                {
                    AudioManager.PlayClipByID("clip_bombexplosion", transform.position);
                    GameObject tmp = Instantiate(explosionCirclePrefab); // instantiate circle explosion
                    tmp.SetActive(true);
                    tmp.transform.position = transform.position;
                    tmp.GetComponent<DrawCircleScript>().Initialize();
                    for(int i = 0; i < 15; i++)
                    {
                        Instantiate(hitPrefab, transform.position + new Vector3(Random.Range(-explosionRadius, explosionRadius), 
                            Random.Range(-explosionRadius, explosionRadius)), Quaternion.identity).transform.localScale *= 2;
                    }
                }
                    
                fired = true;
                
                Destroy(gameObject); // bullet has collided with a target, delete immediately
            }
        }
    }
}
