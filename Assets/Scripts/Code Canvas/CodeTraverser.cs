using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using static CodeCanvasSequence;
using NodeEditorFramework.Standard;
using static CodeCanvasCondition;
using static Entity;

public class CodeTraverser : MonoBehaviour
{
    public Dictionary<string, Dialogue> dialogues = new Dictionary<string, Dialogue>();
    private string codePath = System.IO.Path.Combine(Application.streamingAssetsPath, "CodeTest.codecanvas");
    private Dictionary<string, string> localMap = new Dictionary<string, string>();
    private Dictionary<string, Sequence> functions = new Dictionary<string, Sequence>();
    private Dictionary<string, Task> tasks = new Dictionary<string, Task>();
    public Dictionary<int, ConditionBlock> conditionBlocks = new Dictionary<int, ConditionBlock>();
    public Dictionary<string, EntityDeathDelegate> entityDeathDelegates = new Dictionary<string, EntityDeathDelegate>();

    public Sequence GetFunction(string key)
    {
        if (!functions.ContainsKey(key)) throw new System.Exception("Invalid function name execution.");
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
        public int taskHash;
        public List<string> prerequisites;
        public Sequence sequence;
        public CodeTraverser traverser;
    }

    // Start is called before the first frame update
    void Start()
    {
        Parse();
        foreach (var context in missionTriggers)
        {
            RunMissionTrigger(context);
        }
    }



    void RunMissionTrigger(Context context)
    {
        StartMissionNode.TryAddMission(context.missionName, "B", "Test.", Color.white, 0, context.prerequisites);
        CodeCanvasSequence.RunSequence(context.sequence, context);
    }

    void Parse()
    {
        string[] lines = System.IO.File.ReadAllLines(codePath);
        GetStringScopes(lines);
        dialogues = new Dictionary<string, Dialogue>();
        
        SetUpLocalMap(lines);

        // Pass 1: get tasks, so that dialogue can use the map
        FileCoord d = new FileCoord();
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;
            if (lines[i].Substring(c).StartsWith("Task"))
            {
                var task = CodeCanvasTask.ParseTask(i, c, lines, stringScopes, localMap, out d);
                tasks.Add(task.taskID, task);
            }
            d = StringSensitiveIterator(d, lines, stringScopes);
        }

        d = new FileCoord();
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;
            if (lines[i].Substring(c).StartsWith("Dialogue"))
            {
                CodeCanvasDialogue.ParseDialogue(i, c, lines, stringScopes, localMap, dialogues, tasks, out d);
            }
            else if (lines[i].Substring(c).StartsWith("Function"))
            {
                var func = CodeCanvasFunction.ParseFunction(i, c, lines, stringScopes, conditionBlocks, out d);
                functions.Add(func.name, func.sequence);
            }
            else if (lines[i].Substring(c).StartsWith("MissionTrigger"))
            {
                missionTriggers.Add(CodeCanvasMissionTrigger.ParseMissionTrigger(i, c, lines, stringScopes, conditionBlocks, this, out d));
            }
            d = StringSensitiveIterator(d, lines, stringScopes);
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
                interval = IncrementFileCoord(1, interval, lines);
                continue;
            }

            if (escaped)
            {
                escaped = false;
                interval = IncrementFileCoord(1, interval, lines);
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
            interval = IncrementFileCoord(1, interval, lines);
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

    public static string GetScope(int startLineNum, string[] lines, Dictionary<FileCoord, FileCoord> stringScopes, out FileCoord endOfScope)
    {
        int bCount = 0;
        var searchStarted = false;
        var builder = new StringBuilder();
        
        for (FileCoord d = new FileCoord(startLineNum, 0); d.line < lines.Length; d = StringSensitiveIterator(d, lines, stringScopes))
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

    // TODO: optimize complexity
    private FileCoord IncrementFileCoord(int val, FileCoord coord, string[] lines)
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
                coord = IncrementFileCoord(5, coord, lines);
            }

            if (stringMode)
            {
                var x = lines[coord.line][coord.character];
                if (x == '"') 
                {
                    quotes++;
                    coord = IncrementFileCoord(1, coord, lines);
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
            coord = IncrementFileCoord(1, coord, lines);
            if (coord.line != oldLine && stringMode && quotes % 2 != 0)
            {
                if (quotes < 2) tok1 += '\n';
                else tok2 += '\n';
            }
        }
    }

    public static FileCoord StringSensitiveIterator(FileCoord current, string[] lines, Dictionary<FileCoord, FileCoord> stringScopes)
    {
        if (current.line >= lines.Length) return current;
        if (stringScopes.ContainsKey(current))
        {
            current = new FileCoord(stringScopes[current].line, stringScopes[current].character + 1);
        }

        current.character++;
        while (current.character > lines[current.line].Length - 1)
        {
            current.line++;
            current.character = 0;
            if (current.line >= lines.Length) return current;
        }
        
        // Debug.LogWarning(current.line + " " + current.character + " " + lines[current.line].Length);
        return current;
    }

}
