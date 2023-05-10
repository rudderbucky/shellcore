using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    static Vector2[][] _colliders;
    static Bounds[] _bounds;
    static int _ionFrame = 0;

    private void OnDrawGizmosSelected()
    {

        foreach (var entity in AIData.entities)
        {
            var points = SATCollision.GetColliders(entity, out var bounds);

            for (int i = 0; i < points.Length / 4; i++)
            {
                Debug.DrawLine(points[i * 4 + 0], points[i * 4 + 1], Color.cyan);
                Debug.DrawLine(points[i * 4 + 1], points[i * 4 + 2], Color.cyan);
                Debug.DrawLine(points[i * 4 + 2], points[i * 4 + 3], Color.cyan);
                Debug.DrawLine(points[i * 4 + 3], points[i * 4 + 0], Color.cyan);
            }

            float minX = bounds.min.x;
            float minY = bounds.min.y;
            float maxX = bounds.max.x;
            float maxY = bounds.max.y;
            Debug.DrawLine(new Vector2(minX, minY), new Vector2(minX, maxY), Color.green);
            Debug.DrawLine(new Vector2(minX, maxY), new Vector2(maxX, maxY), Color.green);
            Debug.DrawLine(new Vector2(maxX, maxY), new Vector2(maxX, minY), Color.green);
            Debug.DrawLine(new Vector2(maxX, minY), new Vector2(minX, minY), Color.green);
        }
    }

    private void FixedUpdate()
    {
        UpdateEntityColliders();
        EnergySphereCollisions();


        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost) return;
        // Projectile collisions

        var projectiles = AIData.collidingProjectiles.ToArray();
        foreach (var projectile in projectiles)
        {
            if (projectile == null) 
                continue;
            ProjectileCollision(projectile);
        }
    }

    static void UpdateEntityColliders()
    {
        _colliders = new Vector2[AIData.entities.Count][];
        _bounds = new Bounds[AIData.entities.Count];
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            _colliders[i] = SATCollision.GetColliders(AIData.entities[i], out var bounds);
            _bounds[i] = bounds;
        }
    }

    void EnergySphereCollisions()
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
            return;
        foreach (var entity in AIData.entities)
        {
            Vector3 pos = entity.transform.position;
            var spheres = AIData.energySpheres.ToArray();
            foreach (var energy in spheres)
            {
                if (!energy)
                    continue;
                if (energy.collected)
                    continue;
                if ((pos - energy.transform.position).sqrMagnitude < 1f)
                {
                    var harvester = entity.GetComponentInChildren<Harvester>();
                    if (entity is IHarvester || harvester != null)
                    {
                        if (harvester != null)
                        {
                            energy.Collect(harvester);
                        }
                        else
                        {
                            energy.Collect(entity as IHarvester);
                        }
                        break;
                    }
                }
            }
        }
    }

    static bool ProjectileCollision(IProjectile projectile)
    {
        UpdateEntityColliders();
        Vector2 pos = projectile.GetPosition();
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (!_bounds[i].Contains(pos))
                continue;
            Entity entity = AIData.entities[i];
            if ((pos - (Vector2)entity.transform.position).sqrMagnitude < 1024f)
            {
                if (entity.IsInvisible)
                    continue;
                if (entity.GetIsDead())
                    continue;
                if (entity.GetInvisible())
                    continue;
                if (FactionManager.IsAllied(projectile.GetFaction(), entity.faction))
                    continue;
                if (!projectile.CheckCategoryCompatibility(entity))
                    continue;
                if (projectile.GetOwner() && projectile.GetOwner() == entity)
                    continue;

                Vector2[] colliders = _colliders[i];
                for (int j = 0; j < colliders.Length / 4; j++)
                {
                    bool collision = SATCollision.PointInRectangle(
                        colliders[j * 4 + 0],
                        colliders[j * 4 + 1],
                        colliders[j * 4 + 2],
                        colliders[j * 4 + 3],
                        pos);
                    if (collision)
                    {
                        if (j == colliders.Length / 4 - 1)
                        {
                            projectile.HitDamageable(entity);
                            return true;
                        }
                        projectile.HitPart(entity.parts[j]);
                        return true;
                    }
                }
            }
        }

        foreach (var shard in AIData.shards)
        {
            Vector2 shardPos = shard.transform.position;
            if ((shardPos - pos).sqrMagnitude < 4f)
            {
                projectile.HitDamageable(shard);
                return true;
            }
        }

        return false;
    }

    // Almost same for-loops multiple times. Combine somehow?
    public static Transform GetTargetAtPosition(Vector2 pos)
    {
        UpdateEntityColliders();
        // Entities
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (!_bounds[i].Contains(pos))
                continue;
            Entity entity = AIData.entities[i];
            if ((pos - (Vector2)entity.transform.position).sqrMagnitude < 1024f)
            {
                if (entity.IsInvisible)
                    continue;
                if (entity.GetIsDead())
                    continue;
                if (entity.GetInvisible())
                    continue;
                if (entity == PlayerCore.Instance)
                    continue;

                Vector2[] colliders = _colliders[i];
                for (int j = 0; j < colliders.Length / 4; j++)
                {
                    bool collision = SATCollision.PointInRectangle(
                        colliders[j * 4 + 0],
                        colliders[j * 4 + 1],
                        colliders[j * 4 + 2],
                        colliders[j * 4 + 3],
                        pos);
                    if (collision)
                    {
                        return entity.transform;
                    }
                }
            }
        }

        // Shard Rocks
        foreach (var shard in AIData.shards)
        {
            Vector2 pos2 = shard.transform.position;
            if ((pos - pos2).sqrMagnitude < 4f)
            {
                return shard.transform;
            }
        }

        // Parts
        foreach (var part in AIData.strayParts)
        {
            Vector2 pos2 = part.transform.position;
            if ((pos - pos2).sqrMagnitude < 4f)
            {
                return part.transform;
            }
        }

        // Shards
        foreach (var shard in AIData.rockFragments)
        {
            Vector2 pos2 = shard.transform.position;
            if ((pos - pos2).sqrMagnitude < 4f)
            {
                return shard.transform;
            }
        }

        return null;
    }

    public static Transform[] GetAllTargetsAtPosition(Vector2 pos)
    {
        List<Transform> targets = new();

        // Entities
        for (int i = 0; i < AIData.entities.Count; i++)
        {
            if (!_bounds[i].Contains(pos))
                continue;
            Entity entity = AIData.entities[i];
            if ((pos - (Vector2)entity.transform.position).sqrMagnitude < 1024f)
            {
                if (entity.GetIsDead())
                    continue;
                if (entity.GetInvisible())
                    continue;
                if (entity == PlayerCore.Instance)
                    continue;

                Vector2[] colliders = _colliders[i];
                for (int j = 0; j < colliders.Length / 4; j++)
                {
                    bool collision = SATCollision.PointInRectangle(
                        colliders[j * 4 + 0],
                        colliders[j * 4 + 1],
                        colliders[j * 4 + 2],
                        colliders[j * 4 + 3],
                        pos);
                    if (collision)
                    {
                        targets.Add(entity.transform);
                        break;
                    }
                }
            }
        }

        // Shard Rocks
        foreach (var shard in AIData.shards)
        {
            Vector2 pos2 = shard.transform.position;
            if ((pos - pos2).sqrMagnitude < 4f)
            {
                targets.Add(shard.transform);
            }
        }

        // Parts
        foreach (var part in AIData.strayParts)
        {
            Vector2 pos2 = part.transform.position;
            if ((pos - pos2).sqrMagnitude < 4f)
            {
                targets.Add(part.transform);
            }
        }

        // Shards
        foreach (var shardFragment in AIData.rockFragments)
        {
            Vector2 pos2 = shardFragment.transform.position;
            if ((pos - pos2).sqrMagnitude < 4f)
            {
                targets.Add(shardFragment.transform);
            }
        }

        return targets.ToArray();
    }

    public static IDamageable RaycastDamageable(Vector2 start, Vector2 end, out Vector2 point)
    {
        // Update cache once per frame, in case there's multiple ion lines
        if (Time.frameCount >= _ionFrame)
        {
            UpdateEntityColliders();
            _ionFrame = Time.frameCount;
        }

        int pointCount = (int)((start - end).magnitude * 5f);
        Bounds lineBounds = new()
        {
            max = Vector2.Max(start, end),
            min = Vector2.Min(start, end),
        };

        for (int k = 0; k < pointCount; k++)
        {
            Vector2 pos = Vector2.Lerp(start, end, (float)k / pointCount);
            Debug.DrawLine(pos, pos + Vector2.up * 0.1f, Color.red);
        }

        // Entities
        for (int k = 0; k < pointCount; k++)
        {
            for (int i = 0; i < AIData.entities.Count; i++)
            {
            if (!_bounds[i].Intersects(lineBounds))
                continue;
            
                Vector2 pos = Vector2.Lerp(start, end, (float)k / pointCount);
                Vector2[] colliders = _colliders[i];
                for (int j = 0; j < colliders.Length / 4; j++)
                {
                    bool collision = SATCollision.PointInRectangle(
                        colliders[j * 4 + 0],
                        colliders[j * 4 + 1],
                        colliders[j * 4 + 2],
                        colliders[j * 4 + 3],
                        pos);
                    if (collision)
                    {
                        point = pos;
                        return AIData.entities[i];
                    }
                }
            }
        }

        // Shard Rocks
        
        for (int k = 0; k < pointCount; k++)
        {
            foreach (var shard in AIData.shards)
            {
                Vector2 pos = Vector2.Lerp(start, end, (float)k / pointCount);
                Vector2 pos2 = shard.transform.position;
                if ((pos - pos2).sqrMagnitude < 10f)
                {
                    point = pos;
                    return shard;
                }
            }
        }
        point = Vector2.zero;
        return null;
    }
}
