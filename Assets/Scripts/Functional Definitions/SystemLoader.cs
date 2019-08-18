using UnityEngine;
using UnityEngine.Events;

public class SystemLoader : MonoBehaviour
{
    public ResourceManager resourceManager;
    public SaveHandler saveHandler;
    public TaskManager taskManager;
    public SectorManager sectorManager;
    
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
        if (saveHandler)
            saveHandler.Initialize();

        AllLoaded = true;
    }

    private void Start()
    {
        if (TaskManager.Instance)
            TaskManager.StartQuests();
    }
}
