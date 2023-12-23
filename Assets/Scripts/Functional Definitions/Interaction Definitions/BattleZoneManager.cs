using System.Collections.Generic;
using UnityEngine;
using static Entity;

public class BattleZoneManager : MonoBehaviour
{
    static List<Entity> targets;
    public string sectorName;
    public bool playing;
    public bool opposingFactionAdded = false;

    float startTime = 0f;
    public int CreditsCollected = 0;
    public bool overrideMode;

    public class Stats
    {
        public Stats(int faction)
        {
            this.faction = faction;
        }

        public int faction = 0;
        public int kills = 0;
        public int deaths = 0;
        public int power = 0;
        public int droneSpawns = 0;
        public int droneKills = 0;
        public int turretSpawns = 0;
        public int turretKills = 0;
    }

    public List<Stats> stats = new List<Stats>();

    void OnEnable()
    {
        opposingFactionAdded = false;
        targets = new List<Entity>();
        Entity.OnEntityDeath += OnEntityDeath;
        ShellCore.OnPowerCollected += OnPowerCollected;
        CreditsCollected = 0;
        startTime = Time.time;
        overrideMode = false;
    }

    private void OnDisable()
    {
        Entity.OnEntityDeath -= OnEntityDeath;
        ShellCore.OnPowerCollected -= OnPowerCollected;
        playing = false;
        ClearTargetsAndStats();
    }

    public void ClearTargetsAndStats()
    {
        if (targets == null || stats == null) return;
        targets.Clear();
        stats.Clear();
    }

    public int GetStatsFaction(Entity ent)
    {
        return overrideMode ? ent.faction.overrideFaction : ent.faction.factionID;
    }

    void OnEntityDeath(Entity killed, Entity killer)
    {
        Stats killedStats = stats.Find((x) => x.faction == GetStatsFaction(killed) );
        if (killedStats == null)
        {
            killedStats = new Stats(GetStatsFaction(killed));
            stats.Add(killedStats);
        }

        if (killed is ShellCore)
        {
            killedStats.deaths++;
        }

        if (killer != null)
        {
            Stats killerStats = stats.Find(x =>  x.faction == GetStatsFaction(killer) );
            if (killerStats == null)
            {
                killerStats = new Stats(GetStatsFaction(killer));
                stats.Add(killerStats);
            }

            if (killed is ShellCore)
            {
                killerStats.kills++;
            }
            else if (killed is Drone)
            {
                killerStats.droneKills++;
            }
            else if (killed is Turret)
            {
                killerStats.turretKills++;
            }
        }
    }

    void OnPowerCollected(Entity ent, int amount)
    {
        int faction = GetStatsFaction(ent);
        Stats block = stats.Find((x) => { return x.faction == faction; });
        if (block == null)
        {
            block = new Stats(faction);
            stats.Add(block);
        }

        block.power += amount;
    }

    private string GetFactionHighlight(Stats statBlock)
    {
        if (overrideMode) return $"{(statBlock.faction == PlayerCore.Instance.faction.overrideFaction ? "PLAYER" : "ENEMY" )}";
        return $"<color={FactionManager.GetFactionColorName(statBlock.faction)}>{(statBlock.faction == 0 ? "PLAYER" : "ENEMY")}</color>";
    }

    public string[] GetStats()
    {
        string[] strings = new string[stats.Count];

        stats.Sort((a, b) => {
            if (overrideMode && a.faction == PlayerCore.Instance.faction.overrideFaction)
                return -1;
            if (overrideMode && b.faction == PlayerCore.Instance.faction.overrideFaction)
                return 1;
                    
            return a.faction < b.faction ? -1 : 1; 
        });


        int index = 0;

        foreach (var statBlock in stats)
        {
            string str =
                $"{GetFactionHighlight(statBlock)}\n\n"
                + statBlock.kills + "\n"
                + statBlock.deaths + "\n"
                + (statBlock.deaths > 0 ? (statBlock.kills / statBlock.deaths).ToString() : "-") + "\n\n"
                + statBlock.power + "\n\n"
                + statBlock.droneSpawns + "\n"
                + statBlock.droneKills + "\n"
                + statBlock.turretSpawns + "\n"
                + statBlock.turretKills + "\n\n";

            if (index == 0)
            {
                str += $"{Mathf.RoundToInt(Time.time - startTime)}s\n\n{CreditsCollected}\n";
            }

            strings[index++] = str;
        }

        return strings;
    }

    public void AttemptAlertPlayers(EntityFaction faction, string message, string sound)
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
        {
            if (MasterNetworkAdapter.lettingServerDecide) return;
            MasterNetworkAdapter.instance.AlertPlayerClientRpc(faction.factionID, message, sound);
        }

        if (PlayerCore.Instance && FactionManager.IsAllied(faction, PlayerCore.Instance.faction)) AlertPlayer(message, sound);
    }

    public void AlertPlayer(string message, string sound)
    {
        if (PlayerCore.Instance && PlayerCore.Instance.alerter)
        {
            PlayerCore.Instance.alerter.showMessage(message, sound);
        }
    }


    public bool IsTarget(Entity ent)
    {
        return targets != null && targets.Contains(ent);
    }


    public void ResetCarriers()
    {
        foreach (var target in targets)
        {
            if (!SectorManager.instance.carriers.ContainsKey(GetStatsFaction(target)))
            {
                continue;
            }

            var carrier = SectorManager.instance.carriers[GetStatsFaction(target)];
            if (target is ShellCore shellCore && carrier != null && !carrier.Equals(null) && !carrier.GetIsDead())
            {
                shellCore.SetCarrier(SectorManager.instance.carriers[GetStatsFaction(target)]);
            }
        }
    }

    private List<int> GetLivingFactions()
    {
        List<int> livingFactions = new List<int>();

        // Create dictionary entries for counts of existing target entities of each faction
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] && !targets[i].GetIsDead() && !livingFactions.Contains(GetStatsFaction(targets[i])))
            {
                livingFactions.Add(GetStatsFaction(targets[i]));
            }
        }
        return livingFactions;
    }

    private bool GetAllFactionsAllied(List<int> livingFactions)
    {
        bool allAllied = true;
        for (int i = 0; i < livingFactions.Count; i++)
        {
            for (int j = 0; j < livingFactions.Count; j++)
            {
                var regularEnemies = !FactionManager.IsAllied(livingFactions[i], livingFactions[j]) ||
                    !FactionManager.IsAllied(livingFactions[j], livingFactions[i]);
                
                if (regularEnemies || overrideMode)
                {
                    allAllied = false;
                    break;
                }
            }
        }
        return allAllied;
    }

    public static float END_CHECK_TIMER;

    public void BattleZoneEndCheck(List<int> livingFactions, bool allAllied)
    {
        if ((livingFactions.Count >= 2 && !allAllied) || livingFactions.Count == 0) return;
        if (Time.time < END_CHECK_TIMER) return;
        if (!MasterNetworkAdapter.lettingServerDecide && MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
            MasterNetworkAdapter.instance.DisplayVoteClientRpc(livingFactions[0]);
        playing = false;

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide)
        {
            foreach (Entity playerEntity in targets)
            {
                if (playerEntity && !playerEntity.GetIsDead() && livingFactions.Contains(GetStatsFaction(playerEntity)) &&
                     playerEntity.networkAdapter && playerEntity.networkAdapter.isPlayer.Value)
                {
                    HUDScript.AddScore(playerEntity.networkAdapter.playerName, 50);
                }
            }
        }

        if (!PlayerCore.Instance) return;
        foreach (Entity playerEntity in targets)
        {
            if (!(playerEntity as PlayerCore)) continue;
            if (livingFactions.Contains(GetStatsFaction(playerEntity)))
            {
                AudioManager.PlayClipByID("clip_victory");
                if (CoreScriptsManager.OnBattleWin == null) continue;
                CoreScriptsManager.OnBattleWin.Invoke(sectorName);
            }
            else
            {
                AudioManager.PlayClipByID("clip_fail");
                if (CoreScriptsManager.OnBattleLose == null) continue;
                CoreScriptsManager.OnBattleLose.Invoke(sectorName);
            }
        }

        BattleZoneCreateResultsWindow(livingFactions);
    }

    private void BattleZoneCreateResultsWindow(List<int> livingFactions)
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Server && DialogueSystem.Instance)
        {
            DialogueSystem.ShowBattleResults(livingFactions.Contains(GetStatsFaction(PlayerCore.Instance)));
        }
        else if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && DialogueSystem.Instance)
        {
            DialogueSystem.Instance.StartSectorVote();
        }
    }

    public void UpdateCounters()
    {
        if (playing && enabled)
        {
            if (!opposingFactionAdded)
            {
                playing = false;
                return;
            }

            if (SectorManager.instance == null || SectorManager.instance.carriers == null) return;
            if (targets == null) return;
            ResetCarriers();
            var livingFactions = GetLivingFactions();

            bool allAllied = GetAllFactionsAllied(livingFactions);

            if (MasterNetworkAdapter.lettingServerDecide) return;
            BattleZoneEndCheck(livingFactions, allAllied);
        }
    }


    

    public void AddTarget(Entity target)
    {
        if (targets == null)
        {
            targets = new List<Entity>();
        }

        if (target.faction.overrideFaction != 0)
        {
            overrideMode = true;
        }

        if (target)
        {
            if (!opposingFactionAdded && targets.Exists(e => !FactionManager.IsAllied(e.GetFaction(), target.GetFaction())))
            {
                opposingFactionAdded = true;
            }
            playing = true;
        }

        if (!targets.Contains(target))
        {
            targets.Add(target);
        }
    }

    public static Entity[] getTargets()
    {
        return targets != null ? targets.ToArray() : null;
    }
}
