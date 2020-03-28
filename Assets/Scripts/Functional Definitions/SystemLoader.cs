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

    public static bool AllLoaded;

    private void Awake()
    {
        AllLoaded = false;

        if (resourceManager)
            resourceManager.Initialize();
        if (sectorManager)
            sectorManager.Initialize();
        if (taskManager)
            taskManager.Initialize();
        if (dialogueSystem)
            DialogueSystem.InitCanvases();
        if (saveHandler)
            saveHandler.Initialize();

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
