using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeCanvasSequence : MonoBehaviour
{
    public enum InstructionCommand
    {
        SetInteraction,
        StartCutscene,
        EndCutscene,
        Log
    }

    public enum InteractionType
    {
        Dialogue,
        OpenShipBuilder,
        OpenTrader,
        OpenDroneWorkshop,
        OpenSkirmishMenu
    }

    public struct Instruction
    {
        public InstructionCommand command;
        public string arguments;
        public string GetArgument(string arg)
        {
            switch (arg)
            {
                case "entityID":
                    return "test";
                case "interactionType":
                    return "Dialogue";
                case "dialogueID":
                    return "dial";
            }
            return null;
        }
    }


    public struct Sequence
    {
        public List<Instruction> instructions;
    }

    private struct MissionTrigger
    {
        public string name;
        public string rank;
        public List<string> prerequisites;
        public Sequence sequence;
    }

    public static void RunSequence (Sequence seq, CodeTraverser traverser)
    {
        foreach (var inst in seq.instructions)
        {
            switch (inst.command)
            {
                case InstructionCommand.SetInteraction:
                    InteractAction action = new InteractAction();
                    var entityID = inst.GetArgument("entityID");
                    action.action = new UnityEngine.Events.UnityAction(() =>
                        {
                            DialogueSystem.Instance.SetSpeakerID(entityID);
                            DialogueSystem.StartDialogue(traverser.dialogues["dial"]);
                        });

                    DialogueSystem.Instance.PushInteractionOverrides(entityID, action, null);


                    break;
            }
        }
    }

    public static Sequence ParseSequence(int index, string line)
    {
        // find the first bracket
        while (index < line.Length && line[index] != '(') index++;
        index++;
        int brackets = 1;
        var seq = new Sequence();
        seq.instructions = new List<Instruction>();

        while (index < line.Length && brackets > 0)
        {
            if (line[index] == '(')
            {
                brackets++;
            }
            else if (line[index] == ')')
            {
                brackets--;
            }

            var lineSubstr = line.Substring(index);
            var nextComma = false;
            if (lineSubstr.StartsWith("SetInteraction"))
            {
                var inst = new Instruction();
                inst.command = InstructionCommand.SetInteraction;
                seq.instructions.Add(inst);
            }
            if (nextComma) 
            {
                nextComma = false;
                index += lineSubstr.IndexOf(",");
            }
            index++;
        }

        return seq;
    }
}
