using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject saveAndQuitButton;
    public GameObject mainMenuButton;
    public GameObject settingsButton;

    void OnEnable()
    {
        var mainGame = SceneManager.GetActiveScene().name == "SampleScene";
        if (saveAndQuitButton) saveAndQuitButton.gameObject.SetActive(mainGame);
        if (mainMenuButton && mainGame) mainMenuButton.GetComponentInChildren<Text>().text = "QUIT";
    }
    void Start()
    {
        
    }

    void Update()
    {
        
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
