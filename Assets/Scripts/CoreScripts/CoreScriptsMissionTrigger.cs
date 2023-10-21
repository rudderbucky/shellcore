using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;

public class CoreScriptsMissionTrigger : MonoBehaviour
{
    public static Context ParseMissionTrigger(int lineIndex, int charIndex,
         string[] lines, ScopeParseData data, out FileCoord coord)
    {
        var scope = CoreScriptsManager.GetScope(lineIndex, lines, data.stringScopes, data.commentLines, out coord);
        return ParseMissionTriggerHelper(0, scope, data.blocks);
    }

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
}
