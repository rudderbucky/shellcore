using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class ButtonHoverScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    bool mouseTag; // used for animation
    RectTransform rect; // used for animation

    public void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "WorldCreator" && GetComponentInChildren<Text>() && GetComponentInChildren<Text>().text.Contains("SAVE AND QUIT"))
        {
            GetComponentInChildren<Text>().text = "MAIN MENU";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (name == "MainMenuButton")
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

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }
}
