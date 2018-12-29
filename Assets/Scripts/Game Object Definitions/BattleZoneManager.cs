using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleZoneManager : MonoBehaviour
{
    List<Entity> targets;
    bool playing;

	public void AddTarget(Entity target)
    {
        if (targets == null)
            targets = new List<Entity>();
        if (!playing)
            targets.Clear();
        if (target)
            playing = true;
        targets.Add(target);
    }

	void Update () //TODO: entity death event, not update
    {
        if(playing)
        {
            Dictionary<int, int> alive = new Dictionary<int, int>();
            for (int i = 0; i < targets.Count; i++)
            {
                if (!alive.ContainsKey(targets[i].faction))
                    alive.Add(targets[i].faction, 0);
                if(targets[i] && !targets[i].GetIsDead())
                    alive[targets[i].faction]++;
            }
            int factionCount = 0;
            foreach(var pair in alive)
            {
                if (pair.Value > 0)
                    factionCount++;
            }
            if(factionCount < 2)
            {
                int winningFaction = -1;
                foreach (var pair in alive)
                {
                    if (pair.Value > 0)
                        winningFaction = pair.Key;
                }

                DialogueSystem.ShowPopup("Faction " + winningFaction + " won!");
                playing = false;
            }
        }
	}
}
