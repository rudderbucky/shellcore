using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PartyManager : MonoBehaviour
{
    public List<ShellCore> partyMembers;
    public RectTransform arrow;
    public GameObject wheel;
    public List<UnityAction> options = new List<UnityAction>();
    public List<Text> texts = new List<Text>();
    public GameObject textPrefab;
    public static PartyManager instance;
    public GameObject blocker;
    public void OrderAttack()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Attack the enemy now!</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>DESTRUCTION!</color>");
            core.GetAI().setMode(AirCraftAI.AIMode.Battle);
            core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Attack);
        }
    }

    public void OrderDefendStation()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Defend our station!</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>Falling back!</color>");
            core.GetAI().setMode(AirCraftAI.AIMode.Battle);
            core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Defend);
        }
    }

    public void OrderCollection()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Collect more power.</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>I'm on it.</color>");
            core.GetAI().setMode(AirCraftAI.AIMode.Battle);
            core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Collect);
        }
    }

    public void OrderBuildTurrets()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Build Turrets!</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>Building!</color>");
            core.GetAI().setMode(AirCraftAI.AIMode.Battle);
            core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Fortify);
        }
    }

    public void OrderFollow()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Follow me!</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>Following!</color>");
            core.GetAI().follow(PlayerCore.Instance.transform);
        }
    }

    public Button sukratAssignButton;
    public void AssignSukrat()
    {
        if(SectorManager.instance.current.type != Sector.SectorType.BattleZone)
        {
            PlayerCore.Instance.alerter.showMessage("PARTY MEMBER ASSIGNED", "clip_victory");

            if(!AIData.entities.Exists(x => x.ID == "sukrat"))
            {
                // check if it is a character
                foreach(var ch in SectorManager.instance.characters)
                {
                    if(ch.ID == "sukrat")
                    {
                        var print = ScriptableObject.CreateInstance<EntityBlueprint>();
                        JsonUtility.FromJsonOverwrite(ch.blueprintJSON, print);
                        print.intendedType = EntityBlueprint.IntendedType.ShellCore;
                        var levelEnt = new Sector.LevelEntity();
                        levelEnt.ID = "sukrat";
                        levelEnt.name = ch.name;
                        levelEnt.faction = ch.faction;
                        levelEnt.position = PlayerCore.Instance.transform.position + new Vector3(0, 5);
                        SectorManager.instance.SpawnEntity(print, levelEnt);
                        break;
                    }
                }
            }

            partyMembers.Add(AIData.entities.Find(x => x.ID == "sukrat") as ShellCore);
            sukratAssignButton.GetComponentInChildren<Text>().text = "UNASSIGN";
            var clicked = new Button.ButtonClickedEvent();
            clicked.AddListener(Unassign);
            sukratAssignButton.onClick = clicked;
            sukratHealth.SetActive(true);
        }
        else PlayerCore.Instance.alerter.showMessage("Cannot modify party in BattleZone!", "clip_alert");

        UpdatePortraits();
    }

    public void Unassign()
    {
        if(SectorManager.instance.current.type != Sector.SectorType.BattleZone)
        {
            partyMembers.Clear();
            sukratAssignButton.GetComponentInChildren<Text>().text = "ASSIGN";
            var clicked = new Button.ButtonClickedEvent();
            clicked.AddListener(AssignSukrat);
            sukratAssignButton.onClick = clicked;
            sukratHealth.SetActive(false);
        }
        else PlayerCore.Instance.alerter.showMessage("Cannot modify party in BattleZone!", "clip_alert");

        UpdatePortraits();
    }

    public GameObject sukratHealth;

    private void AddOption(string name, UnityAction action)
    {
        var text = Instantiate(textPrefab, wheel.transform).GetComponent<Text>();
        text.text = name;
        texts.Add(text);
        options.Add(action);
         

        for(int i = 0; i < texts.Count; i++)
        {
            float angle = Mathf.Deg2Rad * i * 360f/texts.Count;
            Debug.Log(angle);
            texts[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(250 * Mathf.Sin(angle), 250 * Mathf.Cos(angle));
        }
    }

    bool initialized = false;
    void Start()
    {
        AddOption("Attack Enemy", OrderAttack);
        AddOption("Defend Stations", OrderDefendStation);
        AddOption("Collect Power", OrderCollection);
        AddOption("Build Turret", OrderBuildTurrets);
        AddOption("Follow Me", OrderFollow);
        sukratAssignButton.onClick.AddListener(AssignSukrat);
        instance = this;
        initialized = true;
    }

    private int index = -1;
    void Update()
    {

        blocker.SetActive(false);
        if(SectorManager.testJsonPath != null && !SectorManager.testJsonPath.Contains("main"))
        {
            blocker.SetActive(true);
            blocker.GetComponentInChildren<Text>().text = "Parties are unavailable in test worlds.";
        }

        if(!PlayerCore.Instance.cursave.missions.Exists(m => m.name == "Trial By Combat")
            || PlayerCore.Instance.cursave.missions.Find(m => m.name == "Trial By Combat").status != Mission.MissionStatus.Complete)
        {
            blocker.SetActive(true);
            blocker.GetComponentInChildren<Text>().text = "Party customization is unlocked after Trial By Combat.";
        }

        if(Input.GetKey(KeyCode.LeftControl) && partyMembers.Count > 0)
        {
            wheel.SetActive(true);
            arrow.rotation = Quaternion.Euler(0, 0, Mathf.Atan2((Input.mousePosition.y - Camera.main.pixelHeight / 2), 
                (Input.mousePosition.x - Camera.main.pixelWidth / 2)) * Mathf.Rad2Deg);
            var x = 90 - arrow.rotation.eulerAngles.z;
            if(x < -180)
            {
                x = 360 + x;
            }
            else if(x < 0)
            {
                x = 360 + x;
            }
            index = Mathf.RoundToInt((x) / (360/options.Count));
        }
        else if(initialized)
        {
            
            if(index != -1)
            {
                Debug.Log(index);
                if(index == 0 || index == options.Count) 
                {
                    options[0].Invoke();
                }
                else options[index].Invoke();
                index = -1;
            }
            wheel.SetActive(false);
        }

        if(sukratHealth.activeSelf)
        {
            for(int i = 0; i < 3; i++)
            {
                sukratHealth.GetComponentsInChildren<Image>()[i+1].GetComponent<RectTransform>().sizeDelta = 
                    new Vector2(100 * partyMembers[0].currentHealth[i] / partyMembers[0].GetMaxHealth()[i], 5);
            }
        }
    }

    public Transform[] portraits;

    public void UpdatePortraits()
    {
        portraits[0].GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(PlayerCore.Instance.blueprint, null, 0);
        portraits[0].GetComponentInChildren<Text>().text = PlayerCore.Instance.cursave.name.ToUpper();
        for(int i = 1; i < 3; i++)
        {
            if(i > partyMembers.Count) 
            {
                portraits[i].GetComponentInChildren<SelectionDisplayHandler>().ClearDisplay();
                portraits[i].GetComponentInChildren<Text>().text = "";
            }
            else 
            {
                portraits[i].GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(partyMembers[i-1].blueprint, null, 0);
                portraits[i].GetComponentInChildren<Text>().text = partyMembers[i-1].entityName.ToUpper();
            }
        }
    }

}
