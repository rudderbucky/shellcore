using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;
using static CoreScriptsSequence;

public class CoreScriptsTrigger : MonoBehaviour
{
    public static Context ParseTrigger(int lineIndex, int charIndex,
         string[] lines, CoreScriptsManager.TriggerType type, ScopeParseData data, out FileCoord coord)
    {
        var scope = CoreScriptsManager.GetScope(lineIndex, lines, data.stringScopes, data.commentLines, out coord);
        return ParseTriggerHelper(0, scope, type, data.blocks);
    }

    private static readonly List<string> requiredMissionTriggerArguments = new List<string>()
    {
        "name",
        "prerequisites",
        "entryPoint",
    };

        private static readonly List<string> requiredSectorTriggerArguments = new List<string>()
    {
        "sectorName"
    };

    private static Context ParseTriggerHelper(int index, string line, CoreScriptsManager.TriggerType type, Dictionary<int, ConditionBlock> blocks)
    {
        var trigger = new Context();
        trigger.prerequisites = new List<string>();
        trigger.type = type;
        bool skipToComma = true;
        int brax = 0;
        List<string> stx = null;
        var fakeArgString = "";

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (lineSubstr.StartsWith("sequence=")) 
            {
                trigger.sequence = ParseSequence(i, line, blocks);
                continue;
            }

            var key = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out key, out val, true);
            fakeArgString = AddArgument(fakeArgString, key, val);
            switch (key)
            {
                case "name":
                    trigger.missionName = val;
                    break;
                case "prerequisites":
                    var ps = val.Trim().Replace("(", "").Replace(")", "").Split(",");
                    foreach (var p in ps)
                    {
                        trigger.prerequisites.Add(p.Trim());
                    }
                    break;
                case "entryPoint":
                    trigger.entryPoint = val;
                    break;
                case "sectorName":
                    trigger.sectorName = val;
                    break;
            }
        }

        switch (trigger.type)
        {
            case TriggerType.Mission:
                AssertArgumentsPresent(fakeArgString, "MissionTrigger", requiredMissionTriggerArguments);
                break;
            case TriggerType.Sector:
                AssertArgumentsPresent(fakeArgString, "SectorTrigger", requiredSectorTriggerArguments);
                break;
            default:
                break;
        }
        return trigger;
    }


    /*
    private static Context ParseMissionTriggerHelper(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var trigger = new Context();
        trigger.type = TriggerType.Mission;
        trigger.prerequisites = new List<string>();
        List<string> stx = new List<string>()
        {
            "name=",
            "prerequisites=",
            "sequence=",
            "entryPoint="
        };
        bool skipToComma = false;
        int brax = 0;

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            var name = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out name, out val);
            if (lineSubstr.StartsWith("name="))
            {
                trigger.missionName = val;
            }
            else if (lineSubstr.StartsWith("prerequisites="))
            {
                var scope = lineSubstr.Substring("prerequisites=".Length);
                scope = scope.Substring(scope.IndexOf("(")+1, scope.IndexOf(")") - scope.IndexOf("("));
                var ps = scope.Split(",");
                foreach (var p in ps)
                {
                    trigger.prerequisites.Add(p.Trim());
                }
            }
            else if (lineSubstr.StartsWith("entryPoint="))
            {
                trigger.entryPoint = val;
            }
            else if (lineSubstr.StartsWith("sequence="))
            {
                trigger.sequence = CoreScriptsSequence.ParseSequence(i, line, blocks);
            }
        }
        return trigger;
    }
    */
}
