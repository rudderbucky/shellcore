using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleZoneManager : MonoBehaviour
{

    static List<Entity> targets;
    public string sectorName;
    bool playing;

    float startTime = 0f;
    public int CreditsCollected = 0;

    class Stats
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

    List<Stats> stats = new List<Stats>();

    void OnEnable()
    {
        targets = new List<Entity>();
        Entity.OnEntityDeath += OnEntityDeath;
        ShellCore.OnPowerCollected += OnPowerCollected;
        CreditsCollected = 0;
        startTime = Time.time;
    }

    private void OnDisable()
    {
        Entity.OnEntityDeath -= OnEntityDeath;
        ShellCore.OnPowerCollected -= OnPowerCollected;
        targets.Clear();
        stats.Clear();
    }

    void OnEntityDeath(Entity killed, Entity killer)
    {
        Stats killedStats = stats.Find((x)=>{ return x.faction == killed.faction; });
        if (killedStats == null)
        {
            killedStats = new Stats(killed.faction);
            stats.Add(killedStats);
        }
        if (killed is ShellCore)
            killedStats.deaths++;

        if (killer != null)
        {
            Stats killerStats = stats.Find(x => x.faction == killer.faction);
            if (killerStats == null)
            {
                killerStats = new Stats(killer.faction);
                stats.Add(killerStats);
            }
            if (killed is ShellCore)
                killerStats.kills++;
            else if (killed is Drone)
                killerStats.droneKills++;
            else if (killed is Turret)
                killerStats.turretKills++;
        }
    }

    void OnPowerCollected(int faction, int amount)
    {
        Stats block = stats.Find((x)=>{ return x.faction == faction; });
        if (block == null)
        {
            block = new Stats(faction);
            stats.Add(block);
        }
        block.power += amount;
    }

    public string[] GetStats()
    {
        string[] strings = new string[stats.Count];

        stats.Sort((a, b) => { return a.faction < b.faction ? -1 : 1; });

        int index = 0;

        foreach (var statBlock in stats)
        {
            string str = "";
            str += "<color=" + FactionManager.GetFactionColorName(statBlock.faction) + ">" + (statBlock.faction == 0 ? "PLAYER" : "ENEMY") + "</color>\n\n";
            str += statBlock.kills + "\n";
            str += statBlock.deaths + "\n";
            str += (statBlock.deaths > 0 ? (statBlock.kills / statBlock.deaths).ToString() : "-") + "\n";
            str += "\n";
            str += statBlock.power + "\n";
            str += "\n";
            str += statBlock.droneSpawns + "\n";
            str += statBlock.droneKills + "\n";
            str += statBlock.turretSpawns + "\n";
            str += statBlock.turretKills + "\n";

            if (index == 0)
            {
                str += "\n";
                str += Mathf.RoundToInt(Time.time - startTime) + "s\n";
                str += "\n";
                str += CreditsCollected + "\n";
            }

            strings[index++] = str;
        }
        return strings;
    }

    public void AlertPlayers(int faction, string message)
    {
        if (faction == 0 && PlayerCore.Instance)
        {
            PlayerCore.Instance.alerter.showMessage(message, "clip_stationlost");
        }
    }
    public void UpdateCounters()
    {
        if (playing && enabled)
        {
            foreach(var target in targets)
            {
                if(!SectorManager.instance.carriers.ContainsKey(target.faction)) continue;
                var carrier = SectorManager.instance.carriers[target.faction];
                if(target as ShellCore && carrier != null && !carrier.Equals(null) && !carrier.GetIsDead()) 
                    (target as ShellCore).SetCarrier(SectorManager.instance.carriers[target.faction]);
            }

            List<int> livingFactions = new List<int>();

            // Create dictionary entries for counts of existing target entities of each faction
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] && !targets[i].GetIsDead() && !livingFactions.Contains(targets[i].faction))
                    livingFactions.Add(targets[i].faction);
            }

            bool allAllied = true;

            for (int i = 0; i < livingFactions.Count; i++)
            {
                for (int j = 0; j < livingFactions.Count; j++)
                {
                    if (!FactionManager.IsAllied(livingFactions[i], livingFactions[j]) ||
                        !FactionManager.IsAllied(livingFactions[j], livingFactions[i]))
                    {
                        allAllied = false;
                        break;
                    }
                }
            }

            if (livingFactions.Count < 2 || allAllied)
            {
                foreach(Entity playerEntity in targets) {
                    if(playerEntity as PlayerCore) {
                        if(livingFactions.Contains(playerEntity.faction)) {
                            AudioManager.PlayClipByID("clip_victory");
                            if(NodeEditorFramework.Standard.WinBattleCondition.OnBattleWin != null)
                                NodeEditorFramework.Standard.WinBattleCondition.OnBattleWin.Invoke(sectorName);
                        }
                        else AudioManager.PlayClipByID("clip_fail");
                    }
                }
                DialogueSystem.ShowBattleResults(livingFactions.Contains(PlayerCore.Instance.faction));
                playing = false;
            }
        }
    }

    public void AddTarget(Entity target)
    {
        if (targets == null)
            targets = new List<Entity>();
        if (!playing)
            targets.Clear();
        if (target)
            playing = true;
        if(!targets.Contains(target)) targets.Add(target);
    }

    public static Entity[] getTargets()
    {
        return targets != null ? targets.ToArray() : null;
    }
}
