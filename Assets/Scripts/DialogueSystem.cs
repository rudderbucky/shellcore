using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    public GameObject dialogueBoxPrefab;
    public GameObject dialogueButtonPrefab;

    GameObject window;
    RectTransform backgroud;
    Text textRenderer;
    GameObject[] buttons;

    int characterCount = 0;
    float nextCharacterTime;
    public float timeBetweenCharacters = 0.02f;
    string text = "";

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Add text
        if(textRenderer && characterCount < text.Length)
        {
            if(Time.time > nextCharacterTime)
            {
                characterCount++;
                nextCharacterTime = Time.time + timeBetweenCharacters;
                textRenderer.text = text.Substring(0, characterCount);
            }
        }
    }

    public static void StartDialogue(Dialogue dialogue)
    {
        Instance.startDialogue(dialogue);
    }

    private void startDialogue(Dialogue dialogue)
    {
        //create window
        window = Instantiate(dialogueBoxPrefab);
        backgroud = window.transform.Find("Background").GetComponent<RectTransform>();
        backgroud.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(endDialogue);
        textRenderer = backgroud.transform.Find("Text").GetComponent<Text>();

        next(dialogue, 0);
    }

    public static void Next(Dialogue dialogue, int ID)
    {
        Instance.next(dialogue, ID);
    }

    public void next(Dialogue dialogue, int ID)
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
                //TODO: create outpost
                break;
            case Dialogue.DialogueAction.Shop:
                //TODO: create shop
                endDialogue();
                return;
            case Dialogue.DialogueAction.Yard:
                //TODO: create yard
                endDialogue();
                return;
            case Dialogue.DialogueAction.Exit:
                endDialogue();
                return;
            default:
                break;
        }

        // change text
        text = current.text.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = Time.time + timeBetweenCharacters;
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
            button.SetParent(backgroud, false);
            button.anchoredPosition = new Vector2(0, 24 + 16 * (current.nextNodes.Count - (i + 1)));
            button.GetComponent<Button>().onClick.AddListener(()=> { Next(dialogue, nextIndex); });
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

    private void endDialogue()
    {
        Destroy(window);
    }
}
