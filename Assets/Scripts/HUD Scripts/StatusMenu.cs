using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StatusMenu : GUIWindowScripts
{
    public PlayerCore player;
    public Text playerName;
    bool toggle = false;
    public static Task[] taskInfo = new Task[0];

    public Button[] buttons;
    public Transform[] sections;
    public static StatusMenu instance;

    void Awake()
    {
        instance = this;

        for (int i = 0; i < buttons.Length; i++)
        {
            int x = i;
            buttons[i].onClick.AddListener(new UnityEngine.Events.UnityAction(() => { SwitchSections(x); }));
        }
    }

    ///
    /// Switches Status Menu sections to the index specified.
    ///
    public void SwitchSections(int x)
    {
        for (int j = 0; j < sections.Length; j++)
        {
            if (x != j)
            {
                sections[j].gameObject.SetActive(false);
                buttons[j].image.color = Color.grey;
            }
            else
            {
                sections[j].gameObject.SetActive(true);
                buttons[j].image.color = Color.white;
            }
        }
    }

    public override void CloseUI()
    {
        AudioManager.PlayClipByID("clip_back");
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        toggle = false;
    }

    void Initialize()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        AudioManager.PlayClipByID("clip_select");
        PlayerViewScript.SetCurrentWindow(this);
        GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer;
        playerName.text = $"<color=yellow>{player.cursave.name}</color>";
        base.Activate();
        GetComponentsInChildren<SubcanvasSortingOrder>(true).ToList().ForEach(x => x.Initialize());
    }

    protected override void Update()
    {
        if (DialogueSystem.isInCutscene)
        {
            if (toggle)
            {
                toggle = false;
                CloseUI();
            }
            return;
        }

        // Not paused & key pressed
        if (Time.timeScale > 0 && InputManager.GetKeyDown(KeyName.StatusMenu))
        {
            Open();
        }

        base.Update();
    }

    public void Open()
    {
        // Player doesn't need to pay attention to something else
        if (!player.GetIsInteracting() && !DialogueSystem.isInCutscene && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
        {
            toggle = !toggle;
            if (toggle)
            {
                Initialize();
            }
            else
            {
                AudioManager.PlayClipByID("clip_back");
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(toggle);
            }
        }
    }

    public override bool GetActive()
    {
        return toggle;
    }

    public void AddMission(string name, string rank)
    {
    }
}
