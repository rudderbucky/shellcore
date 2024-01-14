using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsSequence;
using static CoreScriptsManager;

public class CoreScriptsFunction : MonoBehaviour
{
    public struct Function
    {
        public string name;
        public Sequence sequence;
    }

    public static Function ParseFunction(int lineIndex, int charIndex,
        string[] lines, ScopeParseData data, out FileCoord coord)
    {
        return ParseFunctionHelper(0, CoreScriptsManager.GetScope(lineIndex, lines, data.stringScopes, data.commentLines, out coord), data.blocks);
    }

    private static Function ParseFunctionHelper(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var func = new Function();
        func.sequence = new Sequence();
        func.sequence.instructions = new List<Instruction>();

        index = GetIndexAfter(line, "Function(");
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line))
        {
            var lineSubstr = line.Substring(i).Trim();
            if (lineSubstr.StartsWith("sequence="))
            {
                func.sequence = CoreScriptsSequence.ParseSequence(i, line, blocks);
                continue;
            }

            var name = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out name, out val);

            if (lineSubstr.StartsWith("name="))
            {
                func.name = val;
            }
        }

        return func;
    }
}
