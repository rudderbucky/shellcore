using System.Collections;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.IO;
using NodeEditorFramework.Standard;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CoreScriptsManager;

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
    public VendorUI vendorUI;
    int characterCount = 0;
    float nextCharacterTime;
    private float typingSpeedFactor = 1;
    public double timeBetweenCharacters = 0.01f;
    string text = "";
    public PlayerCore player;
    Vector3? speakerPos = null;
    public FusionStationScript fusionScript;
    public CoreUpgraderScript upgraderScript;
    public static bool isInCutscene = false;
    BattleZoneManager battleZoneManager;

    public Image faderImage;
    public void FadeInScreenBlack(Color color, float speedFactor)
    {
        StartCoroutine(FadeInScreenBlackCo(color, speedFactor));
    }
    private IEnumerator FadeInScreenBlackCo(Color color, float speedFactor)
    {
        color.a = 0;
        faderImage.color = color;
        while (faderImage.color.a < 1)
        {
            var c = faderImage.color;
            c.a += 0.1F;
            faderImage.color = c;
            yield return new WaitForSeconds(0.05F / speedFactor);
        }
    }

    public void FadeOutScreenBlack(Color color, float speedFactor)
    {
        StartCoroutine(FadeOutScreenBlackCo(color, speedFactor));
    }
    private IEnumerator FadeOutScreenBlackCo(Color color, float speedFactor)
    {
        color.a = 1;
        faderImage.color = color;
        while (faderImage.color.a > 0)
        {
            var c = faderImage.color;
            c.a -= 0.1F;
            faderImage.color = c;
            yield return new WaitForSeconds(0.05F / speedFactor);
        }
    }




    public enum DialogueStyle
    {
        Remastered,
        Original
    }

    public static DialogueStyle dialogueStyle;

    public bool IsWindowActive()
    {
        return window && (!window.transform.parent || window.transform.parent.gameObject.activeSelf);
    }

    private void Awake()
    {
        Instance = this;
        dialogueStyle = (DialogueStyle)PlayerPrefs.GetInt("DialogueSystem_dialogueStyle", 0);
        isInCutscene = false;
        CameraScript.panning = false;
    }

    public static List<string> dialogueCanvasPaths = new List<string>();
    public static Dictionary<string, Stack<InteractAction>> interactionOverrides = new Dictionary<string, Stack<InteractAction>>();
    private static List<DialogueTraverser> traversers;
    public static string speakerID;
    private static bool initialized = false;

    public void PushInteractionOverrides(string entityID, InteractAction action, Traverser traverser, Context context = null) 
    {
        if (GetInteractionOverrides().ContainsKey(entityID))
        {
            GetInteractionOverrides()[entityID].Push(action);
        }
        else
        {
            var stack = new Stack<InteractAction>();
            stack.Push(action);
            GetInteractionOverrides().Add(entityID, stack);
        }
    }

    // Bugfixes dialogue canvas paths persisting post world reload in the WC
    public static void ClearStatics()
    {
        if (dialogueCanvasPaths != null)
        {
            dialogueCanvasPaths.Clear();
        }

        if (interactionOverrides != null)
        {
            interactionOverrides.Clear();
        }

        if (traversers != null)
        {
            traversers.Clear();
        }

        if (offloadingDialogues != null)
        {
            offloadingDialogues.Clear();
        }
    }

    public static Dictionary<string, string> offloadingDialogues = new Dictionary<string, string>();

    public static void InitCanvases()
    {
        interactionOverrides = new Dictionary<string, Stack<InteractAction>>();
        traversers = new List<DialogueTraverser>();
        NodeCanvasManager.FetchCanvasTypes();
        NodeTypes.FetchNodeTypes();
        ConnectionPortManager.FetchNodeConnectionDeclarations();

        if (Instance)
        {
            Instance.ClearCanvases();
        }

        var XMLImport = new XMLImportExport();

        if (Entity.OnEntitySpawn != null)
            Entity.OnEntitySpawn += startDialogueGraph;
        else
            Entity.OnEntitySpawn = startDialogueGraph;

        OnDialogueCancel = null;
        OnDialogueEnd = null;
        StartDialogueNode.dialogueCanvasNode = null;
        StartDialogueNode.missionCanvasNode = null;
        initialized = true;
    }

    private static void startDialogueGraph(Entity entity)
    {
        var id = entity.ID;
        if (id == null || offloadingDialogues == null || !offloadingDialogues.ContainsKey(id)) return;

        var path = offloadingDialogues[id];
        offloadingDialogues.Remove(id);
        var canvas = TaskManager.GetCanvas(path) as DialogueCanvas;
        if (canvas != null)
        {
            var traverser = new DialogueTraverser(canvas);
            traversers.Add(traverser);
            if (traverser != null)
            {
                var start = traverser.findRoot();
                canvas.entityID = start.EntityID;
                traverser.StartQuest();
            }
        }
        else Debug.Log("null canvas path");
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

    public bool IsVoting()
    {
        return voteTimeLeft > 0;
    }

    private void Update()
    {
        if (voteTimeLeft > 0)
        {
            voteTimeLeft -= Time.deltaTime;
            if (voteBoxTitle) voteBoxTitle.GetComponentInChildren<Text>().text = "SELECT MAP (" + Mathf.CeilToInt(voteTimeLeft) + "s)";
            if (voteTimeLeft <= 0)
            {
                FinishVote();
            }
        }
        if (window && speakerPos != null && player &&
            ((player.transform.position - ((Vector3)speakerPos)).sqrMagnitude > 100 || player.GetIsDead() 
            || (speaker && speaker.GetIsDead())) && !isInCutscene)
        {
            endDialogue();
        }

        // reset window position if dialogue is over
        if (!window)
        {
            lastPosition = DialogueWindowPosition.None;
        }

        // Add text
        if (textRenderer && characterCount < text.Length)
        {
            if (Time.time > nextCharacterTime)
            {
                characterCount++;
                nextCharacterTime = (float)(Time.time + timeBetweenCharacters / typingSpeedFactor);
                textRenderer.text = text.Substring(0, characterCount);
            }
        }
    }

    private Entity speaker;
    public bool IsSpeaking()
    {
        return speaker && !speaker.GetIsDead();
    }

    public static void StartDialogue(Dialogue dialogue, IInteractable speaker = null, Context context = null)
    {
        Instance.startDialogue(dialogue, speaker, context);
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

        this.speaker = speaker;

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
        var display = window.transform.Find("Background/RadioVisual/Radio/Holder")?.GetComponentInChildren<SelectionDisplayHandler>();
        if (display)
        {
            var remastered = GetDialogueStyle() == DialogueStyle.Remastered;
            if (speaker && remastered)
            {
                DialogueViewTransitionIn(speaker);
                display.AssignDisplay(speaker.blueprint, null, speaker.faction);
                window.transform.Find("Background/RadioVisual/Name").GetComponent<Text>().text = speaker.blueprint.entityName;
            }
            else
            {
                display.gameObject.SetActive(false);
                window.transform.Find("Background/RadioVisual/Name").GetComponent<Text>().text = remastered ? "Unknown Speaker" : "";
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

    private DialogueStyle GetDialogueStyle()
    {
        if (isInCutscene) return DialogueStyle.Remastered;
        return dialogueStyle;
    }
    ///
    /// Creates and returns a button with the passed text, action call, and y position.
    ///
    private GameObject CreateButton(string text, UnityAction call, int ypos)
    {
        RectTransform button = Instantiate(dialogueButtonPrefab).GetComponent<RectTransform>();
        button.SetParent(background, false);
        button.anchoredPosition = new Vector2(0, ypos);
        button.GetComponent<Image>().enabled = GetDialogueStyle() == DialogueStyle.Remastered ;
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
        buttons[0] = CreateButton("Ok.", () => { endDialogue(); }, 24);
    }

    public static void ShowBattleResults(bool victory)
    {
        Instance.showBattleResults(victory);
    }


    private Transform voteBox;
    private Transform voteBoxTitle;
    public GameObject voteBoxPrefab;
    public List<int> voteNumbers = new List<int>();
    private List<Button> voteButtons = new List<Button>();
    private List<string> voteOptions = new List<string>();
    public Dictionary<ulong, int> votesById = new Dictionary<ulong, int>();
    private float voteTimeLeft = 0;
    private static float VOTE_TIME = 10;
    private void FinishVote()
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
            BattleZoneManager.END_CHECK_TIMER = Time.time + 1;
        if (window)
        {
            window.CloseUI();
        }
        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client)
        {
            return;
        }

        var maxIndex = 0;
        for (int i = 0; i < voteNumbers.Count; i++)
        {
            if (voteNumbers[i] > voteNumbers[maxIndex])
            {
                maxIndex = i;
            }
        }

        SectorManager.instance.ReloadSector(maxIndex);
        MasterNetworkAdapter.instance.ReloadSectorClientRpc(maxIndex);
    }

    public void RefreshButtons()
    {
        if (voteButtons == null) return;
        for (int i = 0; i < voteButtons.Count; i++)
        {
            var box = voteButtons[i];
            if (!box || !box.GetComponentInChildren<Text>()) continue;
            box.GetComponentInChildren<Text>().text = voteOptions[i] + " (" + voteNumbers[i] + ")";
        }
    }

    private void StartVote()
    {
        voteNumbers.Clear();
        voteButtons.Clear();
        votesById.Clear();
        voteOptions.Clear();
        voteTimeLeft = VOTE_TIME;
    }

    public void StartSectorVote()
    {
        StartVote();
        foreach (var sect in SectorManager.instance.sectors)
        {
            voteOptions.Add(sect.sectorName);
            voteNumbers.Add(0);
        }
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
        voteBox = window.transform.Find("Background/Vote/Scroll View/Viewport/Content");
        voteBoxTitle = window.transform.Find("Background/Vote/Vote Title");

        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
        {
            StartSectorVote();
            for (int i = 0; i < SectorManager.instance.sectors.Count; i++)
            {
                int a = i;
                var sect = SectorManager.instance.sectors[i];
                var box = Instantiate(voteBoxPrefab, voteBox);
                voteButtons.Add(box.GetComponentInChildren<Button>());
                box.GetComponentInChildren<Button>().onClick.AddListener(() => {
                    voteButtons.ForEach(b => {b.GetComponentInChildren<Image>().color = Color.white;});
                    box.GetComponentInChildren<Image>().color = Color.green;
                    MasterNetworkAdapter.instance.RequestVoteServerRpc(a);
                    RefreshButtons();
                });
            }
            RefreshButtons();
        }
        else
        {
            window.transform.Find("Background/Vote").gameObject.SetActive(false);
            RectTransform resultsUI = window.transform.Find("Background/ViewBorder").gameObject.GetComponent<RectTransform>();
            resultsUI.sizeDelta = new Vector2(550, resultsUI.sizeDelta.y);
            resultsUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -40, 0);
        }
        
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
            rt.localScale = Vector3.one;

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

        window.transform.Find("Holder").Find("Rewards").GetComponent<Text>().text = rewardsText;
    }

    public static void ShowDialogueNode(NodeEditorFramework.Standard.DialogueNode node, Entity speaker = null)
    {
        Instance.showCanvasDialogue(node.text, node.answers, speaker, node.textColor, node.useEntityColor);
    }

    public static void ShowFinishTaskNode(NodeEditorFramework.Standard.FinishTaskNode node, Entity speaker = null)
    {
        if (node.answers == null)
        {
            node.answers = new List<string>();
            node.answers.Add("Ok");
        }

        Instance.showCanvasDialogue(node.rewardText, node.answers, speaker, node.textColor, node.useEntityColor);
    }

    private void showCanvasDialogue(string text, List<string> answers, Entity speaker, Color textColor, bool useEntityColor = true)
    {
        typingSpeedFactor = 1;
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
        var taskRewardInfo = gameObject.transform.Find("TaskRewardInfo");
        taskRewardInfo.Find("Credit Reward Text").GetComponent<Text>().text =
            "Credit reward: " + wrapper.creditReward;

        taskRewardInfo.Find("Reputation Reward Text").GetComponent<Text>().text =
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

            var partImage = taskRewardInfo.Find("Part").GetComponent<Image>();
            partImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.spriteID);
            partImage.rectTransform.sizeDelta = partImage.sprite.bounds.size * 45;
            partImage.color = Color.green;

            // Ability image:
            if (wrapper.partAbilityID > 0)
            {
                var backgroudBox = taskRewardInfo.Find("backgroundbox");
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
                taskRewardInfo.Find("backgroundbox").gameObject.SetActive(false);
            }
        }
        else
        {
            taskRewardInfo.Find("Part").GetComponent<Image>().enabled = false;
            taskRewardInfo.Find("backgroundbox").gameObject.SetActive(false);
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
        background.Find("TaskRewardInfo").gameObject.SetActive(true);
        background.Find("RadioVisual").GetComponent<CanvasGroup>().alpha = 0.1F;
        DialogueViewTransitionIn(speaker);
        AudioManager.PlayClipByID("clip_select", true); // task button cannot create a noise because it launches endDialogue()
        // so cover for its noise here

        // Objective list
        var objectiveList = background.transform.Find("TaskRewardInfo/ObjectiveList").GetComponent<Text>();
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

    private void startDialogue(Dialogue dialogue, IInteractable speaker, Context context = null)
    {
        typingSpeedFactor = 1;
        if (window)
        {
            endDialogue();
        }
        if (speaker != null)
            speakerPos = speaker.GetTransform().position;
        else speakerPos = null;
        //create window
        window = Instantiate(dialogueBoxPrefab).GetComponentInChildren<GUIWindowScripts>();
        window.Activate();



        DialogueViewTransitionIn(speaker as Entity);

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

        next(dialogue, 0, speaker, context);
    }

    public static void Next(Dialogue dialogue, int ID, IInteractable speaker, Context context = null)
    {
        Instance.next(dialogue, ID, speaker, context);
    }

    public void OpenBuilder(Vector3 speakerPos)
    {
        builder.yardPosition = speakerPos;
        builder.Initialize(BuilderMode.Yard);
    }

    public void OpenTrader(Vector3 speakerPos, List<EntityBlueprint.PartInfo> traderInventory)
    {
        builder.yardPosition = speakerPos;
        builder.Initialize(BuilderMode.Trader, traderInventory);
    }

    public void OpenUpgrader(Vector3 speakerPos)
    {
        builder.yardPosition = speakerPos;
        upgraderScript.initialize();
    }

    public void OpenFusion(Vector3 speakerPos)
    {
        builder.yardPosition = speakerPos;
        fusionScript.Activate();
    }

    public void OpenWorkshop(Vector3 speakerPos)
    {
        builder.yardPosition = speakerPos;
        builder.Initialize(BuilderMode.Workshop);
    }

    private void next(Dialogue dialogue, int ID, IInteractable speaker, Context context = null)
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
            case Dialogue.DialogueAction.FinishTask:
                AudioManager.PlayClipByID("clip_typing", true);
                break;
            case Dialogue.DialogueAction.Outpost:
                endDialogue(0, false);
                if ((speaker as IVendor).NeedsAlliedFaction() && !FactionManager.IsAllied((speaker as IVendor).GetFaction(), player.faction))
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
                OpenWorkshop((Vector3)speakerPos);
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Fusion:
                OpenFusion((Vector3)speakerPos);
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.Upgrader:
                OpenUpgrader((Vector3)speakerPos);
                upgraderScript.initialize();
                endDialogue(0, false);
                return;
            case Dialogue.DialogueAction.InvokeEnd:
                endDialogue(ID, false);
                return;
            case Dialogue.DialogueAction.ForceToNextID:
                next(dialogue, current.nextNodes[0], speaker, context);
                return;
            case Dialogue.DialogueAction.Call:
                endDialogue(0, false);
                var s = CoreScriptsManager.instance.GetFunction(current.functionID);
                CoreScriptsSequence.RunSequence(s, context);
                return;
            default:
                break;
        }

        if (current.action == Dialogue.DialogueAction.FinishTask)
        {
            TaskFlow.RewardPlayer(context.missionName);
        }

        if (current.forceSpeakerChange)
        {
            var speakerID = current.speakerID;
            if (current.coreScriptsMode) speakerID = CoreScriptsSequence.VariableSensitizeValue(speakerID);
            speaker = AIData.entities.Find(x => x.ID == speakerID);
            speakerPos = speaker.GetTransform().position;
        }

        var remastered = GetDialogueStyle() == DialogueStyle.Remastered;
        if (speaker as Entity)
        {
            var ent = speaker as Entity;
            // radio image 
            var entName = ent.blueprint.entityName;
            if (current.concealName) entName = "Unknown Speaker";
            if (remastered)
                window.transform.Find("Background/RadioVisual/Radio/Holder").GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(ent.blueprint, null, ent.faction);
            window.transform.Find("Background/RadioVisual/Name").GetComponent<Text>().text = remastered ? entName : "";
        }

        // change text
        if (current.coreScriptsMode)
        {
            text = SendThroughCoreScripts(current.text).Replace("<br>", "\n");
        }
        else
        {
            text = current.text.Replace("<br>", "\n");
        }
        characterCount = 0;

        if (current.typingSpeedFactor == 0) current.typingSpeedFactor = 1;
        typingSpeedFactor = current.typingSpeedFactor;
        nextCharacterTime = (float)(Time.time + timeBetweenCharacters / current.typingSpeedFactor);

        textRenderer.color = current.textColor;
        if (current.useSpeakerColor && speaker is Entity colorEnt) textRenderer.color = FactionManager.GetFactionColor(colorEnt.faction);

        background.Find("RadioVisual").GetComponent<CanvasGroup>().alpha = 1F;
        background.Find("TaskRewardInfo").gameObject.SetActive(false);
        if (current.task != null)
        {
            var wrapper = new RewardWrapper();
            wrapper.creditReward = (int)current.task.creditReward;
            wrapper.partAbilityID = current.task.partReward.abilityID;
            wrapper.partReward = !string.IsNullOrEmpty(current.task.partReward.partID);
            wrapper.partSecondaryData = current.task.partReward.secondaryData;
            wrapper.partTier = current.task.partReward.tier;
            wrapper.reputationReward = current.task.reputationReward;
            wrapper.shardReward = (int)current.task.shardReward;
            wrapper.partID = current.task.partReward.partID;


            background.Find("RadioVisual").GetComponent<CanvasGroup>().alpha = 0.1F;
            background.Find("TaskRewardInfo").gameObject.SetActive(true);
            // Objective list
            var objectiveList = background.transform.Find("TaskRewardInfo/ObjectiveList").GetComponent<Text>();
            objectiveList.text = current.task.useLocalMap ? SendThroughCoreScripts(current.task.objectived) : current.task.objectived;
            SetupRewards(background.gameObject, wrapper);
        }
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

            var buttonText = next.buttonText;
            if (next.coreScriptsMode) buttonText = SendThroughCoreScripts(buttonText);
            Transform button = CreateButton(buttonText, null, 24 + 24 * (current.nextNodes.Count - (i + 1))).transform;

            if (next.action == Dialogue.DialogueAction.ForceToNextID)
            {
                nextIndex = next.nextNodes[0];
            }
            
            int x = i;
            button.GetComponent<Button>().onClick.AddListener(() => { 
                if (x == 0 && current.task != null)
                {
                    SectorManager.instance.player.alerter.showMessage("New Task", "clip_victory");
                    current.task.dialogue = current.text;
                    current.task.dialogueColor = textRenderer.color;
                    StartTaskNode.RegisterTask(current.task, context.missionName);
                }
                Next(dialogue, current.nextNodes[x], speaker, context); 
            });
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

    private string SendThroughCoreScripts(string val)
    {
        val = CoreScriptsSequence.VariableSensitizeValue(val);
        return CoreScriptsManager.instance.GetLocalMapString(val);
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
        speaker = null;
        if (window)
        {
            window.playSoundOnClose = soundOnClose;
            Destroy(window.transform.root.gameObject);
            window.CloseUI();
        }

        // strange behavior here when tasks are checkpointed, look into this when bugs arise
        if (answer == 0)
        {
            if (OnDialogueCancel != null)
            {
                OnDialogueCancel.Invoke();
            }
            DialogueViewTransitionOut();
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

    private enum DialogueWindowPosition
    {
        Down,
        Up,
        None
    }

    private DialogueWindowPosition lastPosition = DialogueWindowPosition.None;

    private void DialogueViewTransitionIn(Entity speaker = null)
    {
        currentState = DialogueState.In;
        var windowRect = window.GetComponent<RectTransform>();
        switch (GetDialogueStyle())
        {
            case DialogueStyle.Original:
                windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5F, 0.5F);
                windowRect.anchoredPosition = new Vector2(0, 0);
                break;
            case DialogueStyle.Remastered:
            default:
                if (speaker && player)
                {
                    if (lastPosition == DialogueWindowPosition.None)
                    {
                        if (player.transform.position.y <= speaker.transform.position.y)
                        {
                            windowRect.anchorMin = new Vector2(0, 0);
                            windowRect.anchorMax = new Vector2(1, 0);
                            windowRect.anchoredPosition = new Vector2(0, 200);
                            lastPosition = DialogueWindowPosition.Up;
                        }
                        else
                        {
                            windowRect.anchorMin = new Vector2(0, 1);
                            windowRect.anchorMax = new Vector2(1, 1);
                            windowRect.anchoredPosition = new Vector2(0, -200);
                            lastPosition = DialogueWindowPosition.Down;
                        }
                    }
                    else if (lastPosition == DialogueWindowPosition.Down)
                    {
                        windowRect.anchorMin = new Vector2(0, 1);
                        windowRect.anchorMax = new Vector2(1, 1);
                        windowRect.anchoredPosition = new Vector2(0, -200);
                    }
                    else
                    {
                        windowRect.anchorMin = new Vector2(0, 0);
                        windowRect.anchorMax = new Vector2(1, 0);
                        windowRect.anchoredPosition = new Vector2(0, 200);
                    }

                }

                FadeBarIn();
                break;
        }
    }

    public void FadeBarIn()
    {
        currentState = DialogueState.In;
        StopCoroutine("BarFadeOut");
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
        hudGroup.alpha = 0F;
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
        hudGroup.alpha = 1F;
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
            StopCoroutine("BarFadeIn");
            StartCoroutine("BarFadeOut");
        }
    }

    public Dictionary<string, Stack<InteractAction>> GetInteractionOverrides()
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

    public void ClearInteractionOverrides(string entityID)
    {
        if (DialogueSystem.interactionOverrides.ContainsKey(entityID))
        {
            var stack = DialogueSystem.interactionOverrides[entityID];
            if(stack.Count > 0)
            {
                DialogueSystem.interactionOverrides[entityID].Clear();
            }
        }
        else
        {
            Debug.LogWarning(entityID + " missing from interaction override dictionary!");
        }
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
