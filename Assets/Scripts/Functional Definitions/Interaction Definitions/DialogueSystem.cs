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
    public GameObject taskDialogueBoxPrefab;
    public GameObject dialogueButtonPrefab;

    public GameObject battleResultsBoxPrefab;
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
    public PlayerCore player;
    Vector3? speakerPos = null;
    public CoreUpgraderScript upgraderScript;

    public enum DialogueStyle
    {
        Remastered,
        Original
    }
    public static DialogueStyle dialogueStyle;
    private void Awake()
    {
        Instance = this;
        dialogueStyle = (DialogueStyle)PlayerPrefs.GetInt("DialogueSystem_dialogueStyle", 0); 
    }

    private void Update()
    {
        if(window && speakerPos != null && player && (player.transform.position - ((Vector3)speakerPos)).sqrMagnitude > 100)
        {
            endDialogue();
        }

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

        PassiveDialogueHandler();
    }

    public static void StartDialogue(Dialogue dialogue, Entity speaker = null)
    {
        Instance.startDialogue(dialogue, speaker);
    }

    public static void ShowPopup(string text, Color color, Entity speaker = null)
    {
        Instance.showPopup(text, color, speaker);
    }

    public static void ShowPopup(string text)
    {
        Instance.showPopup(text, Color.white);
    }

    public GameObject popupBoxPrefab;
    private void showPopup(string text, Color color, Entity speaker = null)
    {
        if (window && window.GetActive())
            return;
        if(window) endDialogue();
        //create window
        speakerPos = null;

        if(!speaker)
            window = Instantiate(popupBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        else 
        {
            window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
            speakerPos = speaker.transform.position;
        }

        window.Activate();
        window.transform.SetSiblingIndex(0);
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(()=> {
            endDialogue();
            // ResourceManager.PlayClipByID("clip_select", false);
        });
        window.OnCancelled.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        // radio image 
        if(speaker) {
            DialogueViewTransitionIn(speaker);
            window.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null);
            window.transform.Find("Name").GetComponent<Text>().text = speaker.blueprint.entityName;
        }

        // change text
        this.text = text.Replace("<br>", "\n");
        if(speaker)
        {
            characterCount = 0;
            nextCharacterTime = (float) (Time.time + timeBetweenCharacters);
        } else 
        {
            textRenderer.text = this.text;
            characterCount = text.Length;
        }

        textRenderer.color = color;

        // ok button
        RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
        button.SetParent(background, false);
        button.anchoredPosition = new Vector2(0, 24);
        button.GetComponent<Button>().onClick.AddListener(() => { endDialogue(); });
        button.Find("Text").GetComponent<Text>().text = "Ok";

        buttons = new GameObject[1];
        buttons[0] = button.gameObject;
        if(speaker)
            AudioManager.PlayClipByID("clip_typing");
    }

    public static void ShowBattleResults(bool victory) {
        Instance.showBattleResults(victory);
    }

    private void showBattleResults(bool victory) {
        if(window) endDialogue(0);
        speakerPos = null;
        
        //create window
        window = Instantiate(battleResultsBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        window.transform.SetSiblingIndex(0);

        if(victory) window.transform.Find("Victory").gameObject.SetActive(true);
        else window.transform.Find("Defeat").gameObject.SetActive(true);
    }

    public static void ShowDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker = null)
    {
        Instance.showDialogueNode(node, speaker);
    }

    private void showDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker)
    {
        if (window) endDialogue(0);
        //speakerPos = speaker.transform.position;
        //create window
        speakerPos = null;
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() => {
            endDialogue(0);
        });
        window.OnCancelled.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        DialogueViewTransitionIn(speaker);

        // radio image 
        if(speaker)
        {
            window.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null);
            window.transform.Find("Name").GetComponent<Text>().text = speaker.blueprint.entityName;
        }
        else 
        {
            window.GetComponentInChildren<SelectionDisplayHandler>().gameObject.SetActive(false);
            window.transform.Find("Name").GetComponent<Text>().text = "Unknown Speaker";
        }

        // update speakerPos
        if(speaker) speakerPos = speaker.transform.position;
        Debug.Log(speakerPos + " " + speaker);

        if(speaker)
            AudioManager.PlayClipByID("clip_typing");
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
            button.anchoredPosition = new Vector2(0, 24 + 24 * (node.answers.Count - (i + 1)));
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => {
                endDialogue(index + 1, index != 0);// cancel is always first -> start from 1
            });
            button.Find("Text").GetComponent<Text>().text = node.answers[i];

            buttons[i] = button.gameObject;
        }
    }

    public static void ShowTaskPrompt(NodeEditorFramework.Standard.StartTaskNode node, Entity speaker = null)
    {
        Instance.showTaskPrompt(node, speaker);
    }

    private void showTaskPrompt(NodeEditorFramework.Standard.StartTaskNode node, Entity speaker) //TODO: reward part image
    {
        if (window) endDialogue(0, false);
        //speakerPos = speaker.transform.position;
        //create window

        speakerPos = null;
        window = Instantiate(taskDialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() => {
            endDialogue(0);
        });
        window.OnCancelled.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        DialogueViewTransitionIn(speaker);

        AudioManager.PlayClipByID("clip_select", true); // task button cannot create a noise because it launches endDialogue()
                                                     // so cover for its noise here
        AudioManager.PlayClipByID("clip_typing", false);
        // change text
        text = node.dialogueText.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters);
        textRenderer.color = node.dialogueColor;

        // update speakerPos
        if(speaker) speakerPos = speaker.transform.position;

        // Objective list
        var objectiveList = background.transform.Find("ObjectiveList").GetComponent<Text>();
        objectiveList.text = node.objectiveList;

        background.transform.Find("Credit Reward Text").GetComponent<Text>().text =
        "Credit reward: " + node.creditReward;

        background.transform.Find("Reputation Reward Text").GetComponent<Text>().text =
        "Reputation reward: " + node.reputationReward;
        // Part reward
        if(node.partReward)
        {
            // Part image:
            PartBlueprint blueprint = ResourceManager.GetAsset<PartBlueprint>(node.partID);
            if(!blueprint)
            {
                Debug.LogWarning("Part reward of Start Task node not found!");
            }
            var partImage = background.transform.Find("Part").GetComponent<Image>();
            partImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.spriteID);
            partImage.rectTransform.sizeDelta = partImage.sprite.bounds.size * 45;
            partImage.color = Color.green;

            // Ability image:
            if(node.partAbilityID > 0)
            {
                var backgroudBox = background.transform.Find("backgroundbox");
                var abilityIcon = backgroudBox.Find("Ability").GetComponent<Image>();
                var tierIcon = backgroudBox.Find("Tier").GetComponent<Image>();
                var type = backgroudBox.Find("Type").GetComponent<Text>();
                var abilityTooltip = backgroudBox.GetComponent<AbilityButtonScript>();

                abilityIcon.sprite = AbilityUtilities.GetAbilityImageByID(node.partAbilityID, null);
                tierIcon.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + Mathf.Clamp(node.partTier, 1, 3));
                type.text = AbilityUtilities.GetAbilityNameByID(node.partAbilityID, null) + (node.partTier > 0 ? " " + node.partTier : "");
                string description = "";
                description += AbilityUtilities.GetAbilityNameByID(node.partAbilityID, null) + (node.partTier > 0 ? " " + node.partTier : "") + "\n";
                description += AbilityUtilities.GetDescriptionByID(node.partAbilityID, node.partTier, null);
                abilityTooltip.abilityInfo = description;
            }
            else
            {
                background.transform.Find("backgroundbox").gameObject.SetActive(false);
            }
        }
        else
        {
            background.transform.Find("Part").GetComponent<Image>().enabled = false;
            background.transform.Find("backgroundbox").gameObject.SetActive(false);
        }

        // create buttons
        buttons = new GameObject[2];

        string[] answers =
        {
            "I need some time to prepare",
            "I'm on my way"
        };

        for (int i = 0; i < answers.Length; i++)
        {
            //TODO: createButton()
            RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
            button.SetParent(background, false);
            button.anchoredPosition = new Vector2(0, 24 + 24 * i/*(node.outputKnobs.Count - (i + 1))*/);
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => {
                if(index == 1) 
                {
                    DialogueViewTransitionOut(); 
                    SectorManager.instance.player.alerter.showMessage("New Task", "clip_victory");
                    endDialogue(index, false);
                } else endDialogue(index, true);
            });
            button.Find("Text").GetComponent<Text>().text = answers[i];

            buttons[i] = button.gameObject;
        }
    }

    private void startDialogue(Dialogue dialogue, Entity speaker)
    {
        if(window) endDialogue();
        speakerPos = speaker.transform.position;
        //create window
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();

        DialogueViewTransitionIn(speaker);

        background = window.transform.Find("Background").GetComponent<RectTransform>();
        background.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() => { endDialogue(); });
        window.OnCancelled.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        next(dialogue, 0, speaker);
    }

    public static void Next(Dialogue dialogue, int ID, Entity speaker)
    {
        Instance.next(dialogue, ID, speaker);
    }

    public void next(Dialogue dialogue, int ID, Entity speaker)
    {
        if(dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("Empty dialogue: " + dialogue.name);
            endDialogue(0, false);
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
                endDialogue(0, false);
                if(speaker.faction != player.faction) {
                    return;
                }
                if(((Vector3)speakerPos - player.transform.position).magnitude < dialogue.vendingBlueprint.range) {
                    vendorUI.SetVendor(speaker as IVendor, player);
                    vendorUI.openUI();
                }
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Shop:
			    builder.yardPosition = (Vector3)speakerPos;
			    builder.Initialize(BuilderMode.Trader, dialogue.traderInventory);
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Yard:
			    builder.yardPosition = (Vector3)speakerPos;
			    builder.Initialize(BuilderMode.Yard, null);
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Exit:
                endDialogue(0, true);
                return;
            case Dialogue.DialogueAction.Workshop:
                workshop.yardPosition = (Vector3)speakerPos;
                workshop.InitializeSelectionPhase();
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Upgrader:
                upgraderScript.initialize();
                endDialogue(0, false);
                return;
            default:
                break;
        }

        // radio image 
        window.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null);
        window.transform.Find("Name").GetComponent<Text>().text = speaker.blueprint.entityName;

        AudioManager.PlayClipByID("clip_typing");
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
                Next(dialogue, nextIndex, speaker);
            });
            if(dialogue.nodes[nextIndex].action != Dialogue.DialogueAction.Exit) {
                button.GetComponent<Button>().onClick.AddListener(()=> {
                    AudioManager.PlayClipByID("clip_select", true); 
                    // need condition to ensure no sound clashes occur
                });
            }
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

    private void endDialogue(int answer = 0, bool soundOnClose = true)
    {
        if(answer == 0) DialogueViewTransitionOut();
        window.playSoundOnClose = soundOnClose;
        window.CloseUI();
        Destroy(window.transform.root.gameObject);
        if (OnDialogueEnd != null)
        {
            Debug.Log(OnDialogueEnd);
            OnDialogueEnd.Invoke(answer);
        }
    }

    // black bars
    public RectTransform blackBarTop;
    public RectTransform blackBarBottom;
    public CanvasGroup hudGroup;
    private enum DialogueState
    {
        In,
        Out,
        Idle
    }
    private DialogueState currentState = DialogueState.Idle;
    private void DialogueViewTransitionIn(Entity speaker = null)
    {
        currentState = DialogueState.In;
        var windowRect = window.GetComponent<RectTransform>();
        switch(dialogueStyle)
        {
            case DialogueStyle.Original:
                windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5F, 0.5F);
                windowRect.anchoredPosition = new Vector2(0, 0);
                break;
            case DialogueStyle.Remastered:
            default:
                if(speaker && player)
                {
                    if(player.transform.position.y <= speaker.transform.position.y)
                    {
                        windowRect.anchorMin = new Vector2(0, 0);
                        windowRect.anchorMax = new Vector2(1, 0);
                        windowRect.anchoredPosition = new Vector2(0, 200);
                    }
                    else
                    {
                        windowRect.anchorMin = new Vector2(0, 1);
                        windowRect.anchorMax = new Vector2(1, 1);
                        windowRect.anchoredPosition = new Vector2(0, -200);
                    }

                }
                StartCoroutine("BarFadeIn");
                break;
        }
        
    }
    IEnumerator BarFadeIn()
    {
        blackBarTop.gameObject.SetActive(true);
        blackBarBottom.gameObject.SetActive(true);

        float count = blackBarBottom.anchoredPosition.y;
        while(count < blackBarBottom.sizeDelta.y)
        {
            if(currentState != DialogueState.In) break;
            count += 0.1F * blackBarBottom.sizeDelta.y;
            hudGroup.alpha -= 0.1F;
            blackBarTop.anchoredPosition = new Vector2(0, -count);
            blackBarBottom.anchoredPosition = new Vector2(0, count);
            yield return new WaitForSeconds(0.0025F);
        }
        if(currentState == DialogueState.In) currentState = DialogueState.Idle;
    }

    IEnumerator BarFadeOut()
    {
        blackBarTop.gameObject.SetActive(true);
        blackBarBottom.gameObject.SetActive(true);

        float count = blackBarBottom.anchoredPosition.y;
        while(count > 0)
        {
            if(currentState != DialogueState.Out) break;
            count -= 0.1F * blackBarBottom.sizeDelta.y;
            hudGroup.alpha += 0.1F;
            blackBarTop.anchoredPosition = new Vector2(0, -count);
            blackBarBottom.anchoredPosition = new Vector2(0, count);
            yield return new WaitForSeconds(0.0025F);
        }
        if(currentState == DialogueState.Out) currentState = DialogueState.Idle;
    }

    public void DialogueViewTransitionOut()
    {
        currentState = DialogueState.Out;
        StartCoroutine("BarFadeOut");
    }

    public RectTransform passiveDialogueRect;
    public Text passiveDialogueText;
    private DialogueState passiveDialogueState = DialogueState.Out;

    public void SlidePassiveDialogueIn()
    {
        passiveDialogueState = DialogueState.In;
        StartCoroutine("SlideDialogueIn");
    }

    public void SlidePassiveDialogueOut()
    {
        passiveDialogueState = DialogueState.Out;
        StartCoroutine("SlideDialogueOut");
    }

    IEnumerator SlideDialogueIn()
    {
        float count = passiveDialogueRect.sizeDelta.y;
        while(count > 0)
        {
            if(passiveDialogueState != DialogueState.In) break;
            count -= 10F;
            passiveDialogueRect.anchoredPosition += 10 * Vector2.up;
            yield return new WaitForSeconds(0.0025F);
        }
    }

    IEnumerator SlideDialogueOut()
    {
        float count = passiveDialogueRect.sizeDelta.y;
        while(count > 0)
        {
            if(passiveDialogueState != DialogueState.Out) break;
            count -= 10F;
            passiveDialogueRect.anchoredPosition -= 10 * Vector2.up;
            yield return new WaitForSeconds(0.0025F);
        }
        passiveDialogueText.text = "";
    }

    public void PushPassiveDialogue(string text)
    {
        if(passiveDialogueState != DialogueState.In) SlidePassiveDialogueIn();
        passiveMessages.Enqueue(text);
    }

    Queue<string> passiveMessages = new Queue<string>();
    float queueTimer = 0;
    void PassiveDialogueHandler()
    {
        queueTimer -= Time.deltaTime;
        if(passiveMessages.Count > 0)
        {
            if(queueTimer <= 0)
            {
                queueTimer = 3;
                passiveDialogueText.text += "\n" + passiveMessages.Dequeue();
            }
        }
        else if(queueTimer <= -2 && passiveDialogueState != DialogueState.Out)
        {
            SlidePassiveDialogueOut();
        }

    }
}
