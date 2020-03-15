using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuantityDisplayScript : MonoBehaviour {

    private PlayerCore player;
    private bool initialized;
    public GameObject targetInfo;
    public GameObject secondaryTargetInfoPrefab;
    int lastCredits;
    public CreditIncrementMarker marker;
	// Use this for initialization

    public void Initialize(PlayerCore player)
    {
        this.player = player;
        initialized = true;
    }
	// Update is called once per frame
	void Update () {
        if (initialized)
        {
            if(lastCredits != player.credits) {
                int diff = player.credits - lastCredits;
                marker.DisplayText(diff);
            }
            lastCredits = player.credits;
            var texts = GetComponentsInChildren<UnityEngine.UI.Text>();
            texts[1].text = player.GetPower() + "";
            texts[3].text = player.unitsCommanding.Count + "/" + player.GetTotalCommandLimit();
            texts[5].text = player.credits + "";

            UpdatePrimaryTargetInfo();

            foreach(var infos in secondaryInfosByEntity)
            {
                UpdateInfo(infos.Key, infos.Value);
            }
        }
	}

    private Dictionary<Entity, GameObject> secondaryInfosByEntity = new Dictionary<Entity, GameObject>();
    public Transform content;
    public void AddEntityInfo(Entity entity)
    {
        var secondary = Instantiate(secondaryTargetInfoPrefab, content);
        secondaryInfosByEntity.Add(entity, secondary);
    }

    public void RemoveEntityInfo(Entity entity)
    {
        if(secondaryInfosByEntity.ContainsKey(entity))
        {
            Destroy(secondaryInfosByEntity[entity]);
            secondaryInfosByEntity.Remove(entity);
        }
    }

    public void UpdatePrimaryTargetInfo()
    {
        var targ = player.GetTargetingSystem().GetTarget();
        UpdateInfo(targ ? targ.GetComponent<Entity>() : null, targetInfo);
    }

    public void UpdateInfo(Entity entity, GameObject targetInfo, int index = 0)
    {
        string description;
        var targetName = targetInfo.transform.Find("Target Name").GetComponent<Text>();
        var targetDesc = targetInfo.transform.Find("Name").GetComponent<Text>();
        var targetDist = targetInfo.transform.Find("Distance").GetComponent<Text>();
        Image targetShape = null;
        if(targetInfo.transform.Find("Shape")) targetShape = targetInfo.transform.Find("Shape").GetComponent<Image>();

        if(entity) {
            targetInfo.SetActive(true);
            description = (entity.Terrain + " ");
            description += (entity.category + "");
            targetName.text = entity.entityName;
            targetDesc.text = description;
            targetDist.text = "Distance: " + (int)(entity.transform.position - player.transform.position).magnitude;
            targetName.color = targetDesc.color = targetDist.color = FactionColors.colors[entity.faction];
            if(targetShape) 
            {
                targetShape.color = targetName.color;
                targetShape.sprite = ReticleScript.instance.shapeArray[ReticleScript.instance.GetTargetIndex(entity)];
                targetShape.SetNativeSize();
                // targetShape.rectTransform.sizeDelta = targetShape.rectTransform.sizeDelta / 1.25F;
            }
        } else {
            targetName.text = targetDesc.text = targetDist.text = "";
            targetInfo.SetActive(false);
        }
    }
}
