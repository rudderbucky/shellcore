using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SiegeEntity
{
    public Sector.LevelEntity entity;
    public float timeSinceWaveStartToSpawn;
    public string flagName;
}

[System.Serializable]
public class SiegeWave
{
    public List<SiegeEntity> entities;
}

[System.Serializable]
public class WaveSet
{
    public SiegeWave[] waves;
}

public class SiegeZoneManager : MonoBehaviour
{
    public Queue<SiegeWave> waves;
    private List<Entity> targets;
    public SiegeWave current;
    private float timer = 0;
    private List<SiegeEntity> entitiesToRemove = new List<SiegeEntity>();
    private List<Entity> entitiesRemainingToRemove;
    private List<Entity> entitiesRemaining;
    private int waveCount = 0;
    public bool playing = false;
    public string sectorName;

    void OnEnable()
    {
        timer = 0;
        waveCount = 0;
        waves = new Queue<SiegeWave>();
        entitiesToRemove = new List<SiegeEntity>();
        entitiesRemaining = new List<Entity>();
        entitiesRemainingToRemove = new List<Entity>();
        targets = new List<Entity>();
        current = null;
        playing = true;
    }

    void OnDisable()
    {
        playing = false;
    }

    public void AlertPlayers(string message)
    {
        if (PlayerCore.Instance)
        {
            PlayerCore.Instance.alerter.showMessage(message, null);
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            playing = false;
            if (CoreScriptsManager.OnSiegeWin != null)
            {
                CoreScriptsManager.OnSiegeWin.Invoke(sectorName);
            }

            Debug.Log("Victory!");
            return;
        }
#endif

        if (playing && enabled)
        {
            timer += Time.deltaTime;
            entitiesToRemove.Clear();
            entitiesRemainingToRemove.Clear();

            if ((current == null || (current.entities.Count == 0 && entitiesRemaining.Count == 0)))
            {
                if (waves.Count > 0)
                {
                    entitiesRemaining.Clear();
                    current = waves.Dequeue();
                    waveCount++;
                    AlertPlayers($"WAVE {waveCount}/{(waves.Count + waveCount)}");
                    timer = 0;
                }
                else if ((current.entities.Count == 0 && entitiesRemaining.Count == 0))
                {
                    playing = false;
                    if (CoreScriptsManager.OnSiegeWin != null)
                    {
                        CoreScriptsManager.OnSiegeWin.Invoke(sectorName);
                    }

                    Debug.Log("Victory!");
                    return;
                }
            }

            // Debug.Log(current.entities.Count + " - " + entitiesRemaining.Count);

            foreach (var ent in current.entities)
            {
                if (timer >= ent.timeSinceWaveStartToSpawn)
                {
                    if (!AIData.flags.Exists((f) => f.name == ent.flagName))
                    {
                        Debug.LogError("<SiegeZoneManager> Invalid flag name.");
                        continue;
                    }

                    ent.entity.position = AIData.flags.Find((f) => f.name == ent.flagName).transform.position;
                    var sectorEntity = SectorManager.instance.SpawnEntity(SectorManager.GetBlueprintOfLevelEntity(ent.entity), ent.entity);
                    if (sectorEntity as Drone || sectorEntity as ShellCore)
                    {
                        Path path = ScriptableObject.CreateInstance<Path>();
                        path.waypoints = new List<Path.Node>();
                        Path.Node node = new Path.Node();
                        var currentTargets = targets.FindAll(targ => targ && !FactionManager.IsAllied(sectorEntity.faction.factionID, targ.faction.factionID));
                        if (currentTargets.Count > 0)
                        {
                            node.position = currentTargets[Random.Range(0, currentTargets.Count)].transform.position;
                        }
                        else if (PlayerCore.Instance)
                        {
                            node.position = PlayerCore.Instance.transform.position;
                        }

                        node.children = new List<int>();
                        path.waypoints.Add(node);

                        (sectorEntity as AirCraft).GetAI().setPath(path);
                    }

                    entitiesToRemove.Add(ent);
                    if (!FactionManager.IsAllied(sectorEntity.faction, PlayerCore.Instance.faction))
                    {
                        entitiesRemaining.Add(sectorEntity);
                    }
                }
            }

            foreach (var ent in entitiesToRemove)
            {
                current.entities.Remove(ent);
            }


            foreach (var ent in entitiesRemaining)
            {
                if (ent.GetIsDead())
                {
                    entitiesRemainingToRemove.Add(ent);
                }
            }

            foreach (var ent in entitiesRemainingToRemove)
            {
                entitiesRemaining.Remove(ent);
            }
        }
    }

    public void AddTarget(Entity target)
    {
        if (targets == null)
        {
            targets = new List<Entity>();
        }

        if (!playing)
        {
            targets.Clear();
        }

        if (target)
        {
            playing = true;
        }

        targets.Add(target);
    }
}
