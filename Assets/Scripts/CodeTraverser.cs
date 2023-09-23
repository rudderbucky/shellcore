using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class CodeTraverser : MonoBehaviour
{
    private List<string> instructions;
    private Dictionary<string, Dialogue> dialogues;
    private int index;
    private string codePath = System.IO.Path.Combine(Application.streamingAssetsPath, "CodeTest.codecanvas");
    Dictionary<string, string> localMap = new Dictionary<string, string>();
    private Dictionary<int, string> dialogueScopes = new Dictionary<int, string>();
    private Dictionary<int, string> responseListScopes = new Dictionary<int, string>();
    private Dictionary<int, string> responseScopes = new Dictionary<int, string>();
    private struct FileCoord
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

    private string GetScope(int startLineNum, string[] lines)
    {
        int bCount = 0;
        var searchStarted = false;
        var builder = new StringBuilder();
        
        for (FileCoord d = new FileCoord(startLineNum, 0); d.line < lines.Length; d = StringSensitiveIterator(d, lines))
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

    FileCoord StringSensitiveIterator(FileCoord current, string[] lines)
    {
        if (current.line >= lines.Length) return current;
        if (stringScopes.ContainsKey(current))
        {
            current = new FileCoord(stringScopes[current].line, stringScopes[current].character + 1);
        }

        var lineChange = false;

        while (current.character >= lines[current.line].Length - 1)
        {
            lineChange = true;
            current.line++;
            current.character = 0;
            if (current.line >= lines.Length) return current;
        }
        
        if (!lineChange) 
        {
            current.character++;
        }
        // Debug.LogWarning(current.line + " " + current.character + " " + lines[current.line].Length);
        return current;
    }

    void Parse()
    {
        string[] lines = System.IO.File.ReadAllLines(codePath);
        GetStringScopes(lines);
        dialogues = new Dictionary<string, Dialogue>();
        
        SetUpLocalMap(lines);

        var equalsMode = false;

        for (FileCoord d = new FileCoord(); d.line < lines.Length; d = StringSensitiveIterator(d, lines))
        {
            var i = d.line;
            var c = d.character;
            if (equalsMode)
            {
                if (lines[i][c] == ' ')
                {
                    continue;
                }
                else if (lines[i].Substring(c).StartsWith("Dialogue"))
                {
                    var dialogue = ScriptableObject.CreateInstance<Dialogue>();
                    dialogue.nodes = new List<Dialogue.Node>();
                    ParseDialogue(c, GetScope(i, lines), dialogue);
#if UNITY_EDITOR
    UnityEditor.AssetDatabase.CreateAsset(dialogue, "Assets/DebugDialogue.asset");
#endif
                    break;
                }
                else
                {
                    equalsMode = false;
                }
            }
            else if (lines[i][c] == '=')
            {
                equalsMode = true;
            }
        }
    }

    int nextID = 0;

    private Dialogue.Node SetNodeID(Dialogue dialogue, Dialogue.Node node, int ID, bool forceReplacement = false)
    {
        if (dialogue.nodes.Exists(n => n.ID == ID) && !forceReplacement)
        {
            if (ID == nextID) nextID++;
            ID = nextID;
        }
        node.ID = ID;
        if (nextID == ID) nextID++;
        if (dialogue.nodes.Exists(n => n.ID == ID) && forceReplacement)
        {
            var n = dialogue.nodes.Find(n => n.ID == ID);
            SetNodeID(dialogue, n, nextID);
        }
        return node;
    }

    int ParseDialogue(int index, string line, Dialogue dialogue, string responseText = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return -1;
        }
        
        var node = new Dialogue.Node();
        bool forcedID = false;
        node.buttonText = responseText;

        // find the first bracket
        while (index < line.Length && line[index] != '(') index++;
        index++;
        int brackets = 1;

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
            var val = lineSubstr.Split(",")[0].Split("=")[1];
            var nextComma = false;
            if (lineSubstr.StartsWith("speakerID="))
            {
                node.speakerID = val;
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("dialogueText="))
            {
                node.text = localMap[val];
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("ID="))
            {
                Debug.LogWarning(lineSubstr);
                node.ID = int.Parse(val);
                forcedID = true;
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("useSpeakerColor="))
            {
                node.useSpeakerColor = val == "true";
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("responses="))
            {
                index = ParseResponses(index, line, dialogue);
                nextComma = true;
            }
            if (nextComma) 
            {
                nextComma = false;
                index += lineSubstr.IndexOf(",");
            }
            index++;
        }
    
        if (!forcedID) node = SetNodeID(dialogue, node, nextID);
        forcedID = false;
        dialogue.nodes.Add(node);
        return index;
    }

    int ParseResponses (int index, string line, Dialogue dialogue)
    {
        // find the first bracket
        while (index < line.Length && line[index] != '[') index++;
        index++;
        int brackets = 1;

        while (index < line.Length && brackets > 0)
        {
            if (line[index] == '[')
            {
                brackets++;
            }
            else if (line[index] == ']')
            {
                brackets--;
            }

            var lineSubstr = line.Substring(index);
            if (lineSubstr.StartsWith("Response("))
            {
                index = ParseResponse(index, line, dialogue);
            }
            else index++;
        }
        return index;
    }

    int ParseResponse (int index, string line, Dialogue dialogue)
    {
        // find the first bracket
        while (index < line.Length && line[index] != '(') index++;
        index++;
        int brackets = 1;

        var node = new Dialogue.Node();
        bool insertNode = true;
        var responseText = "";
        bool forcedID = false;
        bool nextMode = true;


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
            if (lineSubstr.StartsWith("responseText="))
            {
                var val = lineSubstr.Split(",")[0].Split("=")[1];
                node.buttonText = localMap[val];
                responseText = node.buttonText;
            }
            else if (lineSubstr.StartsWith("next="))
            {
                nextMode = true;
            }
            else if (nextMode)
            {
                if (lineSubstr.StartsWith("End"))
                {
                    node.action = Dialogue.DialogueAction.Exit;
                }
                else if (lineSubstr.StartsWith("Dialogue"))
                {
                    insertNode = false;
                    index = ParseDialogue(index, line, dialogue, responseText);
                    
                }
                else if (lineSubstr.StartsWith("SetID"))
                {
                    node.nextNodes = new List<int>();
                    Debug.LogWarning(lineSubstr.Split("SetID(")[1].Replace(")", ""));
                    node.action = Dialogue.DialogueAction.ForceToNextID;
                    node.nextNodes.Add(int.Parse(lineSubstr.Split("SetID(")[1].Replace(")", "")));
                }
                
                nextMode = false;
            }
            index++;
        }

        if (insertNode)
        {
            if (!forcedID) node = SetNodeID(dialogue, node, nextID);
            dialogue.nodes.Add(node);
            Debug.LogWarning("Inserted node: " + dialogue.nodes.Count + " " + dialogue.GetHashCode());
        }
        
        return index;
    }



    void Tick()
    {
        while (index < instructions.Count && Execute(instructions[index]))
        {
            index++;
        }
    }

    bool Execute(string instruction)
    {
        return false;
    }
}
