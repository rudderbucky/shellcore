using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using static CodeCanvasSequence;

public class CodeTraverser : MonoBehaviour
{
    private List<string> instructions;
    public Dictionary<string, Dialogue> dialogues = new Dictionary<string, Dialogue>();
    private int index;
    private string codePath = System.IO.Path.Combine(Application.streamingAssetsPath, "CodeTest.codecanvas");
    private Dictionary<string, string> localMap = new Dictionary<string, string>();
    private Dictionary<int, string> dialogueScopes = new Dictionary<int, string>();
    private Dictionary<int, string> responseListScopes = new Dictionary<int, string>();
    private Dictionary<int, string> responseScopes = new Dictionary<int, string>();
    private Dictionary<string, Sequence> functions = new Dictionary<string, Sequence>();
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

    // Start is called before the first frame update
    void Start()
    {
        Parse();
        CodeCanvasSequence.RunSequence(functions["fortnite pro gamer 69"], this);
    }

    private void ParseMissionTrigger()
    {

    }

    void Parse()
    {
        string[] lines = System.IO.File.ReadAllLines(codePath);
        GetStringScopes(lines);
        dialogues = new Dictionary<string, Dialogue>();
        
        SetUpLocalMap(lines);

        FileCoord d = new FileCoord(); 
        while (d.line < lines.Length)
        {
            var i = d.line;
            var c = d.character;
            if (lines[i].Substring(c).StartsWith("Dialogue"))
            {
                var f = d;
                CodeCanvasDialogue.ParseDialogue(i, c, lines, stringScopes, localMap, dialogues, out d);
            }
            else if (lines[i].Substring(c).StartsWith("Function"))
            {
                var func = CodeCanvasFunction.ParseFunction(i, c, lines, stringScopes, out d);
                functions.Add(func.name, func.sequence);
            }
            d = StringSensitiveIterator(d, lines, stringScopes);
        }
    }

   private void GetStringScopes(string[] lines)
    {

        bool escaped = false;
        bool inScope = false;
        FileCoord interval = new FileCoord();
        for (int i = 0; i < lines.Length; i++)
        {
            for (int c = 0; c < lines[i].Length; c++)
            {
                var ch = lines[i][c];
                if (ch == '\\' && !escaped)
                {
                    escaped = true;
                    continue;
                }

                if (escaped)
                {
                    escaped = false;
                    continue;
                }
                if (ch == '"')
                {
                    if (!inScope)
                    {
                        inScope = true;
                        interval.line = i;
                        interval.character = c;
                    }
                    else
                    {
                        stringScopes.Add(interval, new FileCoord(i, c));
                        inScope = false;
                    }
                }
            }
        }
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


    void SetUpLocalMap(string[] lines)
    {
        int lineIndex = 0;
        localMap.Clear();
        while (lineIndex < lines.Length && lineIndex != -1)
        {
            string currentLine = lines[lineIndex];
            string[] tokens = currentLine.Split(" ");


            if (tokens.Length > 0 && tokens[0].StartsWith("Text("))
            {
                var tok1 = "";
                var tok2 = "";
                var quotes = 0;
                foreach (char x in currentLine.Substring(5))
                {
                    if (x == '"') 
                    {
                        quotes++;
                        continue;
                    }
                    if (quotes % 2 == 0) continue;
                    if (quotes < 2) tok1 += x;
                    else tok2 += x;
                }

                localMap.Add(tok1, tok2);
            }
            lineIndex++;
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
