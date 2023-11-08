using System;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;
using System.Linq;
using System.Collections;
using NodeEditorFramework.Standard;
public class CoreScriptsSequence : MonoBehaviour
{
    public enum InstructionCommand
    {
        SetInteraction,
        ClearInteraction,
        Log,
        Call,
        ConditionBlock,
        SpawnEntity,
        SetVariable,
        AddIntValues,
        ConcatenateValues,
        StartCutscene,
        FinishCutscene,
        SetPath,
        Rotate,
        PassiveDialogue,
        ShowAlert,
        AddObjectiveMarker,
        RemoveObjectiveMarker,
        WarpPlayer,
        StartCameraPan,
        FinishCameraPan,
        RegisterPartyMember,
        AddPartyMember,
        EnablePartyMember,
        DisablePartyMember,
        RemovePartyMember,
        ClearParty,
        ForceTractor,
        FinishMission,
        SetFlagInteractibility,
        FinishTask,
        FailTask,
        ForceStartDialogue,
        FollowEntity,
        Wait,
        DevConsole,
        FadeIntoBlack,
        FadeOutOfBlack,
        DeleteEntity,
        StartInflictionCosmetic,
        FinishInflictionCosmetic,
        AddTask,
        LockParty,
        SetSectorType,
        ForceSpawnPoint,
        UnlockParty,
        EnableDeadZoneDamage,
        DisableDeadZoneDamage,
        ShowPopup,
        StartMusicOverride,
        FinishMusicOverride
    }
    public struct Instruction
    {
        public InstructionCommand command;
        public string arguments;
        public Sequence sequence;
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

    private Dictionary<InstructionCommand, List<string>> requiredArguments = new Dictionary<InstructionCommand, List<string>>()
    {
        
    };

    private static readonly List<char> specialChars = new List<char>()
    {
        ';',
        '?',
        ':',
        '+',
        '=',
        '{',
        '}',
        '[',
        ']',
        '|',
        '\\',
        '/',
        '*',
        '&',
        '^',
        '%',
        '#',
        '@',
        '!',
        '`',
        '~',
    };


    // TODO: Read out of the global variables array for world base properties
    public static string GetArgument(string arguments, string key, bool rawValue = false)
    {
        if (specialChars.Exists(e => key.Contains(e)))
        {
            throw new System.Exception("Argument values cannot have special characters except for underscores in them.");
        }
        var args = arguments.Split(";");
        string retVal = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != key) continue;

            var val = args[i+1];
            if (rawValue)
            {
                retVal = val;
                break;
            } 
            else retVal = VariableSensitizeValue(val);
        }
        if (retVal != null) retVal = retVal.Trim();
        return retVal;
    }

    public static string VariableSensitizeValue(string val)
    {
        if (string.IsNullOrEmpty(val)) return val;
        string retVal = null;
        if (val.StartsWith("$$$") && 
            SaveHandler.instance.GetSave().coreScriptsGlobalVarNames != null && 
            SaveHandler.instance.GetSave().coreScriptsGlobalVarNames.Contains(val.Substring(3).Trim()))
        {
            var index = SaveHandler.instance.GetSave().coreScriptsGlobalVarNames.IndexOf(val.Substring(3).Trim());
            if (index >= 0) 
            {
                retVal = SaveHandler.instance.GetSave().coreScriptsGlobalVarValues[index];
            }
        }
        else if (val.StartsWith("$$$") && SaveHandler.instance.GetSave().taskVariableNames != null && 
            SaveHandler.instance.GetSave().taskVariableNames.Contains(val.Substring(3).Trim()))
        {
            var index = -1;
            var names = SaveHandler.instance.GetSave().taskVariableNames;
            var key = val.Substring(3).Trim();
            for (int j = 0; j < names.Length; j++)
            {
                if (names[j] != val.Substring(3).Trim()) continue;
                index = j;
                break;
            }
            
            if (index >= 0)
            {
                retVal = SaveHandler.instance.GetSave().taskVariableValues[index].ToString();
            }
        }
        else if (val.StartsWith("$$") && !val.StartsWith("$$$") && CoreScriptsManager.instance.globalVariables != null)
        {
            var key = val.Substring(2).Trim();
            if (CoreScriptsManager.instance.globalVariables.ContainsKey(key))
                retVal = CoreScriptsManager.instance.globalVariables[key];
            else 
            {
                Debug.LogWarning($"Variable syntax was used but variable was not found: {val}");
                return "";
            }
        }
        else 
        {
            retVal = val;
        }
        return retVal;
    }

    public static string AddArgument(string arguments, string key, string value)
    {
        if (specialChars.Exists(e => key.Contains(e)) || specialChars.Exists(e => value.Contains(e)))
        {
            throw new System.Exception("Argument values cannot have semicolons or commas in them.");
        }
        if (string.IsNullOrEmpty(arguments)) arguments = $"{key};{value}";
        else arguments += $";{key};{value}";
        return arguments;
    }

    public static void RunSequence (Sequence seq, Context context)
    {
        CoreScriptsManager.instance.StartCoroutine(RunSequenceHelper(seq, context));
    }


    public static IEnumerator RunSequenceHelper (Sequence seq, Context context)
    {
        var traverser = CoreScriptsManager.instance;
        foreach (var inst in seq.instructions)
        {
            switch (inst.command)
            {
                case InstructionCommand.StartMusicOverride:
                    var musicID = GetArgument(inst.arguments, "musicID");
                    AudioManager.musicOverrideID = musicID;
                    AudioManager.PlayMusic(musicID);
                    break;
                case InstructionCommand.FinishMusicOverride:
                    AudioManager.musicOverrideID = null;
                    if (!SectorManager.instance.current.hasMusic || SectorManager.instance.current.musicID == "")
                    {
                        Debug.Log("Jazz music stops.");
                        AudioManager.StopMusic();
                    }
                    else
                    {
                        AudioManager.PlayMusic(SectorManager.instance.current.musicID);
                    }
                    break;
                case InstructionCommand.ShowPopup:
                    DialogueSystem.ShowPopup(CoreScriptsManager.instance.GetLocalMapString(GetArgument(inst.arguments, "text")));
                    break;
                case InstructionCommand.EnableDeadZoneDamage:
                    SectorManager.instance.SetDeadZoneDamageOverride(true);
                    break;
                case InstructionCommand.DisableDeadZoneDamage:
                    SectorManager.instance.SetDeadZoneDamageOverride(false);
                    break;
                case InstructionCommand.ForceSpawnPoint:
                    var sector = SectorManager.GetSectorByName(GetArgument(inst.arguments, "sectorName"));
                    var player = PlayerCore.Instance;
                    player.havenSpawnPoint = player.spawnPoint = SectorManager.GetSectorCenter(sector);
                    player.LastDimension = sector.dimension;
                    break;
                case InstructionCommand.SetSectorType:
                    int sectorType = (int)Enum.Parse<Sector.SectorType>(GetArgument(inst.arguments, "type"));
                    SectorManager.instance.overrideProperties.type = (Sector.SectorType)sectorType;
                    SectorManager.instance.SetSectorTypeBehavior();
                    string[] types = System.Enum.GetNames(typeof(Sector.SectorType));
                    Debug.Log($"Sector type set to: {types[sectorType]} ({sectorType})");
                    break;
                case InstructionCommand.LockParty:
                    PartyManager.instance.SetOverrideLock(true);
                    break;
                case InstructionCommand.UnlockParty:
                    PartyManager.instance.SetOverrideLock(false);
                    break;
                case InstructionCommand.AddTask:
                    StartTaskNode.RegisterTask(CoreScriptsManager.instance.GetTask(GetArgument(inst.arguments, "taskID")), context.missionName);
                    break;
                case InstructionCommand.AddObjectiveMarker: 
                    var sectorName = GetArgument(inst.arguments, "sectorName");
                    var missionName = context.missionName;
                    var entityID = GetArgument(inst.arguments, "entityID");
                    var flagName = GetArgument(inst.arguments, "flagName");
                    var ID = GetArgument(inst.arguments, "ID");
                    ObjectiveMarker.AddObjectiveMarker(entityID, sectorName, missionName, flagName, ID);
                    break;
                case InstructionCommand.RemoveObjectiveMarker:
                    ID = GetArgument(inst.arguments, "ID");
                    ObjectiveMarker.RemoveObjectiveMarker(ID);
                    break;
                case InstructionCommand.SetInteraction:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var dialogueID = GetArgument(inst.arguments, "dialogueID");
                    Interaction.SetInteraction(context, entityID, dialogueID);
                    break;
                case InstructionCommand.ForceStartDialogue:
                    dialogueID = GetArgument(inst.arguments, "dialogueID");
                    Interaction.StartDialogue(context, dialogueID);
                    break;
                case InstructionCommand.ClearInteraction:
                    entityID = GetArgument(inst.arguments, "entityID");
                    Interaction.ClearInteraction(context, entityID);
                    break;
                case InstructionCommand.Call:
                    var s = traverser.GetFunction(GetArgument(inst.arguments, "name"));
                    RunSequence(s, context);
                    break;
                case InstructionCommand.ConditionBlock:
                    var cb = traverser.conditionBlocks[int.Parse(GetArgument(inst.arguments, "ID"))];
                    CoreScriptsCondition.ExecuteConditionBlock(cb, context);
                    break;
                case InstructionCommand.SpawnEntity:
                    var fStr = GetArgument(inst.arguments, "faction");
                    var fac = 0;
                    if (!string.IsNullOrEmpty(fStr)) fac = int.Parse(fStr);
                    SpawnEntity(
                        GetArgument(inst.arguments, "entityID"), 
                        GetArgument(inst.arguments, "forceCharacterTeleport") != "false",
                        GetArgument(inst.arguments, "flagName"),
                        GetArgument(inst.arguments, "blueprintJSON"),
                        fac,
                        GetArgument(inst.arguments, "name"),
                        GetArgument(inst.arguments, "assetID"));
                    break;
                case InstructionCommand.Log:
                    Debug.LogWarning(GetArgument(inst.arguments, "message"));
                    break;
                case InstructionCommand.SetVariable:
                    var variableName = GetArgument(inst.arguments, "name", true);
                    var variableValue = GetArgument(inst.arguments, "value");
                    Variable.SetVariable(variableName, variableValue);
                    break;
                case InstructionCommand.AddIntValues:
                    var val1 = int.Parse(GetArgument(inst.arguments, "val1"));
                    var val2 = int.Parse(GetArgument(inst.arguments, "val2"));
                    var name = GetArgument(inst.arguments, "name", true);
                    Variable.SetVariable(name, val1+val2+"");
                    break;
                case InstructionCommand.ConcatenateValues:
                    var v1 = GetArgument(inst.arguments, "val1");
                    var v2 = GetArgument(inst.arguments, "val2");
                    var varName = GetArgument(inst.arguments, "name", true);
                    Variable.SetVariable(varName, v1+v2);
                    break;
                case InstructionCommand.SetPath:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var rotateWhileMoving = GetArgument(inst.arguments, "rotateWhileMoving") != "false";
                    var customMass = GetArgument(inst.arguments, "customMass") == null ? -1 : float.Parse(GetArgument(inst.arguments, "customMass"));
                    flagName = GetArgument(inst.arguments, "flagName");

                    Mobility.SetPath(entityID, rotateWhileMoving, customMass, flagName, inst.sequence, context);
                    break;
                case InstructionCommand.Rotate:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var targetID = GetArgument(inst.arguments, "targetID");
                    var angle = GetArgument(inst.arguments, "angle");

                    Mobility.Rotate(entityID, targetID, angle, inst.sequence, context);
                    break;
                case InstructionCommand.StartCutscene:
                    Cutscene.StartCutscene();
                    break;
                case InstructionCommand.FinishCutscene:
                    Cutscene.FinishCutscene();
                    break;
                case InstructionCommand.PassiveDialogue:
                    entityID = GetArgument(inst.arguments, "entityID");
                    var text = CoreScriptsManager.instance.GetLocalMapString(GetArgument(inst.arguments, "text"));
                    var soundType = GetArgument(inst.arguments, "soundType");
                    var onlyShowIfInParty = GetArgument(inst.arguments, "onlyShowIfInParty") == "true";
                    var useEntityColor = GetArgument(inst.arguments, "useEntityColor") != "false"; 
                    Interaction.PassiveDialogue(entityID, text, soundType, onlyShowIfInParty, useEntityColor);
                    break;
                case InstructionCommand.ShowAlert:
                    text = CoreScriptsManager.instance.GetLocalMapString(GetArgument(inst.arguments, "text"));
                    var soundID = GetArgument(inst.arguments, "soundID");
                    Interaction.ShowAlert(text, soundID);
                    break;
                case InstructionCommand.WarpPlayer:
                    Mobility.WarpPlayer(GetArgument(inst.arguments, "sectorName"), 
                        GetArgument(inst.arguments, "entityID"),
                        GetArgument(inst.arguments, "flagName"));
                    break;
                case InstructionCommand.StartCameraPan:
                    flagName = GetArgument(inst.arguments, "flagName");
                    var velocityFactor = GetArgument(inst.arguments, "velocityFactor") == null ? 1 : float.Parse(GetArgument(inst.arguments, "velocityFactor"));
                    Cutscene.StartCameraPan(Vector3.zero, false, flagName, velocityFactor, inst.sequence, context);
                    break;
                case InstructionCommand.FinishCameraPan:
                    Cutscene.EndCameraPan();
                    break;
                case InstructionCommand.RegisterPartyMember:
                    Party.RegisterPartyMember(GetArgument(inst.arguments, "entityID"));
                    break;
                case InstructionCommand.AddPartyMember:
                    Party.AddPartyMember(GetArgument(inst.arguments, "entityID"));
                    break;
                case InstructionCommand.EnablePartyMember:
                    Party.SetPartyMemberEnabled(GetArgument(inst.arguments, "entityID"), true);
                    break;
                case InstructionCommand.DisablePartyMember:
                    Party.SetPartyMemberEnabled(GetArgument(inst.arguments, "entityID"), false);
                    break;
                case InstructionCommand.RemovePartyMember:
                    Party.RemovePartyMember(GetArgument(inst.arguments, "entityID"));
                    break;
                case InstructionCommand.ClearParty:
                    var deletePartyMembers = GetArgument(inst.arguments, "deletePartyMembers") == "true";
                    Party.ClearParty(deletePartyMembers);
                    break;
                case InstructionCommand.ForceTractor:
                    Mobility.ForceTractor(GetArgument(inst.arguments, "entityID"), 
                        GetArgument(inst.arguments, "targetEntityID"));
                    break;
                case InstructionCommand.FinishMission:
                    FinishMission(context, 
                        GetArgument(inst.arguments, "rewardsText"),
                        GetArgument(inst.arguments, "soundID"));
                    break;
                case InstructionCommand.SetFlagInteractibility:
                    flagName = GetArgument(inst.arguments, "flagName");
                    sectorName = GetArgument(inst.arguments, "sectorName");
                    entityID = GetArgument(inst.arguments, "entityID");
                    var interactibility = FlagInteractibility.None;
                    if (!string.IsNullOrEmpty(entityID) || !string.IsNullOrEmpty(sectorName))
                    {
                        interactibility = FlagInteractibility.Warp;
                    }
                    if (inst.sequence.instructions != null)
                    {
                        interactibility = FlagInteractibility.Sequence;
                    }

                    foreach (var flag in AIData.flags)
                    {
                        if (flag.name != flagName)
                        {
                            continue;
                        }
                        
                        Debug.Log($"Set flag interactibility: {flagName}");
                        flag.interactibility = interactibility;
                        switch (interactibility)
                        {
                            case FlagInteractibility.Warp:
                                flag.sectorName = sectorName;
                                flag.entityID = entityID;
                                break;
                            case FlagInteractibility.Sequence:
                                flag.sequence = inst.sequence;
                                flag.context = context;
                                break;
                        }
                        
                        break;
                    }

                    break;
                case InstructionCommand.FinishTask:
                    TaskFlow.FinishTask(context);
                    break;
                case InstructionCommand.FailTask:
                    break;
                case InstructionCommand.FollowEntity:
                    Mobility.Follow(
                        GetArgument(inst.arguments, "entityID"),
                        GetArgument(inst.arguments, "targetEntityID"),
                        GetArgument(inst.arguments, "stopFollowing") == "true",
                        GetArgument(inst.arguments, "disallowAggression") == "true"
                    );
                    break;
                case InstructionCommand.Wait:
                    yield return new WaitForSeconds(float.Parse(GetArgument(inst.arguments, "time")));
                    break;
                case InstructionCommand.DevConsole:
                    DevConsoleScript.Instance.EnterCommand(GetArgument(inst.arguments, "command"), true);
                    break;
                case InstructionCommand.FadeIntoBlack:
                    var cStr = GetArgument(inst.arguments, "color");
                    var color = string.IsNullOrEmpty(cStr) ? Color.black : CoreScriptsDialogue.ParseColor(cStr);
                    var speedFactor = GetArgument(inst.arguments, "speedFactor") == null ? 1 : float.Parse(GetArgument(inst.arguments, "speedFactor"));
                    Cutscene.FadeIntoBlack(color, speedFactor);
                    break;
                case InstructionCommand.FadeOutOfBlack:
                    cStr = GetArgument(inst.arguments, "color");
                    color = string.IsNullOrEmpty(cStr) ? Color.black : CoreScriptsDialogue.ParseColor(cStr);
                    speedFactor = GetArgument(inst.arguments, "speedFactor") == null ? 1 : float.Parse(GetArgument(inst.arguments, "speedFactor"));
                    Cutscene.FadeOutOfBlack(color, speedFactor);
                    break;
                case InstructionCommand.DeleteEntity:
                    var id = GetArgument(inst.arguments, "entityID");
                    foreach (var data in AIData.entities)
                    {
                        if (data.ID == id)
                        {
                            Destroy(data.gameObject);
                            break;
                        }
                    }

                    var obj = SectorManager.instance.GetObjectByID(id);
                    if (obj) Destroy(obj);
                    break;
                case InstructionCommand.StartInflictionCosmetic:
                    id = GetArgument(inst.arguments, "entityID");
                    var type = GetArgument(inst.arguments, "type");
                    foreach (var data in AIData.entities)
                    {
                        if (data.ID != id)
                        {
                            continue;
                        }

                        switch (type)
                        {
                            case "PinDown":
                                PinDown.InflictionCosmetic(data, 0, false);
                                break;
                        }
                    }
                    
                    break;
                case InstructionCommand.FinishInflictionCosmetic:
                    id = GetArgument(inst.arguments, "entityID");
                    type = GetArgument(inst.arguments, "type");
                    foreach (var data in AIData.entities)
                    {
                        if (data.ID != id)
                        {
                            continue;
                        }

                        switch (type)
                        {
                            case "PinDown":
                                foreach (var part in data.GetComponentsInChildren<ShellPart>())
                                {
                                    var x = part.GetComponentInChildren<MissileAnimationScript>();
                                    if (x) Destroy(x.gameObject);
                                }
                                break;
                        }
                    }
                    
                    break;
            }
        }
        yield return null;
    }


    private static void FinishMission(Context context, string rewardsText, string jingleID)
    {
        if (!PlayerCore.Instance || PlayerCore.Instance.cursave == null || PlayerCore.Instance.cursave.missions == null)
        {
            return;
        }

        if (!TaskManager.objectiveLocations.ContainsKey(context.missionName))
        {
            Debug.LogWarning($"Task Manager does not contain an objective list for mission {context.missionName}");
        }
        // TODO: prevent using this node in DialogueCanvases
        var mission = PlayerCore.Instance.cursave.missions.Find(
            (m) => m.name == context.missionName);
        mission.status = Mission.MissionStatus.Complete;
        if (CoreScriptsManager.OnVariableUpdate != null)
        {
            CoreScriptsManager.OnVariableUpdate.Invoke("MissionStatus(");
        }

        if (!string.IsNullOrEmpty(rewardsText))
        {
            DialogueSystem.ShowMissionComplete(mission, rewardsText);
        }

        if (!string.IsNullOrEmpty(jingleID))
        {
            AudioManager.OverrideMusicTemporarily(jingleID);
        }

        if (TaskManager.objectiveLocations.ContainsKey(context.missionName))
        {
            TaskManager.objectiveLocations[context.missionName].Clear();
        }


        TaskManager.Instance.AttemptAutoSave();
    }

    public static Sequence ParseSequence(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var seq = new Sequence();
        seq.instructions = new List<Instruction>();

        List<string> stx = null;
        bool skipToComma = false;
        int brax = 0;

        List<string> standardInstructions = new List<string>();

        foreach (string instType in Enum.GetNames(typeof(InstructionCommand)))
        {
            if (instType != "Call" && instType != "ConditionBlock")
            {
                standardInstructions.Add(instType);
            }
        }

        line = GetValueScopeWithinLine(line, index);
        index = CoreScriptsManager.GetNextOccurenceInScope(0, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            if (standardInstructions.Exists(s => lineSubstr.StartsWith(s)))
            {
                seq.instructions.Add(ParseInstruction(i, line, blocks));
            }
            else if (lineSubstr.StartsWith("Call"))
            {
                var funcName = lineSubstr.Substring(5);
                funcName = funcName.Substring(0, funcName.IndexOf(")"));
                var inst = new Instruction();
                inst.arguments = AddArgument(inst.arguments, "name", funcName);
                inst.command = InstructionCommand.Call;
                seq.instructions.Add(inst);
            }
            else if (lineSubstr.StartsWith("ConditionBlock"))
            {
                var b = CoreScriptsCondition.ParseConditionBlock(i, line, blocks);
                blocks.Add(b.ID, b);
                var inst = new Instruction();
                inst.command = InstructionCommand.ConditionBlock;
                inst.arguments = AddArgument(inst.arguments, "ID", b.ID+"");
                seq.instructions.Add(inst);
            }
        }

        return seq;
    }

    public static string GetValueScopeWithinLine(string line, int index)
    {
        // find start and finish of line
        int start = index;
        int end = index;
        bool alreadyEnded = false;
        for (int i = index; i < line.Length; i++)
        {
            if (line[i] == '(')
            {
                start = i;
                break;
            }
        }

        int brackets = 0;
        for (int i = start; i < line.Length; i++)
        {
            if (alreadyEnded && (line[i] == ',' || line[i] == ')'))
            {
                break;
            }
            if (line[i] == '(')
            {
                if (alreadyEnded)
                {
                    throw new Exception($"A line reopens scope after closing it. You might be missing a comma or a closing bracket.\n Search for: {line.Substring(i)}");
                }
                brackets++;
            }
            if (line[i] == ')' && !alreadyEnded)
            {
                brackets--;
                if (brackets == 0)
                {
                    end = i;
                    alreadyEnded = true;
                }
            }
        }


        var x = line.Substring(start, end-start+1);
        return x;
    }


    private static Instruction ParseInstruction(int index, string line, Dictionary<int, ConditionBlock> blocks)
    {
        var substr = line.Substring(index).Split("(")[0].Trim();
        var inst = new Instruction();
        Enum.TryParse<InstructionCommand>(substr, out inst.command);
        inst.arguments = "";
        bool skipToComma = true;
        List<string> stx = null;
        int brax = 0;

        line = GetValueScopeWithinLine(line, index);
        
        index = CoreScriptsManager.GetNextOccurenceInScope(0, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);

            var name = "";
            var val = "";
            GetNameAndValue(lineSubstr, out name, out val, true);
            if (name == "sequence")
            {
                inst.sequence = ParseSequence(i, line, blocks);
                continue;
            }

            inst.arguments = AddArgument(inst.arguments, name, val);
        }

        return inst;
    }

    public static void GetNameAndValue(string line, out string name, out string val, bool continueThroughScopes = false)
    {
        if (!line.Contains("="))
        {
            name = val = "";
            return;
        }

        var brackets = 0;
        name = line.Split("=")[0];
        val = line.Split("=")[1];
        int minIndex = val.Length;

        var commaExists = val.IndexOf(',') != -1;

        for (int i = 0; i < val.Length; i++)
        {
            if (val[i] == '(')
            {
                brackets++;
            }
            else if (val[i] == ')')
            {
                brackets--;
            }

            if (brackets > 0 && continueThroughScopes) continue;

            if (val[i] == ',' || ((!commaExists || brackets < 0) && val[i] == ')')) 
            {
                minIndex = i;
                break;
            }
        }

        var n = name;
        var v = val;
        val = val.Substring(0, minIndex);
        if (specialChars.Exists(e => n.Contains(e)))
        {
            throw new System.Exception($"Attribute names or values cannot have semicolons or commas in them: {n}");
        }

        
        if (specialChars.Exists(e => v.Contains(e)))
        {
            throw new System.Exception($"Attribute names or values cannot have semicolons or commas in them: {v}");
        }
    }

    private static void SpawnEntity(string entityID, bool forceCharacterTeleport, string flagName, string blueprintJSON, int faction, string name, string assetID)
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

        if (!string.IsNullOrEmpty(assetID))
        {
            Sector.LevelEntity entityData = new Sector.LevelEntity
            {
                faction = faction,
                name = name,
                position = coords,
                ID = entityID,
                assetID = assetID
            };
            
            SectorManager.instance.SpawnAsset(entityData);
            return;
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
                        Debug.Log($"Warping to {coords.x}, {coords.y}...");
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
}
