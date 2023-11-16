using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FusionStationScript : MonoBehaviour
{
    public FusionStationInventoryScript finalPart;
    protected Transform[] contentsArray; // holds scroll view sub-sections by part size
    protected GameObject[] contentTexts;
    public ShipBuilderInventoryScript buttonPrefab;

    // Start is called before the first frame update
    void Start()
    {

        foreach (var part in PlayerCore.Instance.parts)
        {
            int size = ResourceManager.GetAsset<PartBlueprint>(part.info.partID).size;
            ShipBuilderInventoryScript dictInvButton = Instantiate(buttonPrefab,
            contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
        }
        
        for (int i = 0; i < contentTexts.Length; i++)
        {
            contentTexts[i].SetActive(contentsArray[i].childCount > 0);
        }
    }

    public void Fuse()
    {
        var pi = new EntityBlueprint.PartInfo();
        pi.partID = "MediumCenter2";
        pi.abilityID = 4;
        finalPart.part = pi;
        finalPart.Restart();
    }
}
