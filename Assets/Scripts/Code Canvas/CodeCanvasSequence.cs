using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CodeCanvasCondition;
using static CodeTraverser;

public class CodeCanvasSequence : MonoBehaviour
{
    public enum InstructionCommand
    {
        SetInteraction,
        StartCutscene,
        EndCutscene,
        Log,
        Call,
        ConditionBlock,
        SpawnEntity
    }
    public struct Instruction
    {
        public InstructionCommand command;
        public string arguments;
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

    public static string GetArgument(string arguments, string key)
    {
        var args = arguments.Split(";");
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == key) 
            {
                return args[i+1];
            }
        }
        return null;
    }

    // TODO: escape semicolons
    public static string AddArgument(string arguments, string key, string value)
    {
        if (string.IsNullOrEmpty(arguments)) arguments = $"{key};{value}";
        else arguments += $";{key};{value}";
        return arguments;
    }

    public static void RunSequence (Sequence seq, Context context)
    {
        var traverser = context.traverser;
        foreach (var inst in seq.instructions)
        {
            switch (inst.command)
            {
                case InstructionCommand.SetInteraction:
                    InteractAction action = new InteractAction();
                    var entityID = GetArgument(inst.arguments, "entityID");
                    action.action = new UnityEngine.Events.UnityAction(() =>
                        {
                            switch (context.type)
                            {
                                case TriggerType.Mission:
                                    TaskManager.Instance.SetSpeakerID(entityID);
                                    break;
                                default:
                                    DialogueSystem.Instance.SetSpeakerID(entityID);
                                    break;
                            }
                            
                            DialogueSystem.StartDialogue(traverser.dialogues[GetArgument(inst.arguments, "dialogueID")], null, context);
                        });

                    switch (context.type)
                    {
                        case TriggerType.Mission:
                            TaskManager.Instance.PushInteractionOverrides(entityID, action, null, context);
                            break;
                        default:
                            DialogueSystem.Instance.PushInteractionOverrides(entityID, action, null);
                            break;
                    }
                    break;
                case InstructionCommand.Call:
                    var s = traverser.GetFunction(GetArgument(inst.arguments, "name"));
                    RunSequence(s, context);
                    break;
                case InstructionCommand.ConditionBlock:
                    var cb = traverser.conditionBlocks[int.Parse(GetArgument(inst.arguments, "ID"))];
                    cb.traverser = traverser;
                    CodeCanvasCondition.ExecuteConditionBlock(cb, context);
                    break;
                case InstructionCommand.SpawnEntity:
                    SpawnEntity(
                        GetArgument(inst.arguments, "entityID"), 
                        GetArgument(inst.arguments, "forceCharacterTeleport") != "false",
                        GetArgument(inst.arguments, "flagName"),
                        GetArgument(inst.arguments, "blueprintJSON"),
                        int.Parse(GetArgument(inst.arguments, "faction")),
                        GetArgument(inst.arguments, "name"));
                    break;

            }
        }
    }

    private static void SpawnEntity(string entityID, bool forceCharacterTeleport, string flagName, string blueprintJSON, int faction, string name)
    {
        Vector2 coords = new Vector2();
        for (int i = 0; i < AIData.flags.Count; i++)
        {
            if (AIData.flags[i].name == flagName)
            {
                coords = AIData.flags[i].transform.position;
                break;
            }
        }

        foreach (var data in SectorManager.instance.characters)
        {
            if (data.ID != entityID) continue;
            Debug.Log("Spawn Entity ID given matches with a character name! Spawning character...");

            foreach (var oj in AIData.entities)
            {
                if (oj && oj.ID == data.ID)
                {
                    Debug.Log("Character already found. Not spawning.");
                    if (forceCharacterTeleport)
                    {
                        if (!(oj is AirCraft airCraft)) continue;
                        airCraft.Warp(coords, false);
                    }

                    return;
                }
            }

            var characterBlueprint = SectorManager.TryGettingEntityBlueprint(data.blueprintJSON);
            Sector.LevelEntity entityData = new Sector.LevelEntity
            {
                faction = data.faction,
                name = data.name,
                position = coords,
                ID = data.ID
            };
            SectorManager.instance.SpawnEntity(characterBlueprint, entityData);
            return;
        }


        Debug.Log($"Spawn Entity ID ( {entityID} ) does not correspond with a character. Performing normal operations.");
        EntityBlueprint blueprint = SectorManager.TryGettingEntityBlueprint(blueprintJSON);

            if (blueprint)
            {
                Sector.LevelEntity entityData = new Sector.LevelEntity
                {
                    faction = faction,
                    name = name,
                    position = coords,
                    ID = entityID
                };
                var entity = SectorManager.instance.SpawnEntity(blueprint, entityData);
                if (DevConsoleScript.fullLog)
                {
                    Debug.Log(entity.transform.position + " " + entity.spawnPoint);
                }

                entity.name = name;
            }
            else
            {
                Debug.LogWarning("Blueprint not found!");
            }
    }


    public static Sequence ParseSequence(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var seq = new Sequence();
        seq.instructions = new List<Instruction>();

        List<string> stx = null;
        bool skipToComma = false;
        int brax = 0;

        List<string> standardInstructions = new List<string>() 
        {
            "SetInteraction",
            "SpawnEntity",
        };
        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (standardInstructions.Exists(s => lineSubstr.StartsWith(s)))
            {
                seq.instructions.Add(ParseInstruction(i, line));
            }
            else if (lineSubstr.StartsWith("Call"))
            {
                var funcName = lineSubstr.Substring(5);
                funcName = funcName.Substring(0, funcName.IndexOf(")"));
                var inst = new Instruction();
                inst.arguments = AddArgument(inst.arguments, "name", funcName);
                inst.command = InstructionCommand.Call;
                seq.instructions.Add(inst);
                // TODO: Function call recursion
            }
            else if (lineSubstr.StartsWith("ConditionBlock"))
            {
                var b = CodeCanvasCondition.ParseConditionBlock(i, line, blocks);
                blocks.Add(b.ID, b);
                var inst = new Instruction();
                inst.command = InstructionCommand.ConditionBlock;
                inst.arguments = AddArgument(inst.arguments, "ID", b.ID+"");
                seq.instructions.Add(inst);
            }
        }

        return seq;
    }

    // TODO: Move condition type to be declaration instead of type= style element
    private static Instruction ParseInstruction(int index, string line)
    {
        var substr = line.Substring(index).Split("(")[0].Trim();
        var inst = new Instruction();
        Enum.TryParse<InstructionCommand>(substr, out inst.command);
        inst.arguments = "";
        bool skipToComma = true;
        List<string> stx = null;
        int brax = 0;

        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (lineSubstr.Split(",").Length > 0 && lineSubstr.Split(",")[0].Split("=").Length > 1)
            {
                var val = lineSubstr.Split(",")[0].Split("=")[1];
                var key = lineSubstr.Split(",")[0].Split("=")[0];
                inst.arguments = AddArgument(inst.arguments, key, val);
            }
        }

        return inst;
    }
}
