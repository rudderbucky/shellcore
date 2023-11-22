using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IntRect
{
    public int x, y, w, h;

    public IntRect(int X, int Y, int W, int H)
    {
        x = X;
        y = Y;
        w = W;
        h = H;
    }

    public bool contains(Vector2 position)
    {
        return position.x >= x && position.x <= x + w && position.y <= y && position.y >= y - h;
    }
}

[CreateAssetMenu(fileName = "Sector", menuName = "ShellCore/Sector", order = 7)]
public class Sector : ScriptableObject
{
    [System.Serializable]
    public struct LevelEntity
    {
        public string name;
        public string ID;
        public int faction;
        public string assetID;
        public string blueprintJSON;
        public string dialogueID;
        public string vendingID;
        public string pathID;
        public Vector2 position;
        public NodeEditorFramework.Standard.PathData patrolPath;
    }

    public enum SectorType
    {
        Neutral,
        Haven,
        BattleZone,
        DangerZone,
        Capitol,
        DarkNeutral,
        SiegeZone
    }

    public struct SectorData
    {
        public string sectorjson;
        public string platformjson;
    }

    public int dimension;
    public string sectorName;
    public IntRect bounds;
    public SectorType type;
    public Color backgroundColor;
    public LevelEntity[] entities;
    public string[] platformData;
    public LandPlatform platform;
    public string[] targets;
    public bool hasMusic;
    public string musicID;
    public bool partDropsDisabled = false;

    [System.NonSerialized]
    public GroundPlatform[] platforms;

    [System.NonSerialized]
    public List<GroundPlatform.Tile> tiles;

    [System.Serializable]
    public struct BackgroundSpawn
    {
        public LevelEntity entity;
        public int timePerSpawn;
        public float radius;
    }

    public BackgroundSpawn[] backgroundSpawns;
    public string waveSetPath;
    public int gasVortices;
    public RectangleEffectSkin rectangleEffectSkin;
    public BackgroundTileSkin backgroundTileSkin;

    public int[] shardCountSet = new int[3] {0, 0, 0};
}
