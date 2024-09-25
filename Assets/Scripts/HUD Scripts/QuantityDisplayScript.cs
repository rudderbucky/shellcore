using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuantityDisplayScript : MonoBehaviour
{
    private PlayerCore player;
    private bool initialized;
    public GameObject targetInfo;
    public GameObject secondaryTargetInfoPrefab;
    int lastCredits;
    public CreditIncrementMarker marker;

    private TooltipManager tooltipManager;
    // Use this for initialization

    public void Initialize(PlayerCore player)
    {
        this.player = player;
        initialized = true;
        tooltipManager = gameObject.GetComponent<TooltipManager>();
        if (!tooltipManager)
        {
            tooltipManager = gameObject.AddComponent<TooltipManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (initialized)
        {
            if (lastCredits != player.GetCredits())
            {
                int diff = player.GetCredits() - lastCredits;
                marker.DisplayText(diff);
            }

            lastCredits = player.GetCredits();
            var texts = GetComponentsInChildren<UnityEngine.UI.Text>();
            texts[1].text = player.GetPower().ToString();
            texts[3].text = player.unitsCommanding.Count + "/" + player.GetTotalCommandLimit();
            texts[5].text = GetValueString(player.GetCredits()).ToString();
            var rect = texts[5].rectTransform.rect;
            rect.center = texts[5].rectTransform.position;
            tooltipManager.AddBounds(rect, $"CREDITS: {player.GetCredits()}");

            UpdatePrimaryTargetInfo();

            foreach (var infos in secondaryInfosByEntity)
            {
                UpdateInfo(infos.Key != null && !infos.Key.Equals(null) ? infos.Key.gameObject : null, infos.Value);
            }
        }
    }

    private Dictionary<Transform, GameObject> secondaryInfosByEntity = new Dictionary<Transform, GameObject>();
    public Transform content;

    public void AddSecondaryInfo(Transform target, ReticleScript reticle)
    {
        var secondary = Instantiate(secondaryTargetInfoPrefab, content);
        secondary.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction
        (() =>
        {
            var targSys = PlayerCore.Instance.GetTargetingSystem();
            if (targSys.GetTarget() && targSys.GetTarget().GetComponent<Entity>())
            {
                reticle.AddSecondaryTarget(targSys.GetTarget());
            }

            reticle.SetTarget(target);
            reticle.RemoveSecondaryTarget(target);
        }));

        if (!secondaryInfosByEntity.ContainsKey(target))
        {
            secondaryInfosByEntity.Add(target, secondary);
        }
        else
        {
            if (secondaryInfosByEntity[target])
            {
                Destroy(secondaryInfosByEntity[target].gameObject);
            }

            secondaryInfosByEntity[target] = secondary;
        }
    }

    public static string GetValueString(int credits)
    {
        if (credits < 100000)
        {
            return $"{credits}";
        }
        else if (credits < 1000000)
        {
            return $"{credits / 1000}K";
        }
        else if (credits < 1000000000)
        {
            return $"{credits / 1000000}M";
        }
        else
        {
            return "LOTS!";
        }
    }

    public void RemoveEntityInfo(Transform entity)
    {
        if (secondaryInfosByEntity.ContainsKey(entity))
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
        if (targetInfo.transform.Find("Number"))
        {
            targetNumber = targetInfo.transform.Find("Number").GetComponent<Text>();
        }

        if (obj == null)
        {
            targetName.text = targetDesc.text = "";
            targetInfo.SetActive(false);
            return;
        }

        var entity = obj.GetComponent<Entity>();
        if (entity)
        {
            targetInfo.SetActive(true);
            description = entity.Terrain + " " + entity.Category;
            targetName.text = entity.entityName + (ReticleScript.instance.DebugMode ? $" ({entity.ID})" : "");
            targetDesc.text = description;
            targetName.color = targetDesc.color = FactionManager.GetFactionColor(entity.faction.factionID);
        }
        else if (obj.GetComponent<ShellPart>())
        {
            var info = obj.GetComponent<ShellPart>().info;
            targetInfo.SetActive(true);
            if (PartIndexScript.CheckPartObtained(info))
            {
                targetName.text = info.partID;
                targetDesc.text = AbilityUtilities.GetAbilityNameByID(info.abilityID, info.secondaryData);
                if (info.tier != 0)
                    targetDesc.text += " " + info.tier;
                targetName.color = targetDesc.color = FactionManager.GetFactionColor(obj.GetComponent<ShellPart>().GetFaction());
            }
            else
            {
                targetName.text = "Unobtained Part";
                targetDesc.text = "Bring to Yard";
                targetName.color = targetDesc.color = FactionManager.GetFactionColor(obj.GetComponent<ShellPart>().GetFaction());
            }
        }
        else if (obj.GetComponent<ShardRock>() || obj.GetComponent<Shard>())
        {
            if (obj.GetComponent<Shard>())
            {
                targetName.text = "Shard";
                targetDesc.text = "Loot";
            }
            else
            {
                targetName.text = "Shard Rock";
                targetDesc.text = "";
            }
            targetDesc.color = targetName.color = new Color32(51, 153, 204, 255);
            targetInfo.SetActive(true);
        }
        else
        {
            targetName.text = targetDesc.text = "";
            targetInfo.SetActive(false);
        }


        if (targetNumber)
        {
            targetNumber.color = targetName.color;
            targetNumber.text = (ReticleScript.instance.GetTargetIndex(obj.transform) + 1).ToString();
        }
    }
}
