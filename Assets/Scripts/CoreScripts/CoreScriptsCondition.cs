using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static CoreScriptsSequence;
using static CoreScriptsManager;
using static Entity;
using static SectorManager;

public class CoreScriptsCondition : MonoBehaviour
{
    private static int blockID = 0;
    public enum ConditionType 
    {
        WinBattleZone,
        WinSiegeZone,
        DestroyEntities,
        Time,
        Status,
        EnterSector,
        Comparison
    }

    public struct Condition
    {
        public ConditionType type;
        public string arguments;
        public Sequence sequence;
        public int ID;
    }

    public struct ConditionBlock
    {
        public List<Condition> conditions;
        public Context context;
        public int ID;
    }

    public static ConditionBlock CreateConditionBlock()
    {
        var block = new ConditionBlock();
        block.conditions = new List<Condition>();
        block.ID = blockID;
        blockID++;
        return block;
    }

    public static ConditionBlock ParseConditionBlock(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var block = CreateConditionBlock();
        bool skipToComma = true;
        int brax = 0;
        List<string> stx = new List<string>()
        {
            "ConditionBlock"
        };

        int condIndex = 0;
        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            block.conditions.Add(ParseCondition(i, line, condIndex, blocks));
            condIndex++;
        }
        return block;
    }

    private static Condition ParseCondition(int index, string line, int condIndex, Dictionary<int, ConditionBlock> blocks)
    {
        var cond = new Condition();
        bool skipToComma = false;
        int brax = 0;
        List<string> stx = null;
        skipToComma = true;

        var substr = line.Substring(index).Split("(")[0].Trim();
        
        Enum.TryParse<ConditionType>(substr, out cond.type);
        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (lineSubstr.StartsWith("sequence=")) 
            {
                cond.sequence = ParseSequence(i, line, blocks);
                continue;
            }

            var key = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out key, out val);
            cond.arguments = AddArgument(cond.arguments, key, val);

        }

        return cond;
    }


    public static void ExecuteConditionBlock(ConditionBlock block, Context context)
    {
        block.context = context;
        for (int i = 0; i < block.conditions.Count; i++)
        {
            var c = block.conditions[i];
            c.ID = i;
            ExecuteCondition(c, block);
        }
    }



    private static void ExecuteCondition(Condition c, ConditionBlock cb)
    {
        var ID = $"{cb.ID}-{c.ID}";
        switch (c.type)
        {
            case ConditionType.Comparison:
                var val1 = CoreScriptsSequence.GetArgument(c.arguments, "val1", true);
                var val2 = CoreScriptsSequence.GetArgument(c.arguments, "val2", true);
                var comp = CoreScriptsSequence.GetArgument(c.arguments, "comp");
                Variable.CompareVals(val1, val2, comp, c, cb);
                break;
            case ConditionType.Time:
                var time = float.Parse(CoreScriptsSequence.GetArgument(c.arguments, "time"));
                var timer = TaskManager.Instance.StartCoroutine(Timer(time, cb, c));
                CoreScriptsManager.instance.timerCoroutines.Add(ID, timer);
                break;
            case ConditionType.DestroyEntities:
                var nameMode = CoreScriptsSequence.GetArgument(c.arguments, "nameMode") == "true";
                var progressionFeedback = CoreScriptsSequence.GetArgument(c.arguments, "progressionFeedback") == "true";
                var targetID = CoreScriptsSequence.GetArgument(c.arguments, "targetID");
                var targetFaction = int.Parse(CoreScriptsSequence.GetArgument(c.arguments, "targetFaction"));
                var targetCount = int.Parse(CoreScriptsSequence.GetArgument(c.arguments, "targetCount"));
                int killCount = 0;
                EntityDeathDelegate act = (e, _) => 
                {
                    killCount = EntityCheck(e, c, cb, nameMode, progressionFeedback, targetID, targetFaction, targetCount, killCount);
                };

                CoreScriptsManager.instance.entityDeathDelegates.Add(ID, act);
                Entity.OnEntityDeath += act;
                break;

            case ConditionType.EnterSector:
                var sectorName = CoreScriptsSequence.GetArgument(c.arguments, "sectorName");
                var invert = CoreScriptsSequence.GetArgument(c.arguments, "invert") == "true";

                SectorLoadDelegate sectorAct = (sector) => 
                {
                    SectorCheck(sector, sectorName, c, cb, invert);
                };
                CoreScriptsManager.instance.sectorLoadDelegates.Add(ID, sectorAct);
                SectorManager.OnSectorLoad += sectorAct;
                break;
        }
    }

    private static IEnumerator Timer(float delay, ConditionBlock cb, Condition c)
    {
        yield return new WaitForSeconds(delay);
        SatisfyCondition(c, cb);
    }

    private static void SectorCheck(string sector, string selectedSectorName, Condition c, ConditionBlock cb, bool invertMode)
    {
        var activate = (sector == selectedSectorName) == !invertMode;
        if (activate) SatisfyCondition(c, cb);
    }

    private static int EntityCheck(Entity entity, Condition c, ConditionBlock cb,
        bool nameMode, bool progressionFeedback, string targetID, int targetFaction, int targetCount, int killCount)
    {

        if (((!nameMode && entity.ID == targetID) || (nameMode && (entity.entityName == targetID || entity.name == targetID)))
            && entity.faction == targetFaction)
        {
            killCount++;

            if (progressionFeedback)
            {
                if (!FactionManager.IsAllied(0, targetFaction))
                {
                    SectorManager.instance.player.alerter.showMessage($"ENEMIES DESTROYED: {killCount} / {targetCount}", "clip_victory");
                }
                else
                {
                    SectorManager.instance.player.alerter.showMessage($"ALLIES DEAD: {killCount} / {targetCount}", "clip_alert");
                }
            }

            if (killCount == targetCount)
            {
                SatisfyCondition(c, cb);
            }
        }
        return killCount;
    }



    public static void SatisfyCondition(Condition cond, ConditionBlock cb)
    {
        foreach (var c in cb.conditions)
        {
            DeinitializeCondition(cb, c);
        }

        if (cond.sequence.instructions != null)
            CoreScriptsSequence.RunSequence(cond.sequence, cb.context);
    }

    private static void DeinitializeCondition(ConditionBlock cb, Condition cond)
    {
        Debug.LogWarning(cond.ID);
        var ID = $"{cb.ID}-{cond.ID}";
        switch(cond.type)
        {
            case ConditionType.Time:
                var coroutine = CoreScriptsManager.instance.timerCoroutines[ID];
                TaskManager.Instance.StopCoroutine(coroutine);
                break;
            case ConditionType.DestroyEntities:
                Entity.OnEntityDeath -= CoreScriptsManager.instance.entityDeathDelegates[ID];
                break;
            case ConditionType.WinBattleZone:
                break;
            case ConditionType.WinSiegeZone:
                break;
            case ConditionType.EnterSector:
                SectorManager.OnSectorLoad -= CoreScriptsManager.instance.sectorLoadDelegates[ID];
                break;
            case ConditionType.Comparison:
                CoreScriptsManager.OnVariableUpdate -= CoreScriptsManager.instance.variableChangedDelegates[ID];
                break;
            default:
                return;
        }
    }
}
