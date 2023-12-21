using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using static CoreScriptsSequence;
using NodeEditorFramework.Standard;
using static CoreScriptsCondition;
using static Entity;
using static SectorManager;
using static TaskManager;

public class CoreScriptsManager : MonoBehaviour
{
    public Dictionary<string, Dialogue> dialogues = new Dictionary<string, Dialogue>();
    private Dictionary<string, string> localMap = new Dictionary<string, string>();
    private Dictionary<string, Sequence> functions = new Dictionary<string, Sequence>();
    private Dictionary<string, Task> tasks = new Dictionary<string, Task>();
    public Dictionary<int, ConditionBlock> conditionBlocks = new Dictionary<int, ConditionBlock>();
    public Dictionary<string, EntityDeathDelegate> entityDeathDelegates = new Dictionary<string, EntityDeathDelegate>();
    public Dictionary<string, SectorLoadDelegate> sectorLoadDelegates = new Dictionary<string, SectorLoadDelegate>();
    public Dictionary<string, VariableChangedDelegate> variableChangedDelegates = new Dictionary<string, VariableChangedDelegate>(); 
    public Dictionary<string, Coroutine> timerCoroutines = new Dictionary<string, Coroutine>();
    public Dictionary<string, string> globalVariables = new Dictionary<string, string>();
    public Dictionary<string, ObjectiveLocation> objectiveLocations = new Dictionary<string, ObjectiveLocation>();
    public Dictionary<string, ProximityData> distanceConditions = new Dictionary<string, ProximityData>();
    public Dictionary<string, FusionData> fusionConditions = new Dictionary<string, FusionData>();

    private List<Context> missionTriggers = new List<Context>();
    private List<Context> startTriggers = new List<Context>();
    private List<Context> sectorTriggers = new List<Context>();     
    public static string[] paths;
    public delegate void VariableChangedDelegate(string variable);
    public static VariableChangedDelegate OnVariableUpdate;
    public static CoreScriptsManager instance;
    public void ClearAllData()
    {
        dialogues.Clear();
        localMap.Clear();
        functions.Clear();
        tasks.Clear();
        conditionBlocks.Clear();
        entityDeathDelegates.Clear();
        sectorLoadDelegates.Clear();
        variableChangedDelegates.Clear();
        timerCoroutines.Clear();
        globalVariables.Clear();
        objectiveLocations.Clear();
        missionTriggers.Clear();
        startTriggers.Clear();
        sectorTriggers.Clear();
        OnVariableUpdate = null;
    }

    public class ProximityData
    {
        public string ent1ID;
        public string ent2ID;
        public string comp;
        public float distanceValue;
        public Transform t1;
        public Transform t2;
        public Condition cond;
        public ConditionBlock block;
    }

    public class FusionData
    {
        public Condition cond;
        public ConditionBlock block;
    }

    public Task GetTask(string taskID)
    {
        if (!tasks.ContainsKey(taskID))
        {
            Debug.LogError($"Task not found: {taskID}");
        }
        return tasks[taskID];
    }

    private void Update()
    {
        while (RunDistanceChecks()) {}
    }

    public void RunFuseChecks()
    {
        while (fusionConditions.Values.Count > 0)
        {
            foreach (var data in fusionConditions.Values)
            {
                SatisfyCondition(data.cond, data.block);
                break;
            }
        }
    }

    private bool RunDistanceChecks()
    {
        foreach (var data in distanceConditions.Values)
        {
            var satisfy = RunDistanceCondition(data);

            if (satisfy)
            {
                SatisfyCondition(data.cond, data.block);
                return true;
            }
        }
        return false;
    }

    public static Transform GetProximityTransformFromID(string ID)
    {
        var e = AIData.entities.Find(e => e.ID == ID);
        if (e) return e.transform;
        var f = AIData.flags.Find(f => f.name == ID);
        if (f) return f.transform;
        return null;
    }


    public static bool RunDistanceCondition(ProximityData data)
    {
        if (!data.t1) data.t1 = GetProximityTransformFromID(data.ent1ID);
        if (!data.t2) data.t2 = GetProximityTransformFromID(data.ent2ID);
        if (!data.t1 || !data.t2) return false;
        var sqDist = Vector2.SqrMagnitude(data.t1.position - data.t2.position);
        switch (data.comp)
        {
            case "Eq":
                return sqDist == data.distanceValue;
            case "Neq":
                return sqDist != data.distanceValue;
            case "Lt":
                // TODO: which way the SqrDistance is inputted affects this
                return sqDist < data.distanceValue;
            case "Gt":
                return sqDist > data.distanceValue;
        }
        return false;
    }


    public static void AssertArgumentsPresent(string args, string statementType, List<string> argNames)
    {
        foreach (var argName in argNames)
        {
            if (string.IsNullOrEmpty(GetArgument(args, argName)))
            {
                throw new System.Exception($"The required argument \"{argName}\" is missing from a statement of type {statementType}.");
            }
        }
    }


    public string GetLocalMapString(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        key = key.Trim();
        if (!localMap.ContainsKey(key)) return "";
        else return localMap[key];
    }

    public Sequence GetFunction(string key)
    {
        if (key == null || !functions.ContainsKey(key)) throw new System.Exception($"Invalid function name: {key}");
        return functions[key];
    }

    public struct FileCoord
    {
        public FileCoord(int line, int character)
        {
            this.line = line;
            this.character = character;
        }
        public int line;
        public int character;
    }
    public enum TriggerType
    {
        Mission,
        Start,
        Sector,
        Spawn
    }
    public class Context
    {
        public TriggerType type;
        public string missionName;
        public string sectorName;
        public int episode;
        public string entryPoint;
        public int taskHash;
        public List<string> prerequisites;
        public Sequence sequence;
    }

    private bool initialized = false;

    private void Awake()
    {
        instance = this;
    }

    public void Reinitialize()
    {
        initialized = false;
        foreach (var val in CoreScriptsManager.instance.conditionBlocks.Values)
        {
            CoreScriptsCondition.DeinitializeAllConditions(val);
        }
        ClearAllData();
        Initialize();             
    }

    public static List<string> canvasMissions;

    public void Initialize()
    {
        if (initialized || MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off) return;
        // initialize save var arrays
        if(SaveHandler.instance.GetSave().coreScriptsGlobalVarNames == null)
            SaveHandler.instance.GetSave().coreScriptsGlobalVarNames = new List<string>();
        
        if(SaveHandler.instance.GetSave().coreScriptsGlobalVarValues == null)
            SaveHandler.instance.GetSave().coreScriptsGlobalVarValues = new List<string>();

        timerCoroutines.Clear();
        objectiveLocations.Clear();
        localMap.Clear();
        OnVariableUpdate = null;

        initialized = true;
        if (paths == null)
        {
            Debug.LogWarning("No paths to parse. Returning.");
            return;
        }

        dialogues = new Dictionary<string, Dialogue>();
        Parse(paths);

        foreach (var context in missionTriggers)
        {
            RunMissionTrigger(context);
        }

        PlayerCore.Instance.cursave.missions.RemoveAll(m => !canvasMissions.Exists(p =>
            System.IO.Path.GetFileNameWithoutExtension(p) == m.name) && !missionTriggers.Exists(t => t.missionName == m.name)   );

        foreach (var context in startTriggers)
        {
            CoreScriptsSequence.RunSequence(context.sequence, context);
        }

        SectorManager.OnSectorLoad += (s) => {
            
            var context = sectorTriggers.Find(c => c.sectorName == s);
            if (context != null)
            {
                CoreScriptsSequence.RunSequence(context.sequence, context);
            }
        };

        var current = sectorTriggers.Find(c => c.sectorName == SectorManager.instance.current.sectorName);
        if (current != null) CoreScriptsSequence.RunSequence(current.sequence, current);

    }

    void RunMissionTrigger(Context context)
    {
        StartMissionNode.TryAddMission(context.missionName, "B", context.entryPoint, Color.white, context.episode, context.prerequisites);
        CoreScriptsSequence.RunSequence(context.sequence, context);
    }


    // Pass 1: get tasks, so that dialogue can use the map
    ScopeParseData ParsePass1(string[] lines)
    {
        var commentLines = GetCommentLines(lines);
        var stringScopes = GetStringScopes(lines, commentLines);
        
        SetUpLocalMap(lines, commentLines);

        var data = new ScopeParseData();
        data.stringScopes = stringScopes;
        data.localMap = localMap;
        data.dialogues = dialogues;
        data.tasks = tasks;
        data.commentLines = commentLines;
        data.blocks = conditionBlocks;

        FileCoord d = new FileCoord();
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;
            if (lines[i].Substring(c).StartsWith("Task") 
                && (c == 0 || char.IsWhiteSpace(lines[i][c])))
            {
                var task = CoreScriptsTask.ParseTask(i, c, lines, data, out d);
                tasks.Add(task.taskID, task);
            }
            d = StringSensitiveIterator(d, lines, stringScopes, commentLines);
        }

        return data;
    }

    void ParsePass2(string[] lines, ScopeParseData data)
    {
        FileCoord d = new FileCoord();
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;
            if (lines[i].Substring(c).StartsWith("Dialogue"))
            {
                CoreScriptsDialogue.ParseDialogue(i, c, lines, data, out d);
            }
            else if (lines[i].Substring(c).StartsWith("D("))
            {
                CoreScriptsDialogue.ParseDialogueShortened(i, c, lines, data, out d);
            }
            else if (lines[i].Substring(c).StartsWith("Function"))
            {
                var func = CoreScriptsFunction.ParseFunction(i, c, lines, data, out d);
                functions.Add(func.name, func.sequence);
            }
            else if (lines[i].Substring(c).StartsWith("MissionTrigger"))
            {
                missionTriggers.Add(CoreScriptsTrigger.ParseTrigger(i, c, lines, TriggerType.Mission, data, out d));
            }
            else if (lines[i].Substring(c).StartsWith("StartTrigger"))
            {
                startTriggers.Add(CoreScriptsTrigger.ParseTrigger(i, c, lines, TriggerType.Start, data, out d));
            }
            else if (lines[i].Substring(c).StartsWith("SectorTrigger"))
            {
                sectorTriggers.Add(CoreScriptsTrigger.ParseTrigger(i, c, lines, TriggerType.Sector, data, out d));
            }
            d = StringSensitiveIterator(d, lines, data.stringScopes, data.commentLines);
        }
    }

    void Parse(string[] paths)
    {
        var linesMap = new Dictionary<string, string[]>();
        var dataMap = new Dictionary<string, ScopeParseData>();

        foreach (var path in paths)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            linesMap.Add(path, lines);
            var data = ParsePass1(lines);
            dataMap.Add(path, data);
        }

        foreach (var path in paths)
        {
            ParsePass2(linesMap[path], dataMap[path]);
        }
        
    }

    private HashSet<int> GetCommentLines(string[] lines)
    {
        var commentLines = new HashSet<int>();
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("//"))
            {
                commentLines.Add(i);
            }
        }

        return commentLines;
    }


    private Dictionary<FileCoord, FileCoord> GetStringScopes(string[] lines, HashSet<int> commentLines)
    {
        var stringScopes = new Dictionary<FileCoord, FileCoord>();
        bool escaped = false;
        bool inScope = false;
        FileCoord start = new FileCoord();
        FileCoord interval = new FileCoord();
        while (interval.line < lines.Length)
        {
            var ch = lines[interval.line][interval.character];
            if (ch == '\\')
            {
                escaped = true;
                interval = IncrementFileCoordWithComments(1, interval, lines, commentLines);
                continue;
            }

            if (ch == '"' && !escaped)
            {
                if (!inScope)
                {
                    inScope = true;
                    start = interval;
                }
                else
                {
                    stringScopes.Add(start, interval);
                    inScope = false;
                }
            }

            if (escaped)
            {
                escaped = false;
            }
            interval = IncrementFileCoordWithComments(1, interval, lines, commentLines);
        }

        return stringScopes;
    }

    // if strings is null it treats every character as valid for comma skipping purposes
    public static int GetNextOccurenceInScope(int lastOccurrence, string scope, List<string> strings, 
        ref int brackets, ref bool skipToComma, char opBracket, char clBracket)
    {
        // TODO: Optimize by turning string list into a dict, and then using max length strings as keys
        var cnt = lastOccurrence;

        // find the first bracket
        while (cnt < scope.Length && scope[cnt] != opBracket && brackets == 0) cnt++;
        if (cnt < scope.Length) cnt++;
        if (brackets == 0)
        {
            brackets = 1;
            while (cnt < scope.Length && char.IsWhiteSpace(scope[cnt])) cnt++;
            return cnt;
        }

        if (skipToComma && cnt <= scope.Length && cnt-1 >= 0 && scope[cnt-1] == ',') cnt--;
        while (skipToComma && cnt < scope.Length && (brackets > 1 || scope[cnt] != ',')) 
        {
            if (scope[cnt] == opBracket)
            {
                brackets++;
            }
            else if (scope[cnt] == clBracket)
            {
                brackets--;
            }

            cnt++;
        }
        if (skipToComma) 
        {
            cnt++;
            while (cnt < scope.Length && char.IsWhiteSpace(scope[cnt]))
            {
                cnt++;
            }
            if (cnt < scope.Length && scope[cnt] == ')') 
            {
                return scope.Length;
            }
        }
        skipToComma = false;


        while(cnt < scope.Length && brackets > 0)
        {
            if (scope[cnt] == opBracket)
            {
                brackets++;
            }
            else if (scope[cnt] == clBracket)
            {
                brackets--;
            }

            if (brackets > 1 || char.IsWhiteSpace(scope[cnt]))
            {
                cnt++;
                continue;
            }

            if (strings != null)
            {
                var x = strings.Find(s => scope.Substring(cnt).StartsWith(s));
                if (x != null) return cnt;
            }
            else return cnt;
            cnt++;
        }

        return scope.Length;
    }

    public struct ScopeParseData
    {
        public Dictionary<FileCoord, FileCoord> stringScopes;
        public Dictionary<string, string> localMap;
        public Dictionary<string, Dialogue> dialogues;
        public HashSet<int> commentLines;
        public Dictionary<int, ConditionBlock> blocks;
        public Dictionary<string, Task> tasks;
    }

    public static string GetScope(int startLineNum, string[] lines, 
    Dictionary<FileCoord, FileCoord> stringScopes, HashSet<int> commentLines,
     out FileCoord endOfScope, bool debug = false)
    {
        int bCount = 0;
        var searchStarted = false;
        var builder = new StringBuilder();
        
        for (FileCoord d = new FileCoord(startLineNum, 0); d.line < lines.Length; d = StringSensitiveIterator(d, lines, stringScopes, commentLines))
        {
            var i = d.line;
            var c = d.character;
            bool ignoreSpecialChar = false;
            var ch = lines[i][c];
            if (ch == '\\')
            {
                ignoreSpecialChar = true;
                c++;
                continue;
            }

            builder.Append(ch);
            if (ch != '(' && ch != ')')
            {
                if (debug) Debug.Log(lines[i].Substring(c) + bCount);
                continue;
            }

            if (ignoreSpecialChar)
            {
                ignoreSpecialChar = false;
                c++;
                continue;
            }

            if (ch == '(') bCount++;
            if (!searchStarted && bCount >= 1)
            {
                searchStarted = true;
            }
            else if (ch == ')')
            {
                bCount--;
            }
            if (debug) Debug.Log(lines[i].Substring(c) + bCount);
            if (searchStarted && bCount == 0)
            {
                endOfScope = d;
                return builder.ToString();
            }
        }
        throw new System.Exception("Did not finish a scope starting at line: " + (startLineNum+1));
    }

    private static FileCoord IncrementFileCoordWithComments(int val, FileCoord coord, string[] lines, HashSet<int> commentLines)
    {
        coord = IncrementFileCoord(val, coord, lines);
        while (commentLines.Contains(coord.line) || (coord.line < lines.Length && coord.character >= lines[coord.line].Length))
        {
            coord.line++;
            coord.character = 0;
        }
        return coord;
        
    }

    // TODO: optimize complexity
    private static FileCoord IncrementFileCoord(int val, FileCoord coord, string[] lines)
    {
        for (int i = 0; i < val; i++)
        {
            coord.character++;
            while (coord.line < lines.Length && coord.character >= lines[coord.line].Length)
            {
                coord.character = 0;
                coord.line++;
            }
        }
        return coord;
    }


    void SetUpLocalMap(string[] lines, HashSet<int> commentLines)
    {
        bool stringMode = false;
        FileCoord coord = new FileCoord();
        var tok1 = "";
        var tok2 = "";
        var delimiter = '"';
        var quotes = 0;
        var lastCharWasBackslash = false;
        while (coord.line < lines.Length)
        {
            string currentLine = lines[coord.line].Substring(coord.character);
            if (!stringMode && currentLine.StartsWith("Text("))
            {
                delimiter = currentLine.Substring(currentLine.IndexOf("Text(")+5).Trim()[0];
                stringMode = true;
                quotes = 0;
                tok1 = "";
                tok2 = "";
                coord = IncrementFileCoordWithComments(5, coord, lines, commentLines);
            }

            if (stringMode)
            {
                var x = lines[coord.line][coord.character];
                if (x == '\\')
                    lastCharWasBackslash = true;
                
                if (x == delimiter && !lastCharWasBackslash) 
                {
                    quotes++;
                    coord = IncrementFileCoordWithComments(1, coord, lines, commentLines);
                    if (quotes == 4)
                    {
                        stringMode = false;
                        
                        tok2 = tok2.Replace("\\\"", "\"");
                        localMap.Add(tok1, tok2);
                    }
                    continue;
                }

                if (x != '\\' && lastCharWasBackslash) lastCharWasBackslash = false;
                
                if (quotes < 2 && quotes > 0) tok1 += x;
                else if (quotes > 2) tok2 += x;
            }

            var oldLine = coord.line;
            coord = IncrementFileCoordWithComments(1, coord, lines, commentLines);
            if (coord.line != oldLine && stringMode && quotes % 2 != 0)
            {
                if (quotes < 2) tok1 += '\n';
                else tok2 += '\n';
            }
        }
    }

    private static FileCoord StringSensitiveIterator(FileCoord current, string[] lines, 
        Dictionary<FileCoord, FileCoord> stringScopes, HashSet<int> commentLines)
    {
        if (current.line >= lines.Length) return current;
        if (stringScopes.ContainsKey(current))
        {
            current = new FileCoord(stringScopes[current].line, stringScopes[current].character + 1);
        }

        return IncrementFileCoordWithComments(1, current, lines, commentLines);
    }

}
