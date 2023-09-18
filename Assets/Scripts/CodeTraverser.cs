using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CodeTraverser : MonoBehaviour
{
    private List<string> instructions;
    private Dictionary<string, Dialogue> dialogues;
    private int index;
    private string codePath = System.IO.Path.Combine(Application.streamingAssetsPath, "CodeTest.codecanvas");
    Dictionary<string, string> localMap = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        Parse();
    }

    void Parse()
    {
        string[] lines = System.IO.File.ReadAllLines(codePath);
        int lineIndex = 0;
        dialogues = new Dictionary<string, Dialogue>();
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
            // Check if assignment by checking for an equals sign
            if (tokens.Length > 1 && tokens[1] == "=")
            {
                if (tokens.Length > 2 && tokens[2].StartsWith("Dialogue("))
                {
                    var dialogue = ScriptableObject.CreateInstance<Dialogue>();
                    dialogue.nodes = new List<Dialogue.Node>();
                    nextID = 0;
                    lineIndex = ParseDialogue(lineIndex, lines, dialogue);
                    dialogues.Add(tokens[0], dialogue);
                    
#if UNITY_EDITOR
    UnityEditor.AssetDatabase.CreateAsset(dialogue, "Assets/DebugDialogue.asset");
#endif
                }
                else
                {
                    Debug.LogError("If you assign you must also specify what you are assigning in the same line with the bracket.");
                    break;
                }
            }
            lineIndex++;
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

    int ParseDialogue(int startIndex, string[] lines, Dialogue dialogue, string responseText = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return -1;
        }
        int brackets = 0;
        
        var node = new Dialogue.Node();
        bool forcedID = false;
        node.buttonText = responseText;
        do
        {
            var toks = lines[startIndex].Split(",");
            var pushNode = false;
            foreach (var tok in toks)
            {
                var cleanedTok = tok.Trim();
                brackets += cleanedTok.Count(x => x == '(');
                var closeBracketCount = cleanedTok.Count(x => x == ')');
                brackets -= closeBracketCount;
                if (closeBracketCount > 0) pushNode = true;
                if (cleanedTok.StartsWith("speakerID="))
                {
                    node.speakerID = cleanedTok.Split("speakerID=")[1];
                }
                else if (cleanedTok.StartsWith("dialogueText="))
                {
                    node.text = localMap[cleanedTok.Split("dialogueText=")[1]];
                }
                else if (cleanedTok.StartsWith("ID="))
                {
                    node = SetNodeID(dialogue, node, int.Parse(cleanedTok.Split("ID=")[1]), true);
                    forcedID = true;
                }
                else if (cleanedTok.StartsWith("useSpeakerColor="))
                {
                    node.useSpeakerColor = cleanedTok.Split("useSpeakerColor=")[1] == "true";
                }
                else if (cleanedTok.StartsWith("customColor="))
                {

                }
                else if (cleanedTok.StartsWith("responses="))
                {
                    startIndex = ParseResponses(startIndex, lines, dialogue) - 1;
                }
            }
            if (pushNode)
            {
                if (!forcedID) node = SetNodeID(dialogue, node, nextID);
                forcedID = false;
                dialogue.nodes.Add(node);
            }
            startIndex++;
        }
        while (brackets > 0 && startIndex < lines.Length);
        return startIndex;
    }

    int ParseResponses (int startIndex, string[] lines, Dialogue dialogue)
    {
        int brackets = 0;
        do
        {
            var toks = lines[startIndex].Split(",");
            foreach (var tok in toks)
            {
                var cleanedTok = tok.Trim();
                brackets += cleanedTok.Count(x => x == '[');
                brackets -= cleanedTok.Count(x => x == ']');
                if (cleanedTok.StartsWith("Response("))
                {
                    startIndex = ParseResponse(startIndex, lines, dialogue) - 1;
                }

            }
            startIndex++;
        } while (brackets > 0 && startIndex < lines.Length);
        return startIndex;
    }

    int ParseResponse (int startIndex, string[] lines, Dialogue dialogue)
    {
        int brackets = 0;
        var node = new Dialogue.Node();
        bool insertNode = true;
        var responseText = "";
        bool forcedID = false;
        do
        {
            var toks = lines[startIndex].Split(",");
            foreach (var tok in toks)
            {
                var cleanedTok = tok.Trim();
                var oldBrackets = brackets;
                brackets += cleanedTok.Count(x => x == '(');
                brackets -= cleanedTok.Count(x => x == ')');
                if (cleanedTok.StartsWith("responseText="))
                {
                    node.buttonText = localMap[cleanedTok.Split("responseText=")[1]];
                    responseText = node.buttonText;
                }
                if (cleanedTok.StartsWith("next="))
                {
                    var str = cleanedTok.Split("next=")[1];
                    if (str.StartsWith("End("))
                    {
                        node.action = Dialogue.DialogueAction.Exit;
                        break;
                    }
                    else if (str.StartsWith("Dialogue("))
                    {
                        // TODO: Figure out edge cases with bracket usage which I don't want to be rigid here
                        startIndex = ParseDialogue(startIndex, lines, dialogue, responseText) - 3;
                        insertNode = false;
                        break;
                    }
                    else if (str.StartsWith("SetID("))
                    {
                        node.nextNodes = new List<int>();
                        Debug.LogWarning(str.Split("SetID(")[1].Replace(")", ""));
                        node.action = Dialogue.DialogueAction.ForceToNextID;
                        node.nextNodes.Add(int.Parse(str.Split("SetID(")[1].Replace(")", "")));
                        break;
                    }
                }

            }
            startIndex++;
        } while (brackets > 0 && startIndex < lines.Length);
        if (insertNode)
        {
            Debug.LogWarning("Inserting node");
            if (!forcedID) node = SetNodeID(dialogue, node, nextID);
            dialogue.nodes.Add(node);
        }
        return startIndex;
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
