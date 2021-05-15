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
            if(lastCredits != player.GetCredits()) {
                int diff = player.GetCredits() - lastCredits;
                marker.DisplayText(diff);
            }
            lastCredits = player.GetCredits();
            var texts = GetComponentsInChildren<UnityEngine.UI.Text>();
            texts[1].text = player.GetPower() + "";
            texts[3].text = player.unitsCommanding.Count + "/" + player.GetTotalCommandLimit();
            texts[5].text = GetCreditString(player.GetCredits()) + "";

            UpdatePrimaryTargetInfo();

            foreach(var infos in secondaryInfosByEntity)
            {
                UpdateInfo(infos.Key ? infos.Key.gameObject : null, infos.Value);
            }
        }
	}

    private Dictionary<Entity, GameObject> secondaryInfosByEntity = new Dictionary<Entity, GameObject>();
    public Transform content;
    public void AddEntityInfo(Entity entity, ReticleScript reticle)
    {
        var secondary = Instantiate(secondaryTargetInfoPrefab, content);
        secondary.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction
        (() => 
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                reticle.RemoveSecondaryTarget(entity);
            }
            else
            {
                var targSys = PlayerCore.Instance.GetTargetingSystem();
                if(targSys.GetTarget() && targSys.GetTarget().GetComponent<Entity>())
                {
                    reticle.AddSecondaryTarget(targSys.GetTarget().GetComponent<Entity>());
                }

                reticle.SetTarget(entity.transform);
                reticle.RemoveSecondaryTarget(entity);
            }

        }));
        
        if(!secondaryInfosByEntity.ContainsKey(entity))
            secondaryInfosByEntity.Add(entity, secondary);
        else
        {
            if(secondaryInfosByEntity[entity]) Destroy(secondaryInfosByEntity[entity].gameObject);
            secondaryInfosByEntity[entity] = secondary;
        }
    }

    public static string GetCreditString(int credits)
    {
        if(credits < 100000) return $"{credits}";
        else if(credits < 1000000)
            return $"{credits / 1000}K";
        else if(credits < 1000000000)
            return $"{credits / 1000000}M";
        else
            return "LOTS!";
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
        UpdateInfo(targ ? targ.gameObject : null, targetInfo);
    }

    public void UpdateInfo(GameObject obj, GameObject targetInfo, int index = 0)
    {
        string description;
        var targetName = targetInfo.transform.Find("Target Name").GetComponent<Text>();
        var targetDesc = targetInfo.transform.Find("Name").GetComponent<Text>();
        Text targetNumber = null;
        if(targetInfo.transform.Find("Number")) targetNumber = targetInfo.transform.Find("Number").GetComponent<Text>();

        if(obj == null)
        {
            targetName.text = targetDesc.text = "";
            targetInfo.SetActive(false);
            return;
        }

        var entity = obj.GetComponent<Entity>();
        if(entity) {
            targetInfo.SetActive(true);
            description = (entity.Terrain + " ");
            description += (entity.category + "");
            targetName.text = entity.entityName;
            targetDesc.text = description;
            targetName.color = targetDesc.color = FactionManager.GetFactionColor(entity.faction);
            if(targetNumber) 
            {
                targetNumber.color = targetName.color;
                targetNumber.text = ReticleScript.instance.GetTargetIndex(entity) + 1 + "";
                // targetShape.rectTransform.sizeDelta = targetShape.rectTransform.sizeDelta / 1.25F;
            }
        } 
        else if(obj.GetComponent<ShellPart>()) 
        {
            var info = obj.GetComponent<ShellPart>().info;
            targetInfo.SetActive(true);
            if(PartIndexScript.CheckPartObtained(info))
            {
                targetName.text = info.partID;
                targetDesc.text = AbilityUtilities.GetAbilityNameByID(info.abilityID, null) + " " + info.tier;
            }
            else
            {
                targetName.text = "Unobtained Part";
                targetDesc.text = "Bring to Yard";
                targetName.color = targetDesc.color = FactionManager.GetFactionColor(obj.GetComponent<ShellPart>().GetFaction());
            }
        } else
        {
            targetName.text = targetDesc.text = "";
            targetInfo.SetActive(false);
        }
    }
}
