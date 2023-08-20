using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject saveAndQuitButton;
    private Vector2 originalsaveAndQuitPos;
    public GameObject mainMenuButton;
    private Vector2 originalMainMenuPos;
    public GameObject settingsButton;
    private Vector2 originalSettingsPos;
    private List<Coroutine> coroutines = new List<Coroutine>();

    void OnEnable()
    {
        var mainGame = SceneManager.GetActiveScene().name == "SampleScene" && MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Off;
        if (saveAndQuitButton) saveAndQuitButton.gameObject.SetActive(mainGame);
        if (mainMenuButton && mainGame) mainMenuButton.GetComponentInChildren<Text>().text = "QUIT";
        if (settingsButton) originalSettingsPos = settingsButton.GetComponent<RectTransform>().anchoredPosition;
        if (mainMenuButton) originalMainMenuPos = mainMenuButton.GetComponent<RectTransform>().anchoredPosition;
        if (saveAndQuitButton) originalsaveAndQuitPos = saveAndQuitButton.GetComponent<RectTransform>().anchoredPosition;
        coroutines.Add(StartCoroutine(ButtonAnimation(settingsButton, 0)));
        coroutines.Add(StartCoroutine(ButtonAnimation(mainMenuButton, 0.05F)));
        coroutines.Add(StartCoroutine(ButtonAnimation(saveAndQuitButton, 0.1F)));
    }

    void OnDisable()
    {
        if (settingsButton) settingsButton.GetComponent<RectTransform>().anchoredPosition = originalSettingsPos;
        if (mainMenuButton) mainMenuButton.GetComponent<RectTransform>().anchoredPosition = originalMainMenuPos;
        if (saveAndQuitButton) saveAndQuitButton.GetComponent<RectTransform>().anchoredPosition = originalsaveAndQuitPos;
        foreach (var c in coroutines) StopCoroutine(c);
        coroutines.Clear();
    }

    IEnumerator ButtonAnimation(GameObject button, float delay)
    {
        if (!button) yield return null;
        var group = button.GetComponent<CanvasGroup>();
        Vector2 orig, curr;
        orig = curr = button.GetComponent<RectTransform>().anchoredPosition;
        curr.y -= 20;
        if (!group) yield return null;
        group.alpha = 0;
        yield return new WaitForSecondsRealtime(delay);
        while (group.alpha < 1 || curr.y <= orig.y)
        {
            group.alpha += Time.fixedUnscaledDeltaTime / 0.1F;
            curr.y = Mathf.Min(orig.y, curr.y + 2);
            button.GetComponent<RectTransform>().anchoredPosition = curr;
            yield return new WaitForSecondsRealtime(0.01F);
        }
        
        button.GetComponent<RectTransform>().anchoredPosition = orig;
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
