using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerBase : MonoBehaviour, IInteractable, IVendor
{
    public VendingBlueprint vendingBlueprint;
    BattleZoneManager BZManager;
    protected Tower currentTower;
    public Dialogue currentDialogue;
    public EntityNetworkAdapter GetAdapter()
    {
        return null;
    }
    public void SetCurrentTower(Tower tower)
    {
        currentTower = tower;
    }

    protected void Start()
    {
        if (SceneManager.GetActiveScene().name == "SectorCreator" || SceneManager.GetActiveScene().name == "WorldCreator") return;
        BZManager = GameObject.Find("SectorManager").GetComponent<BattleZoneManager>();
        GetComponent<SpriteRenderer>().color = SectorManager.instance.current.backgroundColor + Color.grey;
    }


    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            AIData.interactables.Add(this);
            AIData.vendors.Add(this);
        }
    }

    private void OnDisable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            AIData.interactables.Remove(this);
            AIData.vendors.Remove(this);
        }
    }


    public bool TowerActive()
    {
        return currentTower && !currentTower.GetIsDead();
    }

    public VendingBlueprint GetVendingBlueprint()
    {
        return vendingBlueprint;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw lines to the closest ground tiles

        for (int i = 0; i < LandPlatformGenerator.Instance.groundPlatforms.Length; i++)
        {
            Vector2 pos0 = transform.position;
            GroundPlatform.Tile? tile = LandPlatformGenerator.Instance.GetNearestTile(LandPlatformGenerator.Instance.groundPlatforms[i], pos0);
            if (!tile.HasValue || tile.Value == null) continue;
            if (tile.Value.colliders == null)
            {
                string brokenTiles = "";
                var tileList = LandPlatformGenerator.Instance.groundPlatforms[i].tiles;
                for (int j = 0; j < tileList.Count; j++)
                {
                    if (tileList[j].colliders == null)
                    {
                        brokenTiles += tileList[j].pos + " ";
                    }
                }
                Debug.LogError($"Platform [{i}]'s tile at {tile.Value.pos} has no collider. Total tile count: {tileList.Count} Broken tile list: {brokenTiles}");
            }

            Vector2 pos1 = LandPlatformGenerator.TileToWorldPos(tile.Value.pos);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos0, pos1);
        }
    }

    void IInteractable.Interact()
    {
        DialogueSystem.StartDialogue(currentDialogue, this);
    }

    bool IInteractable.GetInteractible()
    {
        return !currentTower || currentTower.GetIsDead();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public int GetFaction()
    {
        return 3;
    }

    public bool NeedsAlliedFaction()
    {
        return false;
    }

    void Update()
    {
        if (!SectorManager.instance)
        {
            return;
        }

        if (SectorManager.instance.overrideProperties != null)
        {
            GetComponent<SpriteRenderer>().color = SectorManager.instance.overrideProperties.backgroundColor + Color.grey;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = SectorManager.instance.current.backgroundColor + Color.grey;
        }
    }
}
