using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using static CoreScriptsSequence;
using NodeEditorFramework.Standard;
using static CoreScriptsCondition;
using static Entity;

public class CoreScriptsManager : MonoBehaviour
{
    public Dictionary<string, Dialogue> dialogues = new Dictionary<string, Dialogue>();
    private string codePath = System.IO.Path.Combine(Application.streamingAssetsPath, "CodeTest.codecanvas");
    private Dictionary<string, string> localMap = new Dictionary<string, string>();
    private Dictionary<string, Sequence> functions = new Dictionary<string, Sequence>();
    private Dictionary<string, Task> tasks = new Dictionary<string, Task>();
    public Dictionary<int, ConditionBlock> conditionBlocks = new Dictionary<int, ConditionBlock>();
    public Dictionary<string, EntityDeathDelegate> entityDeathDelegates = new Dictionary<string, EntityDeathDelegate>();
    public Dictionary<string, string> globalVariables = new Dictionary<string, string>();
    public static CoreScriptsManager instance;
    
    public string GetLocalMapString(string key)
    {
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
    private Dictionary<FileCoord, FileCoord> stringScopes = new Dictionary<FileCoord, FileCoord>();
    private List<Context> missionTriggers = new List<Context>();
    private HashSet<int> commentLines = new HashSet<int>();
    public enum TriggerType
    {
        Mission,
        Launch,
        Sector,
        Spawn
    }
    public class Context
    {
        public TriggerType type;
        public string missionName;
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

    public void Initialize()
    {
        if (initialized || MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off) return;
        // initialize save var arrays
        if(SaveHandler.instance.GetSave().coreScriptsGlobalVarNames == null)
            SaveHandler.instance.GetSave().coreScriptsGlobalVarNames = new List<string>();
        
        if(SaveHandler.instance.GetSave().coreScriptsGlobalVarValues == null)
            SaveHandler.instance.GetSave().coreScriptsGlobalVarValues = new List<string>();

        Parse();
        foreach (var context in missionTriggers)
        {
            RunMissionTrigger(context);
        }
    }

    void RunMissionTrigger(Context context)
    {
        StartMissionNode.TryAddMission(context.missionName, "B", context.entryPoint, Color.white, 0, context.prerequisites);
        CoreScriptsSequence.RunSequence(context.sequence, context);
    }

    void Parse()
    {
        string[] lines = System.IO.File.ReadAllLines(codePath);
        GetCommentLines(lines);
        GetStringScopes(lines);
        dialogues = new Dictionary<string, Dialogue>();
        
        SetUpLocalMap(lines);

        var data = new ScopeParseData();
        data.stringScopes = stringScopes;
        data.localMap = localMap;
        data.dialogues = dialogues;
        data.tasks = tasks;
        data.commentLines = commentLines;
        data.blocks = conditionBlocks;

        // Pass 1: get tasks, so that dialogue can use the map
        FileCoord d = new FileCoord();
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;
            if (lines[i].Substring(c).StartsWith("Task"))
            {
                var task = CoreScriptsTask.ParseTask(i, c, lines, data, out d);
                tasks.Add(task.taskID, task);
            }
            d = StringSensitiveIterator(d, lines, stringScopes, commentLines);
        }


        d = new FileCoord();
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;

            if (lines[i].Substring(c).StartsWith("Dialogue"))
            {

                CoreScriptsDialogue.ParseDialogue(i, c, lines, data, out d);
            }
            else if (lines[i].Substring(c).StartsWith("Function"))
            {
                var func = CoreScriptsFunction.ParseFunction(i, c, lines, data, out d);
                functions.Add(func.name, func.sequence);
            }
            else if (lines[i].Substring(c).StartsWith("MissionTrigger"))
            {
                missionTriggers.Add(CoreScriptsMissionTrigger.ParseMissionTrigger(i, c, lines, data, out d));
            }
            d = StringSensitiveIterator(d, lines, stringScopes, commentLines);
        }
    }

    private void GetCommentLines(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("//"))
            {
                commentLines.Add(i);
            }
        }
    }


    private void GetStringScopes(string[] lines)
    {

        bool escaped = false;
        bool inScope = false;
        FileCoord start = new FileCoord();
        FileCoord interval = new FileCoord();
        while (interval.line < lines.Length)
        {
            var ch = lines[interval.line][interval.character];
            // TODO: using backslashes will break the local map
            if (ch == '\\' && !escaped)
            {
                escaped = true;
                interval = IncrementFileCoordWithComments(1, interval, lines, commentLines);
                continue;
            }

            if (escaped)
            {
                escaped = false;
                interval = IncrementFileCoordWithComments(1, interval, lines, commentLines);
                continue;
            }
            if (ch == '"')
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
            interval = IncrementFileCoordWithComments(1, interval, lines, commentLines);
        }
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
            while (cnt < scope.Length && scope[cnt] == ' ') cnt++;
            return cnt;
        }

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
        if (skipToComma) cnt++;
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

            if (brackets > 1 || scope[cnt] == ' ')
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
     out FileCoord endOfScope)
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
                continue;
            }

            if (ignoreSpecialChar)
            {
                ignoreSpecialChar = false;
                c++;
                continue;
            }

            if (ch == '(') bCount++;
            if (!searchStarted && bCount > 1)
            {
                searchStarted = true;
            }
            else if (ch == ')') bCount--;
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


    void SetUpLocalMap(string[] lines)
    {
        localMap.Clear();
        bool stringMode = false;
        FileCoord coord = new FileCoord();
        var tok1 = "";
        var tok2 = "";
        var quotes = 0;
        while (coord.line < lines.Length)
        {
            string currentLine = lines[coord.line].Substring(coord.character);
            if (!stringMode && currentLine.StartsWith("Text("))
            {
                stringMode = true;
                quotes = 0;
                tok1 = "";
                tok2 = "";
                coord = IncrementFileCoordWithComments(5, coord, lines, commentLines);
            }

            if (stringMode)
            {
                var x = lines[coord.line][coord.character];
                if (x == '"') 
                {
                    quotes++;
                    coord = IncrementFileCoordWithComments(1, coord, lines, commentLines);
                    // TODO: enable escaping quotes
                    if (quotes == 4)
                    {
                        stringMode = false;
                        localMap.Add(tok1, tok2);
                    }
                    continue;
                }

                
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
