using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public Faction[] factions;
    public static List<Faction> defaultFactions;

    static FactionManager instance;

    int factionCount = 0;

    public static void UpdateFactions()
    {
        if (instance)
        {
            instance.Initialize();
        }
    }

    public void Initialize()
    {
        instance = this;

        var fs = ResourceManager.GetAssetsOfType<Faction>();

        int maxID = -1;
        bool playerFound = false;
        for (int i = 0; i < fs.Length; i++)
        {
            if (!isValid(fs[i]))
            {
                Debug.Log("Invalid faction: " + fs[i].factionName);
            }

            if (maxID < fs[i].ID)
            {
                maxID = fs[i].ID;
            }

            if (fs[i].ID == 0)
            {
                playerFound = true;
            }
        }

        if (!playerFound)
        {
            Debug.LogError("Player faction not loaded!");
        }

        if (maxID == -1)
        {
            Debug.LogError("No valid factions loaded!");
        }

        // This may leave some empty indices in-between, but it should be fast
        factions = new Faction[maxID + 1];
        for (int i = 0; i < fs.Length; i++)
        {
            if (factions[fs[i].ID] == null)
            {
                factionCount++;
            }

            factions[fs[i].ID] = fs[i];
        }
    }

    bool isValid(Faction f)
    {
        // validate faction data
        if (f.ID < 0 || f.ID >= 32)
        {
            return false;
        }

        return true;
    }

    public static bool FactionExists(int faction)
    {
        if (faction < 0 || faction >= 32 || instance.factions.Length <= faction)
        {
            return false;
        }

        return instance.factions[faction] != null;
    }

    public static string GetFactionName(int faction)
    {
        return instance.factions[faction].factionName;
    }

    public static Color GetFactionColor(int faction)
    {
        return instance.factions[faction].color;
    }

    public static Color GetFactionShinyColor(int faction)
    {
        return instance.factions[faction].shinyColor;
    }

    public static string GetFactionColorName(int faction)
    {
        return instance.factions[faction].colorName;
    }

    public static bool DoesFactionGrowRandomParts(int faction)
    {
        return instance.factions[faction].growRandomParts;
    }

    public static bool IsAllied(int faction1, int faction2)
    {
        return ((instance.factions[faction1].relations >> faction2) & 1) > 0;
    }

    public static int FactionArrayLength
    {
        get { return instance.factions.Length; }
    }

    public static void SetFactionRelations(int faction, int sum)
    {
        if (FactionExists(faction))
            instance.factions[faction].relations = sum;
    }

    public static int GetFactionRelations(int faction)
    {
        if (FactionExists(faction))
            return instance.factions[faction].relations;
        return -1;
    }
}
