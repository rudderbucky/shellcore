using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsManager;

public class CoreScriptsTask : MonoBehaviour
{
    public static Task ParseTask(int lineIndex, int charIndex,
         string[] lines, ScopeParseData data, out FileCoord coord)
    {
        return ParseTaskHelper(0, CoreScriptsManager.GetScope(lineIndex, lines, data.stringScopes, data.commentLines, out coord), data.localMap);
    }

    private static Task ParseTaskHelper(int index, string line, Dictionary<string, string> localMap)
    {
        List<string> stx = new List<string>()
        {
            "taskID=",
            "objectives=",
            "creditReward=",
            "reputationReward=",
            "shardReward=",
            "partID=",
            "abilityID=",
            "tier=",
        };
        bool skipToComma = false;
        int brax = 0;

        var task = new Task();

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            var name = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out name, out val);

            switch (name)
            {
                case "taskID":
                    task.taskID = val;
                    break;
                case "objectives":
                    task.objectived = localMap[val];
                    break;
                case "creditReward":
                    task.creditReward = int.Parse(val);
                    break;
                case "reputationReward":
                    task.reputationReward = int.Parse(val);
                    break;
                case "shardReward":
                    task.shardReward = int.Parse(val);
                    break;
                case "partID":
                    task.partReward.partID = val;
                    break;
                case "abilityID":
                    task.partReward.abilityID = int.Parse(val);
                    break;
                case "tier":
                    task.partReward.tier = int.Parse(val);
                    break;
            }
        }
        return task;
    }
}
