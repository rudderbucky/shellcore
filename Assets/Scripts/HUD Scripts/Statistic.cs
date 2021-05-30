using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statistic : MonoBehaviour
{
    private static int enemyKills;
    private static int partsCollected;
    private static int uniqueParts;
    private static int sectorsExplored;
    private static int creditGain;
    private static int creditLoss;

    public void updateStats(int index){
        PlayerSave save = GameObject.Find("player").GetComponent<PlayerCore>().cursave;
        switch (index){
        case 0:
        enemyKills = save.shellcoreKills;
        break;
        case 1:
        partsCollected = save.partInventory.Count;
        break;
        case 2:
        uniqueParts = save.partsObtained.Count;
        break;
        case 3:
        creditGain = save.creditsGained;
        break;
        case 4:
        creditLoss = save.creditsGained - save.credits;
        break;
        case 5:
        sectorsExplored = save.sectorsSeen.Count;
        break;
    }       
    }
    public int returnStats(int index){
        updateStats(index);
        switch (index){
        case 0:
        return enemyKills;
        case 1:
        return partsCollected;
        case 2:
        return uniqueParts;
        case 3:
        return sectorsExplored;
        case 4:
        return creditGain;
        case 5:
        return creditLoss;
        }
        return 0;
    }
}
[System.Serializable]
public class Achievement
{
    public string name;
    public string description;
    public bool completion;
    public float progress;
    public Color textColor;
}
