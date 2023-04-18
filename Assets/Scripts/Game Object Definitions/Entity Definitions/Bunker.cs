using UnityEngine;

public class Bunker : GroundConstruct, IVendor
{
    public VendingBlueprint vendingBlueprint;
    BattleZoneManager BZManager;

    public bool NeedsSameFaction()
    {
        return true;
    }
    protected override void Start()
    {
        category = EntityCategory.Station;
        base.Start();
        BZManager = GameObject.Find("SectorManager").GetComponent<BattleZoneManager>();
    }

    public override void RemovePart(ShellPart part)
    {
        if (part)
        {
            if (part.gameObject.name != "Shell Sprite")
            {
                Destroy(part.gameObject);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        TargetManager.Enqueue(targeter);
        WeaponAbility[] bullets = GetComponentsInChildren<WeaponAbility>();
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].Tick();
        }
    }

    protected override void DeathHandler()
    {
        if (currentHealth[0] <= 0 && !isDead)
        {
            OnDeath(); // switch factions
        }
    }

    protected override void OnDeath()
    {
        targeter.SetTarget(null);
        int otherFaction = faction;

        if (sectorMngr.GetCurrentType() == Sector.SectorType.BattleZone)
        {
            BZManager.UpdateCounters();
            BZManager.AttemptAlertPlayers(otherFaction, "WARNING: Bunker lost!", "clip_stationlost");
        }
        else if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide)
        {
            if (MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off && !MasterNetworkAdapter.lettingServerDecide
                && lastDamagedBy is ShellCore core && core.networkAdapter && core.networkAdapter.isPlayer.Value)
                {
                    HUDScript.AddScore(core.networkAdapter.playerName, 1);
                }
        
        }


        if (MasterNetworkAdapter.mode == MasterNetworkAdapter.NetworkMode.Client) return;
        
        faction = lastDamagedBy.faction;
        
        for (int i = 0; i < parts.Count; i++)
        {
            RemovePart(parts[i]);
        }


        Start();
        foreach (var part in parts)
        {
            part.Start();
        }
    }

    public EntityNetworkAdapter GetAdapter()
    {
        return networkAdapter;
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
            if (tile.HasValue && tile.Value != null)
            {
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
    }
}
