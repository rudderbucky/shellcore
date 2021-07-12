using System.Collections;
using UnityEngine;

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

    private void Awake()
    {
        /*
        * Save Handler loads sectors now
        */
        AllLoaded = false;
        Application.targetFrameRate = 60;

        if (resourceManager)
        {
            resourceManager.Initialize();
        }

        if (factionManager)
        {
            factionManager.Initialize();
        }

        if (audioManager)
        {
            audioManager.Initialize();
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

        AllLoaded = true;
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

        if (DialogueSystem.Instance && DialogueSystem.GetInitialized())
        {
            DialogueSystem.StartQuests();
        }
    }
}
