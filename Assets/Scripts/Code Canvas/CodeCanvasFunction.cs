using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
         string[] lines, Dictionary<FileCoord, FileCoord> stringScopes, out FileCoord coord)
    {
        var func = new Function();
        func.name = "fortnite pro gamer 69";
        func.sequence = new Sequence();
        func.sequence.instructions = new List<Instruction>();
        var inst = new Instruction();
        inst.command = InstructionCommand.SetInteraction;
        inst.arguments = "";
        func.sequence.instructions.Add(inst);
        CodeTraverser.GetScope(lineIndex, lines, stringScopes, out coord);
        return func;
    }

    private static Function ParseFunctionHelper(int index, string line)
    {
        return new Function();
    }
}
