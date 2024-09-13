using System.Collections.Generic;
using UnityEngine;

public class PlayerViewScript : MonoBehaviour
{
    private Stack<IWindow> currentWindow;
    private static PlayerViewScript instance;
    public static int currentLayer = 2;
    public static bool paused;

    public static bool hidingHUD;

    public static bool GetIsWindowActive()
    {
        foreach (var window in instance.currentWindow)
        {
            if (window != null && !window.Equals(null) && window.GetActive())
            {
                return true;
            }
        }

        return false;
    }

    public static void SetCurrentWindow(IWindow window)
    {
        instance.currentWindow.Push(window);
    }

    public GameObject escapeMenu;

    Canvas escapeCanvas;

    // Update is called once per frame
    void Awake()
    {
        if (escapeMenu)
        {
            escapeCanvas = escapeMenu.GetComponentInChildren<Canvas>();
        }

        currentWindow = new Stack<IWindow>();
        instance = this;
        if (escapeMenu)
        {
            escapeMenu.SetActive(false);
        }

        paused = false;
        Time.timeScale = 1;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (DialogueSystem.Instance && DialogueSystem.Instance.hudGroup && InputManager.GetKeyDown(KeyName.HideHUD))
        {
            if (!DialogueSystem.isInCutscene && (!DialogueSystem.Instance.IsWindowActive()))
            {
                hidingHUD = !hidingHUD;
                DialogueSystem.Instance.hudGroup.alpha = 1 - DialogueSystem.Instance.hudGroup.alpha;
                if (PassiveDialogueSystem.Instance && PassiveDialogueSystem.Instance.passiveDialogueCanvasGroup) 
                    PassiveDialogueSystem.Instance.passiveDialogueCanvasGroup.alpha = 1 - PassiveDialogueSystem.Instance.passiveDialogueCanvasGroup.alpha;
            }
        }

        if (InputManager.GetKeyUp(KeyName.Exit) && (!RollCredits.instance || !RollCredits.instance.init))
        {
            // for some reason this is escape
            while (currentWindow.Count > 0)
            {
                if (DialogueSystem.isInCutscene)
                {
                    break; // just go straight to escape menu, in cutscenes you can't escape dialogue
                }

                // if the escape menu is on, untoggle it and prevent the same escape from cancelling something else
                if (escapeMenu && escapeMenu.activeSelf && !transform.Find("Settings").gameObject.activeSelf)
                {
                    escapeMenu.GetComponent<EscapeMenu>().Close();
                    return;
                }

                bool shouldReturn = currentWindow.Peek().Equals(null) ? false : currentWindow.Peek().GetActive();

                if (shouldReturn)
                {
                    var window = currentWindow.Pop();
                    window.CloseUI();
                    window.GetOnCancelled()?.Invoke();

                    return; // prevents the escape menu code from running
                }
                else
                {
                    currentWindow.Pop(); // pop through the already closed windows
                }
            }

            if (escapeMenu)
            {
                escapeMenu.SetActive(!escapeMenu.activeSelf); // toggle
                paused = escapeMenu.activeSelf;
                escapeCanvas.sortingOrder = ++currentLayer;
                if (escapeMenu.activeSelf && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off)
                {
                    Time.timeScale = 0;
                    AudioListener.pause = true;
                }
                else
                {
                    Time.timeScale = 1;
                    AudioListener.pause = false;
                }
            }
        }
    }
}
