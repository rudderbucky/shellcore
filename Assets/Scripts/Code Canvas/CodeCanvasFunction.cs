using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CodeCanvasCondition;
using static CodeCanvasSequence;
using static CodeTraverser;

public class CodeCanvasFunction : MonoBehaviour
{
    public struct Function
    {
        public string name;
        public Sequence sequence;
    }
    public static Function ParseFunction(int lineIndex, int charIndex,
         string[] lines, Dictionary<FileCoord, FileCoord> stringScopes, Dictionary<int, ConditionBlock> blocks, out FileCoord coord)
    {
        return ParseFunctionHelper(0, CodeTraverser.GetScope(lineIndex, lines, stringScopes, out coord), blocks);
    }

    private static Function ParseFunctionHelper(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var func = new Function();
        func.sequence = new Sequence();
        func.sequence.instructions = new List<Instruction>();


        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "name=",
            "sequence="
        };

        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i);
            if (lineSubstr.StartsWith("sequence="))
            {
                func.sequence = CodeCanvasSequence.ParseSequence(i, line, blocks);
                continue;
            }

            var name = "";
            var val = "";
            CodeCanvasSequence.GetNameAndValue(lineSubstr, out name, out val);

            if (lineSubstr.StartsWith("name="))
            {
                func.name = val;
            }
        }

        return func;
    }
}
