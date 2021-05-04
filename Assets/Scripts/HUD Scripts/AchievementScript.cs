using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementScript : MonoBehaviour
{
    private int enemyKills;
    private int partsCollected;
    private int uniqueParts;
    private int sectorsExplored;
    private int creditGain;
    private int creditLoss;
    PlayerSave save;

    public void updateKills(){
        enemyKills = save.shellcoreKills;
    }
    public void updateParts(){
        partsCollected = save.partInventory.Count;
    }
    public void updateUniqueParts(){
        uniqueParts = save.partsObtained.Count;
    }
    public void updateCreditsGain(){
        creditGain = save.creditsGained;
    }
    public void updateCreditsSpend(){
        creditLoss = save.creditsGained - save.credits;
    }
    public void updateSectors(){
        sectorsExplored = save.sectorsSeen.Count;
    }
}