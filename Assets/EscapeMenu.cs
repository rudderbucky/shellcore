using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject mainMenuButton;
    public GameObject statusMenuButton;

    public RectTransform[] buttons;
    private Vector2[] originalPositions;

    private List<Coroutine> coroutines = new List<Coroutine>();

    void OnEnable()
    {
        var mainGame = SceneManager.GetActiveScene().name == "SampleScene" && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off;

        originalPositions = new Vector2[buttons.Length];
        if (mainMenuButton && mainGame) mainMenuButton.GetComponentInChildren<Text>().text = "QUIT";
        if (statusMenuButton) statusMenuButton.GetComponent<Button>().interactable = !DialogueSystem.isInCutscene;

        int delay = 0;
        for (int i = 0; i < buttons.Length; i++)
        {
            originalPositions[i] = buttons[i].anchoredPosition;
            coroutines.Add(StartCoroutine(ButtonAnimation(buttons[i], delay * 0.05f)));
            delay++;
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].anchoredPosition = originalPositions[i];
        }

        foreach (var c in coroutines) StopCoroutine(c);
        coroutines.Clear();
    }

    public void Close()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        gameObject.SetActive(false);
    }

    IEnumerator ButtonAnimation(RectTransform button, float delay)
    {
        if (!button) yield return null;
        var group = button.GetComponent<CanvasGroup>();
        Vector2 orig, curr;
        orig = curr = button.anchoredPosition;
        curr.y -= 20;
        if (!group) yield return null;
        group.alpha = 0;
        yield return new WaitForSecondsRealtime(delay);
        while (group.alpha < 1 || curr.y <= orig.y)
        {
            group.alpha += Time.fixedUnscaledDeltaTime / 0.1F;
            curr.y = Mathf.Min(orig.y, curr.y + 2);
            button.anchoredPosition = curr;
            yield return new WaitForSecondsRealtime(0.01F);
        }
        
        button.anchoredPosition = orig;
        yield return null;
        
    }

    public void SaveAndQuit()
    {
        if (SaveHandler.instance) SaveHandler.instance.Save();
        MainMenu();
    }

    public void MainMenu()
    {
        if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off)
        {
            MasterNetworkAdapter.lettingServerDecide = false;
            NetworkManager.Singleton.Shutdown();
            MasterNetworkAdapter.mode = MasterNetworkAdapter.NetworkMode.Off;
            Destroy(NetworkManager.Singleton.gameObject);
            SectorManager.testJsonPath = null;
        }

        if (SectorManager.testJsonPath == null)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("WorldCreator");
            WCWorldIO.instantTest = false;
            SectorManager.testJsonPath = null;
        }
    }
}
