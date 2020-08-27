using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SystemLoader : MonoBehaviour
{
    public ResourceManager resourceManager;
    public SaveHandler saveHandler;
    public TaskManager taskManager;
    public SectorManager sectorManager;
    public BackgroundScript backgroundScript;
    public DialogueSystem dialogueSystem;
    public FactionManager factionManager;

    public static bool AllLoaded;

    private void Awake()
    {
        /*
        * Save Handler loads sectors now
        */
        AllLoaded = false;
        Application.targetFrameRate = 60;

        if (resourceManager)
            resourceManager.Initialize();
        if (factionManager)
            factionManager.Initialize();
        if (sectorManager)
            sectorManager.Initialize();

        // Save Handler will initialize dialogue canvases after sector loading if present.
        if (!saveHandler && dialogueSystem)
            DialogueSystem.InitCanvases();
        if (saveHandler)
            saveHandler.Initialize();
        if (taskManager)
            taskManager.Initialize();

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
            TaskManager.StartQuests();
        if (DialogueSystem.Instance && DialogueSystem.GetInitialized())
            DialogueSystem.StartQuests();
    }
}
