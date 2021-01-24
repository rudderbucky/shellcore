using System.Collections;
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
    public List<PlayerCore> players;
    private bool playing = false;
    public string sectorName;
    void OnEnable()
    {
        timer = 0;
        waveCount = 0;
        waves = new Queue<SiegeWave>();
        players = new List<PlayerCore>();
        entitiesToRemove = new List<SiegeEntity>();
        entitiesRemaining = new List<Entity>();
        entitiesRemainingToRemove = new List<Entity>();
        targets = new List<Entity>();
        current = null;
        playing = true;
    }

    public void AlertPlayers(string message) {
        foreach(PlayerCore player in players) {
            player.alerter.showMessage(message, null);
        }
    }

    void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.K))
        {
            playing = false;
            if(NodeEditorFramework.Standard.WinSiegeCondition.OnSiegeWin != null)
                NodeEditorFramework.Standard.WinSiegeCondition.OnSiegeWin.Invoke(sectorName);
            Debug.Log("Victory!");
            return;
        }
        #endif

        if(playing && enabled)
        {
            timer += Time.deltaTime;
            entitiesToRemove.Clear();
            entitiesRemainingToRemove.Clear();

            if((current == null || (current.entities.Count == 0 && entitiesRemaining.Count == 0)))
            {
                if(waves.Count > 0)
                {
                    entitiesRemaining.Clear();
                    current = waves.Dequeue();
                    waveCount++;
                    AlertPlayers("WAVE " + waveCount);
                    timer = 0;
                }
                else if((current.entities.Count == 0 && entitiesRemaining.Count == 0))
                {
                    playing = false;
                    if(NodeEditorFramework.Standard.WinSiegeCondition.OnSiegeWin != null)
                        NodeEditorFramework.Standard.WinSiegeCondition.OnSiegeWin.Invoke(sectorName);
                    Debug.Log("Victory!");
                    return;
                }
            }

            // Debug.Log(current.entities.Count + " - " + entitiesRemaining.Count);

            foreach(var ent in current.entities)
            {
                if(timer >= ent.timeSinceWaveStartToSpawn)
                {
                    ent.entity.position = AIData.flags.Find((f) => f.name == ent.flagName).transform.position;
                    var sectorEntity = SectorManager.instance.SpawnEntity(SectorManager.GetBlueprintOfLevelEntity(ent.entity), ent.entity);
                    if(sectorEntity as Drone || sectorEntity as ShellCore) 
                    {
                        Path path = ScriptableObject.CreateInstance<Path>();
                        path.waypoints = new List<Path.Node>();
                        Path.Node node = new Path.Node();
                        if(targets.Count > 0) node.position = targets[Random.Range(0, targets.Count)].transform.position;
                        else if(players.Count > 0) node.position = players[Random.Range(0, players.Count)].transform.position;
                        node.children = new List<int>();
                        path.waypoints.Add(node);

                        (sectorEntity as AirCraft).GetAI().setPath(path);
                    }
                    entitiesToRemove.Add(ent);
                    entitiesRemaining.Add(sectorEntity);
                }
            }

            foreach(var ent in entitiesToRemove)
            {
                current.entities.Remove(ent);
            }


            foreach(var ent in entitiesRemaining)
            {
                if(ent.GetIsDead())
                {
                    entitiesRemainingToRemove.Add(ent);
                }
            }

            foreach(var ent in entitiesRemainingToRemove)
            {
                entitiesRemaining.Remove(ent);
            }
        }
    }

    public void AddTarget(Entity target)
    {
        Debug.Log("target " + target.name);
        if (targets == null)
            targets = new List<Entity>();
        if (!playing)
            targets.Clear();
        if (target)
            playing = true;
        targets.Add(target);
    }
}
