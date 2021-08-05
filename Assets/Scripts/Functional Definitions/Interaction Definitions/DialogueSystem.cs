using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.IO;
using NodeEditorFramework.Standard;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

///
/// This class manages dialogue windows as well as dialogue traversers/canvases.
///
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
    public GameObject rewardBoxPrefab;
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
    BattleZoneManager battleZoneManager;

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
        isInCutscene = false;
        CameraScript.panning = false;
    }

    public static List<string> dialogueCanvasPaths = new List<string>();
    public static List<string> speakerIDList = new List<string>();
    public static Dictionary<string, Stack<UnityAction>> interactionOverrides = new Dictionary<string, Stack<UnityAction>>();
    private static List<DialogueTraverser> traversers;
    public static string speakerID;
    private static bool initialized = false;

    // Bugfixes dialogue canvas paths persisting post world reload in the WC
    public static void ClearStatics()
    {
        if (dialogueCanvasPaths != null)
        {
            dialogueCanvasPaths.Clear();
        }

        if (speakerIDList != null)
        {
            speakerIDList.Clear();
        }

        if (interactionOverrides != null)
        {
            interactionOverrides.Clear();
        }

        if (traversers != null)
        {
            traversers.Clear();
        }
    }

    public static void InitCanvases()
    {
        interactionOverrides = new Dictionary<string, Stack<UnityAction>>();
        traversers = new List<DialogueTraverser>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        if (Instance)
        {
            Instance.ClearCanvases();
        }

        var XMLImport = new XMLImportExport();

        for (int i = 0; i < dialogueCanvasPaths.Count; i++)
        {
            string finalPath = System.IO.Path.Combine(Application.streamingAssetsPath, dialogueCanvasPaths[i]);
            //Debug.Log("Dialogue Canvas path [" + i + "] = " + finalPath);
            var canvas = XMLImport.Import(finalPath) as DialogueCanvas;
            //Debug.Log(canvas);
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

    public static void StartQuests()
    {
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
        if (window && speakerPos != null && player && (player.transform.position - ((Vector3)speakerPos)).sqrMagnitude > 100 && !isInCutscene)
        {
            endDialogue();
        }

        // Add text
        if (textRenderer && characterCount < text.Length)
        {
            if (Time.time > nextCharacterTime)
            {
                characterCount++;
                nextCharacterTime = (float)(Time.time + timeBetweenCharacters);
                textRenderer.text = text.Substring(0, characterCount);
            }
        }
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

    ///
    /// Instantiates the given prefab and sets it up as a window.
    ///
    private void CreateWindow(GameObject prefab, string text, Color color, Entity speaker)
    {
        if (window && window.transform.parent)
        {
            Destroy(window.transform.parent.gameObject);
        }

        //create window
        speakerPos = null;

        if (speaker)
        {
            speakerPos = speaker.transform.position;
        }

        window = Instantiate(prefab).GetComponentInChildren<GUIWindowScripts>();


        window.Activate();
        window.transform.SetSiblingIndex(0);
        background = window.transform.Find("Background").GetComponent<RectTransform>();
        var exit = background.transform.Find("Exit");
        exit.GetComponent<Button>().onClick.AddListener(() => { endDialogue(); });
        if (isInCutscene)
        {
            exit.gameObject.SetActive(false);
        }

        window.OnCancelled.AddListener(() => { endDialogue(); });
        textRenderer = background.transform.Find("Text").GetComponent<Text>();
        textRenderer.font = shellcorefont;

        // radio image 
        if (window.GetComponentInChildren<SelectionDisplayHandler>())
        {
            if (speaker)
            {
                DialogueViewTransitionIn(speaker);
                window.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null, speaker.faction);
                window.transform.Find("Name").GetComponent<Text>().text = speaker.blueprint.entityName;
            }
            else
            {
                window.GetComponentInChildren<SelectionDisplayHandler>().gameObject.SetActive(false);
                window.transform.Find("Name").GetComponent<Text>().text = "Unknown Speaker";
            }
        }

        // change text
        if (text != null)
            this.text = text.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters);

        characterCount = 0;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters);

        textRenderer.color = color;

        if (speaker)
        {
            AudioManager.PlayClipByID("clip_typing");
        }
    }

    ///
    /// Creates and returns a button with the passed text, action call, and y position.
    ///
    private GameObject CreateButton(string text, UnityAction call, int ypos)
    {
        RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
        button.SetParent(background, false);
        button.anchoredPosition = new Vector2(0, ypos);
        if (call != null)
        {
            button.GetComponent<Button>().onClick.AddListener(call);
        }

        button.Find("Text").GetComponent<Text>().text = text;

        return button.gameObject;
    }

    public GameObject popupBoxPrefab;

    private void showPopup(string text, Color color, Entity speaker = null)
    {
        CreateWindow(speaker ? dialogueBoxPrefab : popupBoxPrefab, text, color, speaker);

        if (!speaker)
        {
            textRenderer.text = this.text;
            characterCount = text.Length;
        }

        buttons = new GameObject[1];
        buttons[0] = CreateButton("Ok", () => { endDialogue(); }, 24);
    }

    public static void ShowBattleResults(bool victory)
    {
        Instance.showBattleResults(victory);
    }

    private void showBattleResults(bool victory)
    {
        if (window)
        {
            endDialogue(0);
        }

        speakerPos = null;

        //create window
        window = Instantiate(battleResultsBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.DestroyOnClose = true;
        window.Activate();
        window.transform.SetSiblingIndex(0);

        if (victory)
        {
            window.transform.Find("Victory").GetComponent<Text>().text = "<color=lime>VICTORY!</color>";
        }
        else
        {
            window.transform.Find("Victory").GetComponent<Text>().text = "<color=red>DEFEAT</color>";
        }

        battleZoneManager = FindObjectOfType<BattleZoneManager>();
        if (!battleZoneManager)
        {
            return;
        }

        string[] stats = battleZoneManager.GetStats();
        RectTransform scrollArea = window.transform.Find("Background/ViewBorder/Scroll View/Viewport/Content").GetComponent<RectTransform>();
        GameObject prefab = scrollArea.Find("Stats").gameObject;
        for (int i = 0; i < stats.Length; i++)
        {
            var obj = Instantiate(prefab);
            obj.name = "Faction_" + i;
            RectTransform rt = obj.GetComponent<RectTransform>();
            Text text = obj.GetComponent<Text>();

            rt.SetParent(scrollArea);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(120f + 80f * i, 10f);
            rt.offsetMax = new Vector2(128f + 80f * (i + 1), -10f);

            text.text = stats[i];
        }

        scrollArea.sizeDelta = new Vector2(120f + 80f * stats.Length, scrollArea.sizeDelta.y);
    }

    public GameObject missionCompleteBoxPrefab;

    public static void ShowMissionComplete(Mission mission, string rewardsText)
    {
        Instance.showMissionComplete(mission, rewardsText);
    }

    private void showMissionComplete(Mission mission, string rewardsText)
    {
        // if(window) endDialogue(0);
        speakerPos = null;

        //create window
        window = Instantiate(missionCompleteBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        window.transform.SetSiblingIndex(0);

        if (mission.name.Length <= 33)
        {
            window.transform.Find("Holder").Find("Mission Name").GetComponent<Text>().text = mission.name.ToUpper();
        }
        else
        {
            window.transform.Find("Holder").Find("Mission Name").GetComponent<Text>().text = mission.name.ToUpper().Substring(0, 30) + "...";
        }

        window.transform.Find("Rank").GetComponent<Text>().text = mission.rank.ToUpper();
        window.transform.Find("Rank").GetComponent<Text>().color = TaskDisplayScript.rankColorsByString[mission.rank];
        window.transform.Find("Holder").Find("Rewards").GetComponent<Text>().text = rewardsText;
    }

    public static void ShowDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker = null)
    {
        Instance.showDialogue(node.text, node.answers, speaker, node.textColor, node.useEntityColor);
    }

    public static void ShowFinishTaskNode(NodeEditorFramework.Standard.FinishTaskNode node, Entity speaker = null)
    {
        if (node.answers == null)
        {
            node.answers = new List<string>();
            node.answers.Add("Ok");
        }

        Instance.showDialogue(node.rewardText, node.answers, speaker, node.textColor, node.useEntityColor);
    }

    private void showDialogue(string text, List<string> answers, Entity speaker, Color textColor, bool useEntityColor = true)
    {
        CreateWindow(dialogueBoxPrefab, text, useEntityColor && speaker ? FactionManager.GetFactionColor(speaker.faction) : textColor, speaker);
        DialogueViewTransitionIn(speaker);

        // create buttons
        buttons = new GameObject[answers.Count];

        for (int i = 0; i < answers.Count; i++)
        {
            int index = i;
            buttons[i] = CreateButton(answers[i], () =>
            {
                AudioManager.PlayClipByID("clip_select", true);
                endDialogue(index + 1, false); // cancel is always first -> start from 1
            }, 24 + 24 * (answers.Count - (i + 1)));
        }
    }

    public static void ShowTaskPrompt(NodeEditorFramework.Standard.StartTaskNode node, Entity speaker = null)
    {
        Instance.showTaskPrompt(node, speaker);
    }

    private void SetupRewards(GameObject gameObject, RewardWrapper wrapper)
    {
        gameObject.transform.Find("Credit Reward Text").GetComponent<Text>().text =
            "Credit reward: " + wrapper.creditReward;

        gameObject.transform.Find("Reputation Reward Text").GetComponent<Text>().text =
            "Reputation reward: " + wrapper.reputationReward;
        // Part reward
        if (wrapper.partReward)
        {
            // Part image:
            PartBlueprint blueprint = ResourceManager.GetAsset<PartBlueprint>(wrapper.partID);
            if (!blueprint)
            {
                Debug.LogWarning("Part reward of Start Task wrapper not found!");
            }

            var partImage = gameObject.transform.Find("Part").GetComponent<Image>();
            partImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.spriteID);
            partImage.rectTransform.sizeDelta = partImage.sprite.bounds.size * 45;
            partImage.color = Color.green;

            // Ability image:
            if (wrapper.partAbilityID > 0)
            {
                var backgroudBox = gameObject.transform.Find("backgroundbox");
                var abilityIcon = backgroudBox.Find("Ability").GetComponent<Image>();
                var tierIcon = backgroudBox.Find("Tier").GetComponent<Image>();
                var type = backgroudBox.Find("Type").GetComponent<Text>();
                var abilityTooltip = backgroudBox.GetComponent<AbilityButtonScript>();

                abilityIcon.sprite = AbilityUtilities.GetAbilityImageByID(wrapper.partAbilityID, wrapper.partSecondaryData);
                if (wrapper.partTier >= 1)
                {
                    tierIcon.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + Mathf.Clamp(wrapper.partTier, 1, 3));
                }
                else
                {
                    tierIcon.enabled = false;
                }

                type.text = AbilityUtilities.GetAbilityNameByID(wrapper.partAbilityID, null) + (wrapper.partTier > 0 ? " " + wrapper.partTier : "");
                string description = "";
                description += AbilityUtilities.GetAbilityNameByID(wrapper.partAbilityID, null) + (wrapper.partTier > 0 ? " " + wrapper.partTier : "") + "\n";
                description += AbilityUtilities.GetDescriptionByID(wrapper.partAbilityID, wrapper.partTier, null);
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
    }

    public static void ShowReward(RewardWrapper wrapper)
    {
        Instance.showReward(wrapper);
    }

    private void showReward(RewardWrapper wrapper)
    {
        if (window)
        {
            endDialogue(0, false);
        }

        //create window
        window = Instantiate(rewardBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();
        window.transform.SetSiblingIndex(0);

        SetupRewards(window.gameObject, wrapper);
    }

    private void showTaskPrompt(NodeEditorFramework.Standard.StartTaskNode node, Entity speaker)
    {
        if (window)
        {
            endDialogue(0, false);
        }

        CreateWindow(taskDialogueBoxPrefab, node.dialogueText, node.useEntityColor && speaker ? FactionManager.GetFactionColor(speaker.faction) : node.dialogueColor, speaker);
        DialogueViewTransitionIn(speaker);
        AudioManager.PlayClipByID("clip_select", true); // task button cannot create a noise because it launches endDialogue()
        // so cover for its noise here

        // Objective list
        var objectiveList = background.transform.Find("ObjectiveList").GetComponent<Text>();
        objectiveList.text = node.objectiveList;

        var wrapper = new RewardWrapper();
        wrapper.creditReward = node.creditReward;
        wrapper.partAbilityID = node.partAbilityID;
        wrapper.partReward = node.partReward;
        wrapper.partSecondaryData = node.partSecondaryData;
        wrapper.partTier = node.partTier;
        wrapper.reputationReward = node.reputationReward;
        wrapper.shardReward = node.shardReward;
        wrapper.partID = node.partID;

        SetupRewards(background.gameObject, wrapper);

        /*
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

                abilityIcon.sprite = AbilityUtilities.GetAbilityImageByID(node.partAbilityID, node.partSecondaryData);
                if(node.partTier >= 1)
                    tierIcon.sprite = ResourceManager.GetAsset<Sprite>("AbilityTier" + Mathf.Clamp(node.partTier, 1, 3));
                else tierIcon.enabled = false;
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
        }*/

        string[] answers =
        {
            node.declineResponse,
            node.acceptResponse
        };

        // create buttons
        buttons = new GameObject[answers.Length];

        for (int i = 0; i < answers.Length; i++)
        {
            //TODO: createButton()
            int index = i;
            buttons[i] = CreateButton(answers[i], () =>
            {
                if (index == 1)
                {
                    DialogueViewTransitionOut();
                    SectorManager.instance.player.alerter.showMessage("New Task", "clip_victory");
                    endDialogue(index, false);
                }
                else
                {
                    endDialogue(index, true);
                }
            }, 24 + 24 * i);
        }
    }

    private void startDialogue(Dialogue dialogue, Entity speaker)
    {
        if (window)
        {
            endDialogue();
        }

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
        if (dialogue.nodes.Count == 0)
        {
            Debug.LogWarning("Empty dialogue: " + dialogue.name);
            endDialogue(0, false);
            return;
        }

        if (player.GetIsDead())
        {
            Debug.Log("Dead player");
            endDialogue(0, false);
            return;
        }

        // clear everything
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i]);
            }
        }

        // find dialogue index
        int currentIndex = getNodeIndex(dialogue, ID);
        if (currentIndex == -1)
        {
            Debug.LogWarning($"Missing node '{ID}' in {dialogue.name}");
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
                if (speaker.faction != player.faction)
                {
                    return;
                }

                if (((Vector3)speakerPos - player.transform.position).magnitude < dialogue.vendingBlueprint.range)
                {
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
        window.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null, speaker.faction);
        window.transform.Find("Name").GetComponent<Text>().text = speaker.blueprint.entityName;

        // change text
        text = current.text.Replace("<br>", "\n");
        characterCount = 0;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters);
        textRenderer.color = current.textColor;

        // create buttons
        buttons = new GameObject[current.nextNodes.Count];

        for (int i = 0; i < current.nextNodes.Count; i++)
        {
            int nextIndex = getNodeIndex(dialogue, current.nextNodes[i]);
            if (nextIndex == -1)
            {
                Debug.LogWarning($"Missing node '{current.nextNodes[i]}' in {dialogue.name}");
                endDialogue();
                return;
            }

            Dialogue.Node next = dialogue.nodes[nextIndex];

            Transform button = CreateButton(next.buttonText, null, 24 + 16 * (current.nextNodes.Count - (i + 1))).transform;

            button.GetComponent<Button>().onClick.AddListener(() => { Next(dialogue, nextIndex, speaker); });
            if (dialogue.nodes[nextIndex].action != Dialogue.DialogueAction.Exit)
            {
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    AudioManager.PlayClipByID("clip_select", true);
                    // need condition to ensure no sound clashes occur
                });
            }

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
        if (answer == 0)
        {
            if (OnDialogueCancel != null)
            {
                OnDialogueCancel.Invoke();
            }

            DialogueViewTransitionOut();
        }

        if (window)
        {
            window.playSoundOnClose = soundOnClose;
            window.CloseUI();
            Destroy(window.transform.root.gameObject);
        }

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

    public enum DialogueState
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
        switch (dialogueStyle)
        {
            case DialogueStyle.Original:
                windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5F, 0.5F);
                windowRect.anchoredPosition = new Vector2(0, 0);
                break;
            case DialogueStyle.Remastered:
            default:
                if (speaker && player)
                {
                    if (player.transform.position.y <= speaker.transform.position.y)
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
        while (count < blackBarBottom.sizeDelta.y)
        {
            if (currentState != DialogueState.In)
            {
                break;
            }

            count += 0.1F * blackBarBottom.sizeDelta.y;
            hudGroup.alpha -= 0.1F;
            blackBarTop.anchoredPosition = new Vector2(0, -count);
            blackBarBottom.anchoredPosition = new Vector2(0, count);
            yield return new WaitForSeconds(0.0025F);
        }

        if (currentState == DialogueState.In)
        {
            currentState = DialogueState.Idle;
        }
    }

    IEnumerator BarFadeOut()
    {
        if (blackBarTop == null || blackBarBottom == null)
        {
            yield break;
        }

        blackBarTop.gameObject.SetActive(true);
        blackBarBottom.gameObject.SetActive(true);

        float count = blackBarBottom.anchoredPosition.y;
        while (count > 0)
        {
            if (currentState != DialogueState.Out)
            {
                break;
            }

            count -= 0.1F * blackBarBottom.sizeDelta.y;
            if (!PlayerViewScript.hidingHUD)
            {
                hudGroup.alpha += 0.1F;
            }

            blackBarTop.anchoredPosition = new Vector2(0, -count);
            blackBarBottom.anchoredPosition = new Vector2(0, count);
            yield return new WaitForSeconds(0.0025F);
        }

        if (currentState == DialogueState.Out)
        {
            currentState = DialogueState.Idle;
        }
    }

    public void DialogueViewTransitionOut()
    {
        if (!isInCutscene)
        {
            currentState = DialogueState.Out;
            StartCoroutine("BarFadeOut");
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

    public void ClearCanvases()
    {
        if (traversers != null)
        {
            for (int i = 0; i < traversers.Count; i++)
            {
                traversers[i].nodeCanvas.Destroy();
            }
        }

        dialogueCanvasPaths.Clear();
    }

    public void AddCanvasPath(string path)
    {
        //Debug.Log("Found Dialogue Path " + dialogueCanvasPaths.Count);
        dialogueCanvasPaths.Add(path);
    }

    public static Entity GetSpeaker()
    {
        var speakerObj = SectorManager.instance.GetEntity(speakerID);
        return speakerObj;
    }
}
