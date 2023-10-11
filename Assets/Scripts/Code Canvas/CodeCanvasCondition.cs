using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static CodeCanvasSequence;
using static CodeTraverser;
using static Entity;
public class CodeCanvasCondition : MonoBehaviour
{
    private static int blockID = 0;
    public enum ConditionType 
    {
        WinBattleZone,
        WinSiegeZone,
        DestroyEntities,
        Time,
        Status,
    }

    public struct Condition
    {
        public ConditionType type;
        public string arguments;
        public Sequence sequence;
        public string state;
    }

    public struct ConditionBlock
    {
        public List<Condition> conditions;
        public CodeTraverser traverser;
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
        Debug.LogWarning("Parse block called");
        var block = CreateConditionBlock();
        bool skipToComma = true;
        int brax = 0;
        List<string> stx = new List<string>()
        {
            "ConditionBlock"
        };

        int condIndex = 0;
        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i);
            Debug.LogWarning(lineSubstr);
            block.conditions.Add(ParseCondition(i, line, condIndex, blocks));
            condIndex++;
        }
        return block;
    }

    private static Condition ParseCondition(int index, string line, int condIndex, Dictionary<int, ConditionBlock> blocks)
    {
        Debug.LogWarning("Parse Cond called");
        var cond = new Condition();
        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "type=",
            "sectorName=",
            "lossMode=",
            "sequence=",
            "entityID=",
            "targetID=",
            "nameMode=",
            "targetFaction=",
            "targetCount=",
            "progressionFeedback=",
            "sequence=",
        };

        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i);
            if (lineSubstr.StartsWith("sequence=")) 
            {
                cond.sequence = ParseSequence(i, line, blocks);
                continue;
            }
            var val = lineSubstr.Split(",")[0].Split("=")[1];
            var key = lineSubstr.Split(",")[0].Split("=")[0];
            cond.arguments = AddArgument(cond.arguments, key, val);
            Debug.LogWarning(cond.arguments);

        }
        Enum.TryParse<ConditionType>(GetArgument(cond.arguments, "type"), out cond.type);
        Debug.LogWarning(cond.type);
        return cond;
    }


    public static void ExecuteConditionBlock(ConditionBlock block, Context context)
    {
        Debug.LogWarning("exec called");
        /*
        var cond = new Condition();
        cond.type = ConditionType.DestroyEntities;
        cond.arguments = AddArgument(cond.arguments, "targetID", "Strike Drone");
        cond.arguments = AddArgument(cond.arguments, "nameMode", "true");
        cond.arguments = AddArgument(cond.arguments, "targetFaction", 1+"");
        cond.arguments = AddArgument(cond.arguments, "targetCount", 3+"");
        cond.arguments = AddArgument(cond.arguments, "progressionFeedback", "true");
        cond.state = "0";
        block.conditions.Add(cond);
        */
        block.context = context;
        for (int i = 0; i < block.conditions.Count; i++)
        {
            var c = block.conditions[i];
            ExecuteCondition($"{block.ID}-{i}", c, block);
        }
    }

    private static void ExecuteCondition(string ID, Condition c, ConditionBlock cb)
    {
        switch (c.type)
        {
            case ConditionType.DestroyEntities:
                var nameMode = CodeCanvasSequence.GetArgument(c.arguments, "nameMode") == "true";
                var progressionFeedback = CodeCanvasSequence.GetArgument(c.arguments, "progressionFeedback") == "true";
                var targetID = CodeCanvasSequence.GetArgument(c.arguments, "targetID");
                var targetFaction = int.Parse(CodeCanvasSequence.GetArgument(c.arguments, "targetFaction"));
                var targetCount = int.Parse(CodeCanvasSequence.GetArgument(c.arguments, "targetCount"));
                int killCount = 0;
                EntityDeathDelegate act = (e, _) => 
                {
                    killCount = EntityCheck(ID, e, c, cb, nameMode, progressionFeedback, targetID, targetFaction, targetCount, killCount);
                };

                cb.traverser.entityDeathDelegates.Add(ID, act);
                Entity.OnEntityDeath += act;
                break;

        }
    }

    private static int EntityCheck(string ID, Entity entity, Condition c, ConditionBlock cb,
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
                SatisfyCondition(ID, c, cb);
            }
        }
        return killCount;
    }



    private static void SatisfyCondition(string ID, Condition cond, ConditionBlock cb)
    {
        foreach (var c in cb.conditions)
        {
            DeinitializeCondition(ID, cb, c);
        }

        if (cond.sequence.instructions != null)
            CodeCanvasSequence.RunSequence(cond.sequence, cb.traverser, cb.context);
    }

    private static void DeinitializeCondition(string ID, ConditionBlock cb, Condition cond)
    {
        switch(cond.type)
        {
            case ConditionType.DestroyEntities:
                Entity.OnEntityDeath -= cb.traverser.entityDeathDelegates[ID];
                break;
            case ConditionType.WinBattleZone:
            case ConditionType.WinSiegeZone:
            default:
                return;
        }
    }
}
