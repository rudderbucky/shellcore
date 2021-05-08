using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Flag : MonoBehaviour, IInteractable
{
    public FlagInteractibility interactibility;
    public string sectorName;
    public string entityID;

    public void Interact()
    {
        switch(interactibility)
        {
            case FlagInteractibility.Warp:

                // need player to warp
                if(!PlayerCore.Instance) break;
                // use sectorName and entityID to find the transform to warp to
                var sector = SectorManager.GetSectorByName(sectorName);
                foreach(var ent in sector.entities)
                {
                    if(ent.ID == entityID)
                    {;
                        // position is a global vector (i.e., not local to the sector itself), so this should work
                        PlayerCore.Instance.Warp(ent.position);
                    }
                }
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
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            AIData.flags.Add(this);
            AIData.interactables.Add(this);
        }
    }

    private void OnDisable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            AIData.flags.Remove(this);
            AIData.interactables.Remove(this);
        }
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
