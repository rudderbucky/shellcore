using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;
using static CoreScriptsSequence;

public class CoreScriptsDialogue : MonoBehaviour
{
    private static Dialogue.Node GetDefaultNode()
    {
        var node = new Dialogue.Node();
        node.coreScriptsMode = true;
        node.nextNodes = new List<int>();
        node.useSpeakerColor = true;
        node.typingSpeedFactor = 1;
        return node;
    }

    private static (string, Dialogue, DialogueRecursionMetadata, DialoguePropertyMetadata) ParseDialogueCore(int lineIndex, int charIndex,
         string[] lines, ScopeParseData data, out FileCoord coord)
         {
            var dialogue = ScriptableObject.CreateInstance<Dialogue>();
            dialogue.nodes = new List<Dialogue.Node>();
            nextID = 0;
            var metadata = new DialogueRecursionMetadata();
            var scope = CoreScriptsManager.GetScope(lineIndex, lines, data.stringScopes, data.commentLines, out coord);
            var propertyMetadata = new DialoguePropertyMetadata();
            propertyMetadata.defaultColor = Color.white;
            propertyMetadata.useSpeakerColor = true;
            propertyMetadata.typingSpeedFactor = 1;
            return (scope, dialogue, metadata, propertyMetadata);
         }

    public static void ParseDialogue(int lineIndex, int charIndex,
         string[] lines, ScopeParseData data, out FileCoord coord)
    {
        var tup = ParseDialogueCore(lineIndex, charIndex, lines, data, out coord);
        var scope = tup.Item1;
        var dialogue = tup.Item2;
        var metadata = tup.Item3;
        var propertyMetadata = tup.Item4;
        ParseDialogueHelper(charIndex, scope, dialogue, data.localMap, out metadata, data.tasks, propertyMetadata);
        data.dialogues[metadata.dialogueID] = dialogue;
    }

    public static void ParseDialogueShortened(int lineIndex, int charIndex,
         string[] lines, ScopeParseData data, out FileCoord coord)
    {
        var tup = ParseDialogueCore(lineIndex, charIndex, lines, data, out coord);
        var scope = tup.Item1;
        var dialogue = tup.Item2;
        var metadata = tup.Item3;
        var propertyMetadata = tup.Item4;
        var allNodes = new List<Dialogue.Node>();
        ParseDialogueShortenedHelper(charIndex, scope, dialogue, data.localMap, out metadata, data.tasks, propertyMetadata, allNodes);
        data.dialogues[metadata.dialogueID] = dialogue;
        PlayerCore.Instance.dialogue = dialogue;
    }




    private struct DialogueRecursionMetadata
    {
        // used to find where to parse after parsing an internal dialogue node within an external
        public int index;
        public string dialogueID;
    }

    private struct DialoguePropertyMetadata
    {
        public Color defaultColor;
        public bool useSpeakerColor;
        public float typingSpeedFactor;
        public bool concealName;
    }

    public static Color ParseColor(string val)
    {
        var colorCommaSep = val.Trim().Replace("(", "").Replace(")", "").Split(",");
        var color = new Color32();
        color.r = (byte)int.Parse(colorCommaSep[0]);
        color.g = (byte)int.Parse(colorCommaSep[1]);
        color.b = (byte)int.Parse(colorCommaSep[2]);
        color.a = 255;
        return color;
    }

    // TODO: Remove order-sensitivity on color properties and response parsing
    private static void ParseDialogueHelper(int index, string line, Dialogue dialogue, 
        Dictionary<string, string> localMap, out DialogueRecursionMetadata metadata, Dictionary<string, Task> tasks, 
        DialoguePropertyMetadata data, string responseText = null)
    {
        
        metadata = new DialogueRecursionMetadata();

        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return;
        }
        
        var node = GetDefaultNode();
        node.typingSpeedFactor = data.typingSpeedFactor;
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
            "concealName="
        };

        var skipSettingID = false;
        if (nextID == 0)
        {
            // force the top-level node to have ID 0
            node = SetNodeID(dialogue.nodes, node, nextID);
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
                ParseResponses(i, line, dialogue, node.nextNodes, localMap, tasks, data);
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
                node.typingSpeedFactor = float.Parse(val, CultureInfo.InvariantCulture);
                data.typingSpeedFactor = node.typingSpeedFactor;
            }
            else if (lineSubstr.StartsWith("color="))
            {
                var color = ParseColor(val);
                data.defaultColor = color;
                data.useSpeakerColor = false;
            }
            else if (lineSubstr.StartsWith("useSpeakerColor="))
            {
                data.useSpeakerColor = val == "true";
            }
            else if (lineSubstr.StartsWith("taskID="))
            {
                node.task = tasks[val];
            }
            else if (lineSubstr.StartsWith("finishTask="))
            {
                if (val == "true") node.action = Dialogue.DialogueAction.FinishTask;
            }
            else if (lineSubstr.StartsWith("concealName="))
            {
                data.concealName = val == "true";
            }
        }

        node.concealName = data.concealName;
        node.useSpeakerColor = data.useSpeakerColor;
        node.textColor = Color.white;
        
        if (!data.useSpeakerColor) 
        {
            node.textColor = data.defaultColor;
        }
        dialogue.nodes.Add(node);
        var last = dialogue.nodes.Count - 1;
        
        metadata.index = index;
        if (skipSettingID) return;
        if (!forcedID) dialogue.nodes[last] = SetNodeID(dialogue.nodes, node, nextID);
        else dialogue.nodes[last] = SetNodeID(dialogue.nodes, node, node.ID, true);
    }

    private static void ParseResponses (int index, string line, Dialogue dialogue, 
        List<int> nextNodes, Dictionary<string, string> localMap, 
        Dictionary<string, Task> tasks, DialoguePropertyMetadata data)
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
                ParseResponse(i, line, dialogue, localMap, nextNodes, tasks, data);
            }
        }
    }

    private static int ParseResponse (int index, string line, 
        Dialogue dialogue, Dictionary<string, string> localMap, List<int> nextNodes,
        Dictionary<string, Task> tasks, DialoguePropertyMetadata data)
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
                    ParseDialogueHelper(index, line, dialogue, localMap, out metadata, tasks, data, responseText);
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
                else if (val.StartsWith("Yard"))
                {
                    node.action = Dialogue.DialogueAction.Yard;
                }
                else if (val.StartsWith("Trader"))
                {
                    node.action = Dialogue.DialogueAction.Shop;
                    try
                    {
                        var parse = val.Replace("Trader(", "").Replace(")", "").Trim();
                        var inventory = JsonUtility.FromJson<ShipBuilder.TraderInventory>(CoreScriptsManager.instance.GetLocalMapString(parse));
                        dialogue.traderInventory = inventory.parts;
                    }
                    catch
                    {

                    }
                }
                else if (val.StartsWith("Upgrader"))
                {
                    node.action = Dialogue.DialogueAction.Upgrader;
                }
                else if (val.StartsWith("Workshop"))
                {
                    node.action = Dialogue.DialogueAction.Workshop;
                }
                else if (val.StartsWith("Fusion"))
                {
                    node.action = Dialogue.DialogueAction.Fusion;
                }
            }
        }

        if (insertNode)
        {
            if (!forcedID) node = SetNodeID(dialogue.nodes, node, nextID);
            nextNodes.Add(node.ID);
            dialogue.nodes.Add(node);
            node.textColor = Color.white;
        }
        
        return index;
    }

    private static int nextID = 0;

    private static Dialogue.Node SetNodeID(List<Dialogue.Node> allNodes, Dialogue.Node node, int ID, bool forceReplacement = false)
    {
        if (allNodes.Exists(n => n.ID == ID) && !forceReplacement)
        {
            if (ID == nextID) nextID++;
            ID = nextID;
        }
        node.ID = ID;
        if (nextID == ID) nextID++;
        if (allNodes.Exists(n => n.ID == ID) && forceReplacement)
        {
            var index = allNodes.FindIndex(n => n.ID == ID);
            allNodes[index] = SetNodeID(allNodes, allNodes[index], nextID);

            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].action == Dialogue.DialogueAction.ForceToNextID) continue;
                if (allNodes[i].nextNodes.Contains(ID))
                {
                    allNodes[i].nextNodes[allNodes[i].nextNodes.FindIndex(x => x == ID)] = allNodes[index].ID;
                }
            }
        }
        return node;
    }

    
private static void ParseDialogueShortenedHelper(int index, string line, Dialogue dialogue, 
        Dictionary<string, string> localMap, out DialogueRecursionMetadata metadata, Dictionary<string, Task> tasks, 
        DialoguePropertyMetadata data, List<Dialogue.Node> allNodes, string responseText = null)
    {
        
        metadata = new DialogueRecursionMetadata();

        if (dialogue == null)
        {
            Debug.LogError("Null dialogue passed while parsing.");
            return;
        }
        
        var node = GetDefaultNode();
        allNodes.Add(node);
        node.typingSpeedFactor = data.typingSpeedFactor;
        bool forcedID = false;
        node.buttonText = responseText;

        bool skipToComma = false;
        int brax = 0;
        List<string> stx = null;

        /*
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
            "concealName="
        */

        var skipSettingID = false;
        if (nextID == 0)
        {
            // force the top-level node to have ID 0
            node = SetNodeID(allNodes, node, nextID, true);
            skipSettingID = true;
        }


        line = GetValueScopeWithinLine(line, index);
        index = CoreScriptsManager.GetNextOccurenceInScope(0, line, stx, ref brax, ref skipToComma, '(', ')');
        var argIndex = 0;
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i).Trim();
            if (lineSubstr.StartsWith("R("))
            {
                ParseResponseShortened(i, line, dialogue, localMap, node.nextNodes, tasks, data, allNodes);
                continue;
            }

            int bx = 0;
            var val = "";
            for (int ii = 0; ii < lineSubstr.Length; ii++)
            {
                if (lineSubstr[ii] == '(') bx++;
                if (lineSubstr[ii] == ')') bx--;
                if (lineSubstr[ii] == ',' && bx == 0)
                {
                    val = lineSubstr.Substring(0, ii);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(val))
            {
                switch (argIndex)
                {
                    case 0:
                        node.text = val;
                        break;
                    case 1:
                        node.speakerID = val;
                        node.forceSpeakerChange = true;
                        break;
                    case 2:
                        metadata.dialogueID = val;
                        break;
                    case 3:  
                        node.ID = int.Parse(val);
                        forcedID = true;
                        break;
                    case 4:                    
                        node.task = tasks[val];
                        break;
                    case 5:
                        if (val == "true") node.action = Dialogue.DialogueAction.FinishTask;
                        break;
                    case 6:
                        node.typingSpeedFactor = float.Parse(val, CultureInfo.InvariantCulture);
                        data.typingSpeedFactor = node.typingSpeedFactor;
                        break;
                    case 7:
                        var color = ParseColor(val);
                        data.defaultColor = color;
                        data.useSpeakerColor = false;
                        break;
                }
            }  
            argIndex++;
        }


        node.concealName = data.concealName;
        node.useSpeakerColor = data.useSpeakerColor;
        node.textColor = Color.white;
        
        if (!data.useSpeakerColor) 
        {
            node.textColor = data.defaultColor;
        }
        dialogue.nodes.Add(node);
        var last = dialogue.nodes.Count - 1;
        
        metadata.index = index;
        if (skipSettingID) return;
        if (!forcedID) dialogue.nodes[last] = SetNodeID(allNodes, node, nextID);
        else dialogue.nodes[last] = SetNodeID(allNodes, node, node.ID, true);
    }

    private static int ParseResponseShortened (int index, string line, 
        Dialogue dialogue, Dictionary<string, string> localMap, List<int> nextNodes,
        Dictionary<string, Task> tasks, DialoguePropertyMetadata data, List<Dialogue.Node> allNodes)
    {
        
        var node = GetDefaultNode();
        allNodes.Add(node);
        var responseText = "";
        bool insertNode = true;
        bool forcedID = false;

        
        bool skipToComma = false;
        int brax = 0;
        List<string> stx = null;
        int argIndex = 0;

        var queueDialogue = false;

        line = GetValueScopeWithinLine(line, index);
        index = CoreScriptsManager.GetNextOccurenceInScope(0, line, stx, ref brax, ref skipToComma, '(', ')');
        for (int i = index; i < line.Length; i = CoreScriptsManager.GetNextOccurenceInScope(i, line, stx, ref brax, ref skipToComma, '(', ')'))
        {
            skipToComma = true;
            var lineSubstr = line.Substring(i);
            //Debug.LogWarning(lineSubstr);

            if (lineSubstr.StartsWith("D("))
            {
                if (argIndex > 0 && lineSubstr.StartsWith("D("))
                {
                    insertNode = false;
                    DialogueRecursionMetadata metadata;
                    ParseDialogueShortenedHelper(index, line, dialogue, localMap, out metadata, tasks, data, allNodes, responseText);
                    index = metadata.index;
                    nextNodes.Add(dialogue.nodes[dialogue.nodes.Count - 1].ID);
                    queueDialogue = false;
                    continue;
                }
                else queueDialogue = true;
                continue;
            }

            var val = lineSubstr.Split(')')[0];
            val = val.Split(',')[0].Trim();            
            switch (argIndex)
            {
                case 0:    
                    node.buttonText = val;
                    responseText = node.buttonText;
                    break;
                case 1:
                    if (val.StartsWith("End"))
                    {
                        node.action = Dialogue.DialogueAction.Exit;
                    }
                    else if (val.StartsWith("Call"))
                    {
                        node.action = Dialogue.DialogueAction.Call;
                        node.functionID = val.Replace("Call(", "").Replace(")", "").Trim();
                    }
                    else if (val.StartsWith("SetID"))
                    {
                        node.nextNodes = new List<int>();
                        node.action = Dialogue.DialogueAction.ForceToNextID;
                        
                        var parse = val.Replace("SetID(", "").Replace(")", "").Trim();
                        node.nextNodes.Add(int.Parse(parse));
                    }
                    else if (val.StartsWith("Yard"))
                    {
                        node.action = Dialogue.DialogueAction.Yard;
                    }
                    else if (val.StartsWith("Trader"))
                    {
                        node.action = Dialogue.DialogueAction.Shop;
                        try
                        {
                            var parse = val.Replace("Trader(", "").Replace(")", "").Trim();
                            var inventory = JsonUtility.FromJson<ShipBuilder.TraderInventory>(CoreScriptsManager.instance.GetLocalMapString(parse));
                            dialogue.traderInventory = inventory.parts;
                        }
                        catch
                        {

                        }
                    }
                    else if (val.StartsWith("Upgrader"))
                    {
                        node.action = Dialogue.DialogueAction.Upgrader;
                    }
                    else if (val.StartsWith("Workshop"))
                    {
                        node.action = Dialogue.DialogueAction.Workshop;
                    }
                    else if (val.StartsWith("Fusion"))
                    {
                        node.action = Dialogue.DialogueAction.Fusion;
                    }
                    break;

            }
            argIndex++;
        }
        
        if (queueDialogue)
        {
            insertNode = false;
            DialogueRecursionMetadata metadata;
            ParseDialogueShortenedHelper(index, line, dialogue, localMap, out metadata, tasks, data, allNodes, responseText);
            index = metadata.index;
            nextNodes.Add(dialogue.nodes[dialogue.nodes.Count - 1].ID);
        }

        if (insertNode)
        {
            if (!forcedID) node = SetNodeID(allNodes, node, nextID);
            nextNodes.Add(node.ID);
            dialogue.nodes.Add(node);
            node.textColor = Color.white;
        }

        return index;
    }
}