using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    private void OnDrawGizmosSelected()
    {

        foreach (var entity in AIData.entities)
        {
            var points = SATCollision.GetColliders(entity);

            for (int i = 0; i < points.Length / 4; i++)
            {
                Debug.DrawLine(points[i * 4 + 0], points[i * 4 + 1], Color.cyan);
                Debug.DrawLine(points[i * 4 + 1], points[i * 4 + 2], Color.cyan);
                Debug.DrawLine(points[i * 4 + 2], points[i * 4 + 3], Color.cyan);
                Debug.DrawLine(points[i * 4 + 3], points[i * 4 + 0], Color.cyan);
            }
        }
    }

    private void FixedUpdate()
    {
        EnergySphereCollisions();


        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost) return;
        // Projectile collisions

        foreach (var projectile in AIData.collidingProjectiles)
        {
            Vector2 pos = projectile.GetPosition();
            bool hit = false;
            foreach (var entity in AIData.entities)
            {
                if ((pos - (Vector2)entity.transform.position).sqrMagnitude < 1024f)
                {
                    if (hit)
                        break;
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

                    // SAT
                    Vector2[] colliders = SATCollision.GetColliders(entity);
                    for (int i = 0; i < colliders.Length / 4; i++)
                    {
                        bool collision = SATCollision.PointInRectangle(
                            colliders[i * 4 + 0],
                            colliders[i * 4 + 1],
                            colliders[i * 4 + 2],
                            colliders[i * 4 + 3],
                            pos);
                        if (collision)
                        {
                            if (i == colliders.Length / 4 - 1)
                            {
                                projectile.HitDamageable(entity);
                                break;
                            }
                            projectile.HitPart(entity.parts[i]);
                            hit = true;
                            break;
                        }
                    }
                }
            }
        }

        foreach (var shard in AIData.shards)
        {
            Vector2 pos = shard.transform.position;
            foreach (var projectile in AIData.collidingProjectiles)
            {
                if ((pos - projectile.GetPosition()).sqrMagnitude < 4f)
                {
                    projectile.HitDamageable(shard);
                }
            }
        }
    }

    void EnergySphereCollisions()
    {
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
            return;
        foreach (var entity in AIData.entities)
        {
            Vector3 pos = entity.transform.position;
            foreach (var energy in AIData.energySpheres)
            {
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
                    }
                }
            }
        }
    }

    // Almost same for-loops multiple times. Combine somehow?
    public static ITargetable GetTargetAtPosition(Vector2 pos)
    {
        // Entities
        foreach (var entity in AIData.entities)
        {
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

                Vector2[] colliders = SATCollision.GetColliders(entity);
                for (int i = 0; i < colliders.Length / 4; i++)
                {
                    bool collision = SATCollision.PointInRectangle(
                        colliders[i * 4 + 0],
                        colliders[i * 4 + 1],
                        colliders[i * 4 + 2],
                        colliders[i * 4 + 3],
                        pos);
                    if (collision)
                    {
                        return entity;
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
                return shard;
            }
        }

        return null;
    }

    public static ITargetable[] GetAllTargetsAtPosition(Vector2 pos)
    {
        List<ITargetable> targets = new();

        // Entities
        foreach (var entity in AIData.entities)
        {
            if ((pos - (Vector2)entity.transform.position).sqrMagnitude < 1024f)
            {
                if (entity.GetIsDead())
                    continue;
                if (entity.GetInvisible())
                    continue;
                //if (entity == PlayerCore.Instance)
                //    continue;

                Vector2[] colliders = SATCollision.GetColliders(entity);
                for (int i = 0; i < colliders.Length / 4; i++)
                {
                    bool collision = SATCollision.PointInRectangle(
                        colliders[i * 4 + 0],
                        colliders[i * 4 + 1],
                        colliders[i * 4 + 2],
                        colliders[i * 4 + 3],
                        pos);
                    if (collision)
                    {
                        targets.Add(entity);
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
                targets.Add(shard);
            }
        }

        return targets.ToArray();
    }
}
