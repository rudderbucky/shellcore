using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Flag : MonoBehaviour, IInteractable
{
    public FlagInteractibility interactibility;
    public string sectorName;
    public string entityID;
    public delegate void EntityRangeCheckDelegate(float range);
    public EntityRangeCheckDelegate RangeCheckDelegate;

    private void Update()
    {
        if (RangeCheckDelegate != null && PlayerCore.Instance)
        {
            RangeCheckDelegate.Invoke(Vector2.SqrMagnitude(PlayerCore.Instance.transform.position - transform.position));
        }
    }

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
        else
        {
            var dimensionChanged = PlayerCore.Instance.Dimension != sector.dimension;
            PlayerCore.Instance.Dimension = sector.dimension;

            // TODO: We currently nuke all characters when teleporting to a different dimension. It would be nicer to have
            // a set dimension for each character which is appropriately used.
            if (dimensionChanged)
            {
                foreach (var ent in AIData.entities)
                {
                    if (!(PartyManager.instance && PartyManager.instance.partyMembers != null && ent is ShellCore &&
                            PartyManager.instance.partyMembers.Contains(ent as ShellCore)))
                        foreach (var data in SectorManager.instance.characters)
                        {
                            if (data.ID == ent.ID)
                            {
                                Destroy(ent.gameObject);
                            }
                        }
                }
            }
        }

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
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
