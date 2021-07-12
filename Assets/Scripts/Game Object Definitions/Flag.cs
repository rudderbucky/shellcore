using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Flag : MonoBehaviour, IInteractable
{
    public FlagInteractibility interactibility;
    public string sectorName;
    public string entityID;

    public static void FindEntityAndWarpPlayer(string sectorName, string entityID)
    {
        // need player to warp
        if (!PlayerCore.Instance)
        {
            return;
        }

        // use sectorName and entityID to find the transform to warp to
        var sector = SectorManager.GetSectorByName(sectorName);
        if (sector == null)
        {
            Debug.LogWarning("<Flag> Cannot find specified sector");
            return;
        }

        PlayerCore.Instance.Dimension = sector.dimension;

        foreach (var ent in sector.entities)
        {
            if (ent.ID == entityID)
            {
                // position is a global vector (i.e., not local to the sector itself), so this should work
                PlayerCore.Instance.Warp(ent.position);
            }
        }
    }

    public void Interact()
    {
        switch (interactibility)
        {
            case FlagInteractibility.Warp:
                FindEntityAndWarpPlayer(sectorName, entityID);
                break;
        }
    }

    public bool GetInteractible()
    {
        return interactibility != FlagInteractibility.None;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void OnEnable()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "SectorCreator" && sceneName != "WorldCreator")
        {
            AIData.flags.Add(this);
            AIData.interactables.Add(this);
        }
    }

    private void OnDisable()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "SectorCreator" && sceneName != "WorldCreator")
        {
            AIData.flags.Remove(this);
            AIData.interactables.Remove(this);
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
