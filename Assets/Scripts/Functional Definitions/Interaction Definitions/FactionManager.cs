using UnityEngine;
using System.Collections;

public class FactionManager : MonoBehaviour
{
    public Faction[] factions;

    static FactionManager instance;

    int factionCount = 0;

    public static void UpdateFactions()
    {
        if (instance)
            instance.Initialize();
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
                Debug.Log("Invalid faction: " + fs[i].factionName);

            if (maxID < fs[i].ID)
                maxID = fs[i].ID;

            if (fs[i].ID == 0)
                playerFound = true;
        }
        if (!playerFound)
            Debug.LogError("Player faction not loaded!");
        if (maxID == -1)
            Debug.LogError("No valid factions loaded!");

        Debug.Log("Factions found: " + fs.Length + ", max ID: " + maxID);

        // This may leave some empty indices in-between, but it should be fast
        factions = new Faction[maxID + 1];
        for (int i = 0; i < fs.Length; i++)
        {
            if (factions[fs[i].ID] != null)
                Debug.LogWarning("Faction ID conflict! [" + fs[i].ID + "]");
            else
                factionCount++;
            factions[fs[i].ID] = fs[i];
        }
    }

    bool isValid(Faction f)
    {
        // validate faction data
        if (f.ID < 0 || f.ID >= 32)
            return false;
        return true;
    }

    public static bool FactionExists(int faction)
    {
        if (faction < 0 || faction >= 32)
            return false;
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

    public static bool IsAllied(int faction1, int faction2)
    {
        return ((instance.factions[faction1].relations >> faction2) & 1) > 0;
    }

    public static int FactionCount
    {
        get { return instance.factions.Length; }
    }
}
