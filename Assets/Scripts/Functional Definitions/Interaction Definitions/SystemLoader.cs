using System.Collections;
using UnityEngine;
using Unity.Netcode;
public class SystemLoader : MonoBehaviour
{
    public ResourceManager resourceManager;
    public SaveHandler saveHandler;
    public TaskManager taskManager;
    public SectorManager sectorManager;
    public DialogueSystem dialogueSystem;
    public FactionManager factionManager;
    public AudioManager audioManager;

    public static bool AllLoaded;
    public static bool InitializeCalled;

    public void Initialize()
    {
        /*
        * Save Handler loads sectors now
        */
        AllLoaded = false;
        if (factionManager)
        {
            factionManager.Initialize();
        }

        if (sectorManager)
        {
            sectorManager.Initialize();
        }

        // Save Handler will initialize dialogue canvases after sector loading if present.
        if (!saveHandler && dialogueSystem)
        {
            DialogueSystem.InitCanvases();
        }

        if (saveHandler)
        {
            saveHandler.Initialize();
        }


        // Save Handler will initialize mission canvases after sector loading if present.
        if (!saveHandler && taskManager)
        {
            taskManager.Initialize();
        }

        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer)
            AllLoaded = true;
        else 
        {
            InitializeCalled = true;
            PlayerCore.Instance.AttemptCreateNetworkObject(true);
        }
    }

    public static SystemLoader instance;
    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;

        if (resourceManager)
        {
            resourceManager.Initialize();
        }

        if (audioManager)
        {
            audioManager.Initialize();
        }

        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer) 
        {
            Initialize();
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        if (TaskManager.Instance)
        {
            TaskManager.StartQuests();
        }
    }
}
