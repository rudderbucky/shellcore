using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NodeEditorFramework.Standard;
using NodeEditorFramework.IO;
using NodeEditorFramework;

public class DialogueSystem : MonoBehaviour, IDialogueOverrideHandler
{
    public static DialogueSystem Instance { get; private set; }

    public delegate void DialogueDelegate(int answer);
    public static DialogueDelegate OnDialogueEnd;
    public delegate void CancelDelegate();
    public static CancelDelegate OnDialogueCancel;

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
    public static bool isInCutscene = false;

    public enum DialogueStyle
    {
        Remastered,
        Original
    }
    public static DialogueStyle dialogueStyle;

    public bool IsWindowActive()
    {
        return window;
    }
    
    private void Awake()
    {
        Instance = this;
        dialogueStyle = (DialogueStyle)PlayerPrefs.GetInt("DialogueSystem_dialogueStyle", 0); 
    }

    public static List<string> dialogueCanvasPaths = new List<string>();
    public static List<string> speakerIDList = new List<string>();
    public static Dictionary<string, Stack<UnityAction>> interactionOverrides = new Dictionary<string, Stack<UnityAction>>();
    private static List<DialogueTraverser> traversers;
    public static string speakerID;
    private static bool initialized = false;
    public static void InitCanvases()
    {
        interactionOverrides = new Dictionary<string, Stack<UnityAction>>();
        traversers = new List<DialogueTraverser>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < dialogueCanvasPaths.Count; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, dialogueCanvasPaths[i]);
            Debug.Log("Dialogue Canvas path [" + i + "] = " + finalPath);
            var canvas = XMLImport.Import(finalPath) as DialogueCanvas;
            Debug.Log(canvas);
            if (canvas != null)
            {
                traversers.Add(new DialogueTraverser(canvas));
            }
        }

        initialized = true;
    }

    public static bool GetInitialized()
    {
        return initialized;
    }

    public static void StartQuests() {
        Instance.startQuests();
    }

    // Traverse quest graph
    public void startQuests()
    {
        for (int i = 0; i < traversers.Count; i++)
        {
            traversers[i].StartQuest();
        }
    }

    private void Update()
    {
        if(window && speakerPos != null && player && (player.transform.position - ((Vector3)speakerPos)).sqrMagnitude > 100 && !isInCutscene)
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
        if(window) Destroy(window.transform.parent.gameObject);
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
        var exit = background.transform.Find("Exit");
        exit.GetComponent<Button>().onClick.AddListener(() => {
            endDialogue();
        });
        if(isInCutscene) exit.gameObject.SetActive(false);
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

        if(victory) window.transform.Find("Victory").GetComponent<Text>().text = "<color=lime>VICTORY!</color>";
        else window.transform.Find("Victory").GetComponent<Text>().text = "<color=red>DEFEAT</color>";

    }

    public GameObject missionCompleteBoxPrefab;
    public static void ShowMissionComplete(Mission mission, string rewardsText) {
        Instance.showMissionComplete(mission, rewardsText);
    }

    private void showMissionComplete(Mission mission, string rewardsText) {
        // if(window) endDialogue(0);
        speakerPos = null;
        
        //create window
        window = Instantiate(missionCompleteBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        window.transform.SetSiblingIndex(0);

        window.transform.Find("Mission Name").GetComponent<Text>().text = mission.name.ToUpper();
        window.transform.Find("Rank").GetComponent<Text>().text = mission.rank.ToUpper();
        window.transform.Find("Rank").GetComponent<Text>().color = TaskDisplayScript.rankColorsByString[mission.rank];
        window.transform.Find("Rewards").GetComponent<Text>().text = rewardsText;
    }

    public static void ShowDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker = null)
    {
        Instance.showDialogueNode(node, speaker);
    }

    private void showDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker)
    {
        if (window) Destroy(window.transform.parent.gameObject);
        //speakerPos = speaker.transform.position;
        //create window
        speakerPos = null;
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        var exit = background.transform.Find("Exit");
        exit.GetComponent<Button>().onClick.AddListener(() => {
            endDialogue(0, false);
        });
        if(isInCutscene) exit.gameObject.SetActive(false);
        window.OnCancelled.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;
        DialogueViewTransitionIn(speaker);

        // radio image 
        if(speaker)
        {
            window.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null, speaker.faction);
            window.transform.Find("Name").GetComponent<Text>().text = speaker.blueprint.entityName;
        }
        else 
        {
            window.GetComponentInChildren<SelectionDisplayHandler>().gameObject.SetActive(false);
            window.transform.Find("Name").GetComponent<Text>().text = "Unknown Speaker";
        }

        // update speakerPos
        if(speaker) speakerPos = speaker.transform.position;

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
            // Debug.Log(i + "test");
            button.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.PlayClipByID("clip_select", true);
                endDialogue(index + 1, false);// cancel is always first -> start from 1
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
        var exit = background.transform.Find("Exit");
        exit.GetComponent<Button>().onClick.AddListener(() => {
            endDialogue(0);
        });
        if(isInCutscene) exit.gameObject.SetActive(false);
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
            node.declineResponse,
            node.acceptResponse
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

    public void OpenBuilder(Vector3 speakerPos)
    {
        builder.yardPosition = (Vector3)speakerPos;
        builder.Initialize(BuilderMode.Yard, null);
    }

    public void OpenTrader(Vector3 speakerPos, List<EntityBlueprint.PartInfo> traderInventory)
    {
        builder.yardPosition = (Vector3)speakerPos;
        builder.Initialize(BuilderMode.Trader, traderInventory);
    }

    public void OpenWorkshop()
    {
        Debug.LogWarning("Nice try!");
    }

    public void next(Dialogue dialogue, int ID, Entity speaker)
    {
        if(dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("Empty dialogue: " + dialogue.name);
            endDialogue(0, false);
            return;
        }

        if(player.GetIsDead())
        {
            Debug.Log("Dead player");
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
                AudioManager.PlayClipByID("clip_typing", true);
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
                OpenTrader((Vector3)speakerPos, dialogue.traderInventory);
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Yard:
                OpenBuilder((Vector3)speakerPos);
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
            // button.GetComponent<Button>().onClick.AddListener(()=> {  });
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
        // strange behavior here when tasks are checkpointed, look into this when bugs arise
        if(answer == 0) 
        {
            if(OnDialogueCancel != null)
                OnDialogueCancel.Invoke();
            DialogueViewTransitionOut();
        }
        window.playSoundOnClose = soundOnClose;
        window.CloseUI();
        Destroy(window.transform.root.gameObject);
        if (OnDialogueEnd != null)
        {
            // Debug.Log(OnDialogueEnd);
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
                FadeBarIn();
                break;
        }
        
    }

    public void FadeBarIn()
    {
        currentState = DialogueState.In;
        StartCoroutine("BarFadeIn");
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
        if (blackBarTop == null || blackBarBottom == null)
            yield break;
        blackBarTop.gameObject.SetActive(true);
        blackBarBottom.gameObject.SetActive(true);

        float count = blackBarBottom.anchoredPosition.y;
        while(count > 0)
        {
            if(currentState != DialogueState.Out) break;
            count -= 0.1F * blackBarBottom.sizeDelta.y;
            if(!PlayerViewScript.hidingHUD) hudGroup.alpha += 0.1F;
            blackBarTop.anchoredPosition = new Vector2(0, -count);
            blackBarBottom.anchoredPosition = new Vector2(0, count);
            yield return new WaitForSeconds(0.0025F);
        }
        if(currentState == DialogueState.Out) currentState = DialogueState.Idle;
    }

    public void DialogueViewTransitionOut()
    {
        if(!isInCutscene)
        {
            currentState = DialogueState.Out;
            StartCoroutine("BarFadeOut");
        }
    }

    public RectTransform passiveDialogueRect;
    public GameObject passiveDialogueInstancePrefab;
    public RectTransform passiveDialogueContents;
    public Text passiveDialogueText;
    private DialogueState passiveDialogueState = DialogueState.Out;
    public RectTransform passiveDialogueScrollView;
    Queue<(string, string)> passiveMessages = new Queue<(string, string)>();

    public void SlidePassiveDialogueOut()
    {
        passiveDialogueState = DialogueState.Out;
        StartCoroutine("SlideDialogueOut");
    }

    IEnumerator SlidePassiveDialogueIn(Transform transform)
    {
        float count = transform.localScale.y;
        while(count < 1)
        {
            if(passiveDialogueState != DialogueState.In) break;
            count += 0.05F;
            transform.localScale += new Vector3(0, 0.05F);
            yield return new WaitForSeconds(0.0025F);
        }
    }

    IEnumerator FadePassiveDialogueOut()
    {
        float count = passiveDialogueScrollView.localScale.y;
        passiveDialogueState = DialogueState.Out;
        while(count > 0F)
        {
            if(passiveDialogueState != DialogueState.Out) break;
            count -= 0.05F;
            passiveDialogueScrollView.localScale -= new Vector3(0, 0.05F, 0);
            yield return new WaitForSeconds(0.0025F);
        }

        for(int i = 0; i < passiveDialogueContents.childCount; i++)
        {
            Destroy(passiveDialogueContents.GetChild(i).gameObject);
        }
    }

    public void PushPassiveDialogue(string id, string text)
    {
        if(passiveDialogueState != DialogueState.In) passiveDialogueState = DialogueState.In;
        passiveMessages.Enqueue((id, text));
    }
    float queueTimer = 0;

    public void ResetPassiveDialogueQueueTime()
    {
        queueTimer = 0;
    }

    void PassiveDialogueHandler()
    {
        queueTimer -= Time.deltaTime;
        if(passiveMessages.Count > 0)
        {
            passiveDialogueScrollView.localScale = new Vector3(1, 1, 1);
            if(queueTimer <= 0)
            {
                queueTimer = 3;
                var dialogue = passiveMessages.Dequeue();
                var instance = Instantiate(passiveDialogueInstancePrefab, passiveDialogueContents);
                Entity speaker = AIData.entities.Find(e => e.GetID() == dialogue.Item1);
                var name = speaker.name;
                if(speaker as PlayerCore)
                    name = (speaker as PlayerCore).cursave.name;
                instance.transform.Find("Name").GetComponent<Text>().text = name;
                instance.transform.Find("Text").GetComponent<Text>().text = dialogue.Item2;
                instance.transform.localScale -= new Vector3(0, 1);
                StartCoroutine(SlidePassiveDialogueIn(instance.transform));

                instance.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null, speaker.faction);
            }
        }
        else if(queueTimer <= -2 && passiveDialogueState != DialogueState.Out)
        {
            StartCoroutine(FadePassiveDialogueOut());
        }

    }

    public List<string> GetSpeakerIDList()
    {
        return speakerIDList;
    }

    public Dictionary<string, Stack<UnityAction>> GetInteractionOverrides()
    {
        return interactionOverrides;
    }

    public void SetNode(ConnectionPort node)
    {
        TaskManager.Instance.setNode(node);
    }

    public void SetNode(Node node)
    {
        TaskManager.Instance.setNode(node);
    }

    public void SetSpeakerID(string ID)
    {
        speakerID = ID;
    }

    public void SetCanvasPath(string path)
    {
        Debug.Log("Found Dialogue Path " + dialogueCanvasPaths.Count);
        dialogueCanvasPaths.Add(path);
    }

    public static Entity GetSpeaker() {
        var speakerObj = SectorManager.instance.GetEntity(speakerID);
        return speakerObj;
    }
}
