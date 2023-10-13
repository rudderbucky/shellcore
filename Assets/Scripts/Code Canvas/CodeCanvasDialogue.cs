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

    // TODO: force top-of-stack dialogue to be ID 0
    public static void ParseDialogue(int lineIndex, int charIndex,
         string[] lines, Dictionary<FileCoord, FileCoord> stringScopes,
        Dictionary<string, string> localMap, Dictionary<string, Dialogue> dialogues, Dictionary<string, Task> tasks, out FileCoord coord)
    {
        var dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.nodes = new List<Dialogue.Node>();
        nextID = 0;
        var metadata = new DialogueRecursionMetadata();
        var scope = CodeTraverser.GetScope(lineIndex, lines, stringScopes, out coord);
        ParseDialogueHelper(charIndex, scope, dialogue, localMap, out metadata, tasks);
        dialogues[metadata.dialogueID] = dialogue;
//#if UNITY_EDITOR
//       UnityEditor.AssetDatabase.CreateAsset(dialogue, "Assets/DebugDialogue.asset");
//#endif
    }

    private struct DialogueRecursionMetadata
    {
        public int index;
        public string dialogueID;
    }

    // TODO: Add property inheritance to child nodes like speaker ID, typing speed, color etc
    private static void ParseDialogueHelper(int index, string line, Dialogue dialogue, 
        Dictionary<string, string> localMap, out DialogueRecursionMetadata metadata, Dictionary<string, Task> tasks, string responseText = null)
    {
        
        metadata = new DialogueRecursionMetadata();

        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return;
        }
        
        var node = GetDefaultNode();
        bool forcedID = false;
        node.buttonText = responseText;

        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "dialogueID=",
            "speakerID=",
            "dialogueText=",
            "nodeID=",
            "useSpeakerColor=",
            "responses=",
            "taskID="
        };

        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i).Trim();
            
            var name = "";
            var val = "";
            CodeCanvasSequence.GetNameAndValue(lineSubstr, out name, out val);

            if (lineSubstr.StartsWith("responses="))
            {
                ParseResponses(i, line, dialogue, node.nextNodes, localMap, tasks);
                continue;
            }

            skipToComma = true;
            if (lineSubstr.StartsWith("dialogueID="))
            {
                metadata.dialogueID = val;
            }
            else if (lineSubstr.StartsWith("speakerID="))
            {
                node.speakerID = val;
                node.forceSpeakerChange = true;
            }
            else if (lineSubstr.StartsWith("dialogueText="))
            {
                node.text = localMap[val];
            }
            else if (lineSubstr.StartsWith("nodeID="))
            {
                node.ID = int.Parse(val);
                forcedID = true;
            }
            else if (lineSubstr.StartsWith("useSpeakerColor="))
            {
                node.useSpeakerColor = val == "true";
            }
            else if (lineSubstr.StartsWith("taskID="))
            {
                node.task = tasks[val];
            }
        }

        dialogue.nodes.Add(node);
        
        node.textColor = Color.white;
        var last = dialogue.nodes.Count - 1;
        if (!forcedID) dialogue.nodes[last] = SetNodeID(dialogue, node, nextID);
        else dialogue.nodes[last] = SetNodeID(dialogue, node, node.ID, true);
        metadata.index = index;
    }

    private static void ParseResponses (int index, string line, Dialogue dialogue, List<int> nextNodes, Dictionary<string, string> localMap, Dictionary<string, Task> tasks)
    {
        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "Response(",
        };

        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i).Trim();
            if (lineSubstr.StartsWith("Response("))
            {
                ParseResponse(i, line, dialogue, localMap, nextNodes, tasks);
            }
        }
    }

    private static int ParseResponse (int index, string line, Dialogue dialogue, Dictionary<string, string> localMap, List<int> nextNodes, Dictionary<string, Task> tasks)
    {
        
        var node = GetDefaultNode();
        var responseText = "";
        bool insertNode = true;
        bool forcedID = false;

        
        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "responseText=",
            "next="
        };

        index = CodeTraverser.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CodeTraverser.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i);

            var name = "";
            var val = "";
            CodeCanvasSequence.GetNameAndValue(lineSubstr, out name, out val);
            if (lineSubstr.StartsWith("responseText="))
            {
                node.buttonText = localMap[val];
                responseText = node.buttonText;
            }
            else if (lineSubstr.StartsWith("next="))
            {
                val = val.Trim();
                if (val.StartsWith("End"))
                {
                    node.action = Dialogue.DialogueAction.Exit;
                }
                else if (val.StartsWith("Call"))
                {
                    node.action = Dialogue.DialogueAction.Call;
                    node.functionID = val.Replace("Call(", "").Replace(")", "").Trim();
                }
                else if (val.StartsWith("Dialogue"))
                {
                    insertNode = false;
                    DialogueRecursionMetadata metadata;
                    ParseDialogueHelper(index, line, dialogue, localMap, out metadata, tasks, responseText);
                    index = metadata.index;
                    nextNodes.Add(dialogue.nodes[dialogue.nodes.Count - 1].ID);
                    
                }
                else if (val.StartsWith("SetID"))
                {
                    node.nextNodes = new List<int>();
                    node.action = Dialogue.DialogueAction.ForceToNextID;
                    
                    var parse = val.Replace("SetID(", "").Replace(")", "").Trim();
                    node.nextNodes.Add(int.Parse(parse));
                }
                
            }
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
