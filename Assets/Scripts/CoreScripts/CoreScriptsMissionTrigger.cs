using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsSequence;
using static CoreScriptsManager;

public class CoreScriptsMissionTrigger : MonoBehaviour
{

    public static Context ParseMissionTrigger(int lineIndex, int charIndex,
         string[] lines, Dictionary<FileCoord, FileCoord> stringScopes, Dictionary<int, ConditionBlock> blocks, CoreScriptsManager traverser, out FileCoord coord)
    {
        var scope = CoreScriptsManager.GetScope(lineIndex, lines, stringScopes, out coord);
        return ParseMissionTriggerHelper(0, scope, traverser, blocks);
    }

    private static Context ParseMissionTriggerHelper(int index, string line, CoreScriptsManager traverser, Dictionary<int, ConditionBlock> blocks)
    {
        var trigger = new Context();
        trigger.type = TriggerType.Mission;
        trigger.traverser = traverser;
        trigger.prerequisites = new List<string>();
        List<string> stx = new List<string>()
        {
            "name=",
            "prerequisites=",
            "sequence="
        };
        bool skipToComma = false;
        int brax = 0;

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (lineSubstr.StartsWith("name="))
            {
                trigger.missionName = lineSubstr.Split(",")[0].Split("=")[1];
            }
            else if (lineSubstr.StartsWith("prerequisites="))
            {
                // TODO: this introduces a bug where missions cannot have commas in their names
                var scope = lineSubstr.Substring("prerequisites=".Length);
                scope = scope.Substring(scope.IndexOf("(")+1, scope.IndexOf(")") - scope.IndexOf("("));
                var ps = scope.Split(",");
                foreach (var p in ps)
                {
                    trigger.prerequisites.Add(p.Trim());
                }
            }
            else if (lineSubstr.StartsWith("sequence="))
            {
                trigger.sequence = CoreScriptsSequence.ParseSequence(i, line, blocks);
            }
        }
        return trigger;
    }
}
