using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    public delegate void DialogueDelegate(int answer);
    public static DialogueDelegate OnDialogueEnd;

    public GameObject dialogueBoxPrefab;
    public GameObject dialogueButtonPrefab;
    public Font shellcorefont;
    GUIWindowScripts window;
    RectTransform background;
    Text textRenderer;
    GameObject[] buttons;
    public ShipBuilder builder;
    public DroneWorkshop workshop;
    public VendorUI vendorUI;
    int characterCount = 0;
    float nextCharacterTime;
    public double timeBetweenCharacters = 0.0175d;
    string text = "";
    Transform playerTransform;
    Vector3? speakerPos;
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(window && speakerPos != null && playerTransform && (playerTransform.position - ((Vector3)speakerPos)).sqrMagnitude > 200)
            endDialogue();
        // Add text
        if(textRenderer && characterCount < text.Length)
        {
            if(Time.time > nextCharacterTime)
            {
                characterCount++;
                nextCharacterTime = (float) (Time.time + timeBetweenCharacters);
                textRenderer.text = text.Substring(0, characterCount);
            }
        }
    }

    public static void StartDialogue(Dialogue dialogue, Entity speaker = null, PlayerCore player = null)
    {
        Instance.startDialogue(dialogue, speaker, player);
    }

    public static void ShowPopup(string text, Color color)
    {
        Instance.showPopup(text, color);
    }

    public static void ShowPopup(string text)
    {
        Instance.showPopup(text, Color.white);
    }

    private void showPopup(string text, Color color)
    {
        if (window && window.GetActive())
            return;
        if(window) endDialogue();
        playerTransform = null;
        //create window

        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        window.transform.SetSiblingIndex(0);
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(()=> { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        // change text
        this.text = text.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float) (Time.time + timeBetweenCharacters);
        textRenderer.color = color;

        // ok button
        RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
        button.SetParent(background, false);
        button.anchoredPosition = new Vector2(0, 24);
        button.GetComponent<Button>().onClick.AddListener(() => { endDialogue(); });
        button.Find("Text").GetComponent<Text>().text = "Ok";

        buttons = new GameObject[1];
        buttons[0] = button.gameObject;
    }

    public static void ShowDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker = null, PlayerCore player = null)
    {
        Instance.showDialogueNode(node, speaker, player);
    }

    private void showDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker, PlayerCore player)
    {
        if (window) endDialogue(0);
        playerTransform = player ? player.transform : null;
        //speakerPos = speaker.transform.position;
        //create window
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() => {
            endDialogue(0);
            ResourceManager.PlayClipByID("clip_select", false);
        });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        ResourceManager.PlayClipByID("clip_typing");
        // change text
        text = node.text.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters);
        textRenderer.color = node.textColor;

        // create buttons
        buttons = new GameObject[node.answers.Count];
        
        for (int i = 0; i < node.answers.Count; i++)
        {
            RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
            button.SetParent(background, false);
            button.anchoredPosition = new Vector2(0, 24 + 16 * (node.answers.Count - (i + 1)));
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => {
                endDialogue(index + 1);// cancel is always first -> start from 1
                ResourceManager.PlayClipByID("clip_select", false);
            });
            button.Find("Text").GetComponent<Text>().text = node.answers[i];

            buttons[i] = button.gameObject;
        }
    }

    public static void ShowTaskPrompt(NodeEditorFramework.Standard.StartTaskNode node, Entity speaker = null, PlayerCore player = null)
    {
        Instance.showTaskPrompt(node, speaker, player);
    }

    private void showTaskPrompt(NodeEditorFramework.Standard.StartTaskNode node, Entity speaker, PlayerCore player) //TODO: reward part image
    {
        if (window) endDialogue(0);
        playerTransform = player ? player.transform : null;
        //speakerPos = speaker.transform.position;
        //create window
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() => {
            endDialogue(0);
            ResourceManager.PlayClipByID("clip_select", false);
        });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        ResourceManager.PlayClipByID("clip_typing");
        // change text
        text = node.description.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters);
        textRenderer.color = Color.white; ;

        // create buttons
        buttons = new GameObject[2];

        string[] answers =
        {
            "I need some time to prepare",
            "I'll do it!"
        };

        for (int i = 0; i < answers.Length; i++)
        {
            //TODO: createButton()
            RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
            button.SetParent(background, false);
            button.anchoredPosition = new Vector2(0, 24 + 16 * i/*(node.outputKnobs.Count - (i + 1))*/);
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => {
                endDialogue(index);
                ResourceManager.PlayClipByID("clip_select", false);
            });
            button.Find("Text").GetComponent<Text>().text = answers[i];

            buttons[i] = button.gameObject;
        }
    }

    private void startDialogue(Dialogue dialogue, Entity speaker, PlayerCore player)
    {
        if(window) endDialogue();
        playerTransform = player ? player.transform : null;
        speakerPos = speaker.transform.position;
        //create window
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        next(dialogue, 0, speaker, player);
    }

    public static void Next(Dialogue dialogue, int ID, Entity speaker, PlayerCore player)
    {
        Instance.next(dialogue, ID, speaker, player);
    }

    public void next(Dialogue dialogue, int ID, Entity speaker, PlayerCore player)
    {
        if(dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("Empty dialogue: " + dialogue.name);
            endDialogue();
            return;
        }

        // clear everything
        if(buttons != null)
            for(int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i]);
            }

        // find dialogue index
        int currentIndex = getNodeIndex(dialogue, ID);
        if(currentIndex == -1)
        {
            Debug.LogWarning("Missing node '" + ID + "' in " + dialogue.name);
            endDialogue();
            return;
        }
        Dialogue.Node current = dialogue.nodes[currentIndex];

        // check if the node has an action
        switch (current.action)
        {
            case Dialogue.DialogueAction.None:
                //Do nothing and continue after this check
                break;
            case Dialogue.DialogueAction.Outpost:
                if(speaker.faction != player.faction) {
                    endDialogue();
                    ResourceManager.PlayClipByID(null);
                    return;
                }
                if(((Vector3)speakerPos - player.transform.position).magnitude < dialogue.vendingBlueprint.range) {
                    vendorUI.blueprint = dialogue.vendingBlueprint;
                    vendorUI.outpostPosition = (Vector3)speakerPos;
                    vendorUI.player = player;
                    vendorUI.openUI();
                }
                endDialogue();
                ResourceManager.PlayClipByID(null);
                return;
            case Dialogue.DialogueAction.Shop:
			    builder.yardPosition = (Vector3)speakerPos;
			    builder.Initialize(BuilderMode.Trader, dialogue.traderInventory);
                endDialogue();
                ResourceManager.PlayClipByID(null);
                return;
            case Dialogue.DialogueAction.Yard:
			    builder.yardPosition = (Vector3)speakerPos;
			    builder.Initialize(BuilderMode.Yard, null);
                endDialogue();
                ResourceManager.PlayClipByID(null);
                return;
            case Dialogue.DialogueAction.Exit:
                endDialogue();
                return;
            case Dialogue.DialogueAction.Workshop:
                workshop.yardPosition = (Vector3)speakerPos;
                workshop.InitializeSelectionPhase();
                endDialogue();
                ResourceManager.PlayClipByID(null);
                return;
            default:
                break;
        }

        ResourceManager.PlayClipByID("clip_typing");
        // change text
        text = current.text.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float) (Time.time + timeBetweenCharacters);
        textRenderer.color = current.textColor;

        // create buttons
        buttons = new GameObject[current.nextNodes.Count];

        for (int i = 0; i < current.nextNodes.Count; i++)
        {
            int nextIndex = getNodeIndex(dialogue, current.nextNodes[i]);
            if (nextIndex == -1)
            {
                Debug.LogWarning("Missing node '" + current.nextNodes[i] + "' in " + dialogue.name);
                endDialogue();
                return;
            }
            Dialogue.Node next = dialogue.nodes[nextIndex];

            RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
            button.SetParent(background, false);
            button.anchoredPosition = new Vector2(0, 24 + 16 * (current.nextNodes.Count - (i + 1)));
            button.GetComponent<Button>().onClick.AddListener(()=> {
                Next(dialogue, nextIndex, speaker, player);
                ResourceManager.PlayClipByID("clip_select", false);
            });
            button.GetComponent<Button>().onClick.AddListener(()=> {  });
            button.Find("Text").GetComponent<Text>().text = next.buttonText;

            buttons[i] = button.gameObject;
        }
    }

    private int getNodeIndex(Dialogue dialogue, int ID)
    {
        for (int i = 0; i < dialogue.nodes.Count; i++)
        {
            if (dialogue.nodes[i].ID == ID)
            {
                return i;
            }
        }
        return -1;
    }

    private void endDialogue(int answer = 0)
    {
        window.ToggleActive();
        Destroy(window.transform.root.gameObject);
        Debug.Log(answer);
        if (OnDialogueEnd != null)
            OnDialogueEnd.Invoke(answer);
    }
}
