using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CodeTraverser;

public class CodeCanvasDialogue : MonoBehaviour
{
    private static Dialogue.Node GetDefaultNode()
    {
        var node = new Dialogue.Node();
        node.nextNodes = new List<int>();
        node.useSpeakerColor = true;
        return node;
    }

    public static void ParseDialogue(int lineIndex, int charIndex,
         string[] lines, Dictionary<FileCoord, FileCoord> stringScopes,
        Dictionary<string, string> localMap, Dictionary<string, Dialogue> dialogues, out FileCoord coord)
    {
        var dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.nodes = new List<Dialogue.Node>();
        nextID = 0;
        ParseDialogueHelper(charIndex, CodeTraverser.GetScope(lineIndex, lines, stringScopes, out coord), dialogue, localMap);
        dialogues["dial"] = dialogue;
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(dialogue, "Assets/DebugDialogue.asset");
#endif
    }

    // TODO: Add property inheritance to child nodes like speaker ID, typing speed, color etc
    private static int ParseDialogueHelper(int index, string line, Dialogue dialogue, Dictionary<string, string> localMap, string responseText = null)
    {
        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return -1;
        }
        
        var node = GetDefaultNode();
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
            var nextComma = false;
            var val = "";
            if (lineSubstr.StartsWith("dialogueID="))
            {
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("speakerID="))
            {
                val = lineSubstr.Split(",")[0].Split("=")[1];
                node.speakerID = val;
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("dialogueText="))
            {
                val = lineSubstr.Split(",")[0].Split("=")[1];
                node.text = localMap[val];
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("ID="))
            {
                val = lineSubstr.Split(",")[0].Split("=")[1];
                Debug.LogWarning("parsing " + val);
                node.ID = int.Parse(val);
                forcedID = true;
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("useSpeakerColor="))
            {
                val = lineSubstr.Split(",")[0].Split("=")[1];
                
                node.useSpeakerColor = val == "true";
                nextComma = true;
            }
            else if (lineSubstr.StartsWith("responses="))
            {
                index = ParseResponses(index, line, dialogue, node.nextNodes, localMap);
            }
            if (nextComma) 
            {
                nextComma = false;
                index += lineSubstr.IndexOf(",");
            }
            index++;
        }

        dialogue.nodes.Add(node);
        
        node.textColor = Color.white;
        var last = dialogue.nodes.Count - 1;
        if (!forcedID) dialogue.nodes[last] = SetNodeID(dialogue, node, nextID);
        else dialogue.nodes[last] = SetNodeID(dialogue, node, node.ID, true);
        return index;
    }

    private static int ParseResponses (int index, string line, Dialogue dialogue, List<int> nextNodes, Dictionary<string, string> localMap)
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
                index = ParseResponse(index, line, dialogue, localMap, nextNodes);
            }
            else index++;
        }
        
        return index;
    }

    private static int ParseResponse (int index, string line, Dialogue dialogue, Dictionary<string, string> localMap, List<int> nextNodes)
    {
        // find the first bracket
        while (index < line.Length && line[index] != '(') index++;
        index++;
        int brackets = 1;

        var node = GetDefaultNode();
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
                    nextMode = false;
                }
                else if (lineSubstr.StartsWith("Dialogue"))
                {
                    insertNode = false;
                    index = ParseDialogueHelper(index, line, dialogue, localMap, responseText);
                    nextNodes.Add(dialogue.nodes[dialogue.nodes.Count - 1].ID);
                    nextMode = false;
                    
                }
                else if (lineSubstr.StartsWith("SetID"))
                {
                    node.nextNodes = new List<int>();
                    node.action = Dialogue.DialogueAction.ForceToNextID;
                    
                    var parse = lineSubstr.Split("SetID(")[1].Replace(" ", "");

                    //parse = parse.Substring(0, parse.IndexOf(")", 0, 1));
                    parse = parse.Substring(0, parse.IndexOf(")"));
                    node.nextNodes.Add(int.Parse(parse));
                    nextMode = false;
                }
                
            }
            index++;
        }

        if (insertNode)
        {
            if (!forcedID) node = SetNodeID(dialogue, node, nextID);
            nextNodes.Add(node.ID);
            dialogue.nodes.Add(node);
            node.textColor = Color.white;
        }
        
        return index;
    }

    private static int nextID = 0;

    private static Dialogue.Node SetNodeID(Dialogue dialogue, Dialogue.Node node, int ID, bool forceReplacement = false)
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
            var index = dialogue.nodes.FindIndex(n => n.ID == ID);
            dialogue.nodes[index] = SetNodeID(dialogue, dialogue.nodes[index], nextID);

            for (int i = 0; i < dialogue.nodes.Count; i++)
            {
                if (dialogue.nodes[i].action == Dialogue.DialogueAction.ForceToNextID) continue;
                if (dialogue.nodes[i].nextNodes.Contains(ID))
                {
                    dialogue.nodes[i].nextNodes[dialogue.nodes[i].nextNodes.FindIndex(x => x == ID)] = dialogue.nodes[index].ID;
                }
            }
        }
        return node;
    }
}
