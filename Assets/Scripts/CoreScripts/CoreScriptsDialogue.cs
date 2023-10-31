using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;

public class CoreScriptsDialogue : MonoBehaviour
{
    private static Dialogue.Node GetDefaultNode()
    {
        var node = new Dialogue.Node();
        node.useLocalMap = true;
        node.nextNodes = new List<int>();
        node.useSpeakerColor = true;
        node.typingSpeedFactor = 1;
        return node;
    }


    public static void ParseDialogue(int lineIndex, int charIndex,
         string[] lines, ScopeParseData data, out FileCoord coord)
    {
        var dialogue = ScriptableObject.CreateInstance<Dialogue>();
        dialogue.nodes = new List<Dialogue.Node>();
        nextID = 0;
        var metadata = new DialogueRecursionMetadata();
        var scope = CoreScriptsManager.GetScope(lineIndex, lines, data.stringScopes, data.commentLines, out coord);
        ParseDialogueHelper(charIndex, scope, dialogue, data.localMap, out metadata, data.tasks, Color.white);
        data.dialogues[metadata.dialogueID] = dialogue;
    }

    private struct DialogueRecursionMetadata
    {
        public int index;
        public string dialogueID;
    }

    // TODO: Remove order-sensitivity on color properties and response parsing
    private static void ParseDialogueHelper(int index, string line, Dialogue dialogue, 
        Dictionary<string, string> localMap, out DialogueRecursionMetadata metadata, Dictionary<string, Task> tasks, 
        Color defaultColor, string responseText = null, bool useSpeakerColor = true, float typingSpeedFactor = 1)
    {
        
        metadata = new DialogueRecursionMetadata();

        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return;
        }
        
        var node = GetDefaultNode();
        node.typingSpeedFactor = typingSpeedFactor;
        bool forcedID = false;
        node.buttonText = responseText;

        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "dialogueID=",
            "entityID=",
            "dialogueText=",
            "nodeID=",
            "color=",
            "useSpeakerColor=",
            "responses=",
            "taskID=",
            "finishTask=",
            "typingSpeedFactor=",
        };

        var skipSettingID = false;
        if (nextID == 0)
        {
            // force the top-level node to have ID 0
            node = SetNodeID(dialogue, node, nextID);
            skipSettingID = true;
        }

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i).Trim();
            
            var name = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out name, out val, true);

            if (lineSubstr.StartsWith("responses="))
            {
                ParseResponses(i, line, dialogue, node.nextNodes, localMap, tasks, defaultColor, useSpeakerColor, typingSpeedFactor);
                continue;
            }

            skipToComma = true;
            if (lineSubstr.StartsWith("dialogueID="))
            {
                metadata.dialogueID = val;
            }
            else if (lineSubstr.StartsWith("entityID="))
            {
                node.speakerID = val;
                node.forceSpeakerChange = true;
            }
            else if (lineSubstr.StartsWith("dialogueText="))
            {
                node.text = val;
            }
            else if (lineSubstr.StartsWith("nodeID="))
            {
                node.ID = int.Parse(val);
                forcedID = true;
            }
            else if (lineSubstr.StartsWith("typingSpeedFactor="))
            {
                node.typingSpeedFactor = float.Parse(val);
                typingSpeedFactor = node.typingSpeedFactor;
            }
            else if (lineSubstr.StartsWith("color="))
            {
                var colorCommaSep = val.Trim().Replace("(", "").Replace(")", "").Split(",");
                var color = new Color32();
                color.r = (byte)int.Parse(colorCommaSep[0]);
                color.g = (byte)int.Parse(colorCommaSep[1]);
                color.b = (byte)int.Parse(colorCommaSep[2]);
                color.a = 255;
                defaultColor = color;
                useSpeakerColor = false;

            }
            else if (lineSubstr.StartsWith("useSpeakerColor="))
            {
                useSpeakerColor = val == "true";
            }
            else if (lineSubstr.StartsWith("taskID="))
            {
                node.task = tasks[val];
            }
            else if (lineSubstr.StartsWith("finishTask="))
            {
                if (val == "true") node.action = Dialogue.DialogueAction.FinishTask;
            }
        }

        node.useSpeakerColor = useSpeakerColor;
        node.textColor = Color.white;
        
        if (!useSpeakerColor) 
        {
            node.textColor = defaultColor;
        }
        dialogue.nodes.Add(node);
        var last = dialogue.nodes.Count - 1;
        
        metadata.index = index;
        if (skipSettingID) return;
        if (!forcedID) dialogue.nodes[last] = SetNodeID(dialogue, node, nextID);
        else dialogue.nodes[last] = SetNodeID(dialogue, node, node.ID, true);
    }

    private static void ParseResponses (int index, string line, Dialogue dialogue, 
        List<int> nextNodes, Dictionary<string, string> localMap, 
        Dictionary<string, Task> tasks, Color defaultColor, bool useSpeakerColor = true, float typingSpeedFactor = 1)
    {
        bool skipToComma = false;
        int brax = 0;
        var stx = new List<string>()
        {
            "Response(",
        };

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i).Trim();
            if (lineSubstr.StartsWith("Response("))
            {
                ParseResponse(i, line, dialogue, localMap, nextNodes, defaultColor, tasks, useSpeakerColor, typingSpeedFactor);
            }
        }
    }

    private static int ParseResponse (int index, string line, 
        Dialogue dialogue, Dictionary<string, string> localMap, List<int> nextNodes, Color defaultColor,
        Dictionary<string, Task> tasks, bool useSpeakerColor = true, float typingSpeedFactor = 1)
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

        index = CoreScriptsManager.GetNextOccurenceInScope(index, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            var lineSubstr = line.Substring(i);

            var name = "";
            var val = "";
            CoreScriptsSequence.GetNameAndValue(lineSubstr, out name, out val);
            if (lineSubstr.StartsWith("responseText="))
            {
                node.buttonText = val;
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
                    ParseDialogueHelper(index, line, dialogue, localMap, out metadata, tasks, defaultColor, responseText, useSpeakerColor, typingSpeedFactor);
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
