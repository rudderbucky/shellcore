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
    private Dictionary<string, WorldData.PartyData> partyResponses = new Dictionary<string, WorldData.PartyData>();
    public void OrderAttack()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Attack the enemy now!</color>");
        foreach(var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].attackDialogue}</color>");
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Attack);
            }
        }
    }

    public void OrderDefendStation()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Defend our station!</color>");
        foreach(var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].defendDialogue}</color>");
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Defend);
            }
        }
    }

    public void OrderCollection()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Collect more power.</color>");
        foreach(var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].collectDialogue}</color>");
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Collect);
            }
        }
    }

    public void OrderBuildTurrets()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Build Turrets!</color>");
        foreach(var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].buildDialogue}</color>");
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Fortify);
            }
        }
    }

    public void OrderFollow()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Follow me!</color>");
        foreach(var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].followDialogue}</color>");
                core.GetAI().follow(PlayerCore.Instance.transform);
            }
        }
    }
    public void AssignCharacter(string charID, Button assignButton)
    {
        if(partyMembers.Count >= 2)
        {
            PlayerCore.Instance.alerter.showMessage("Cannot assign more than 2 party members!", "clip_alert");
            return;
        }

        if(SectorManager.instance.GetCurrentType() != Sector.SectorType.BattleZone)
        {
            AssignBackend(charID);

            assignButton.GetComponentInChildren<Text>().text = "UNASSIGN";
            var clicked = new Button.ButtonClickedEvent();
            clicked.AddListener(() => Unassign(charID, assignButton));
            assignButton.onClick = clicked;
            // sukratHealth.SetActive(true);
        }
        else PlayerCore.Instance.alerter.showMessage("Cannot modify party in BattleZone!", "clip_alert");

        UpdatePortraits();
    }

    public void ClearParty()
    {
        partyMembers.Clear();
        partyResponses.Clear();
    }

    public void AssignBackend(string charID)
    {
        if(partyMembers.Count >= 2)
        {
            PlayerCore.Instance.alerter.showMessage("Cannot assign more than 2 party members!", "clip_alert");
            return;
        }

        PlayerCore.Instance.alerter.showMessage("PARTY MEMBER ASSIGNED", "clip_victory");

        // check if it is a character
        foreach(var ch in SectorManager.instance.characters)
        {
            if(ch.ID == charID)
            {
                if(!AIData.entities.Exists(x => x.ID == charID))
                {
                    var print = ScriptableObject.CreateInstance<EntityBlueprint>();
                    JsonUtility.FromJsonOverwrite(ch.blueprintJSON, print);
                    print.intendedType = EntityBlueprint.IntendedType.ShellCore;
                    var levelEnt = new Sector.LevelEntity();
                    levelEnt.ID = charID;
                    levelEnt.name = ch.name;
                    levelEnt.faction = ch.faction;
                    levelEnt.position = PlayerCore.Instance.transform.position + new Vector3(0, 5);
                    SectorManager.instance.SpawnEntity(print, levelEnt);
                }

                if(!partyResponses.ContainsKey(charID))
                    partyResponses.Add(charID, ch.partyData);

                break;
            }
        }

        partyMembers.Add(AIData.entities.Find(x => x.ID == charID) as ShellCore);
    }

    public void Unassign(string charID, Button assignButton)
    {
        if(SectorManager.instance.GetCurrentType() != Sector.SectorType.BattleZone)
        {
            var member = partyMembers.Find(c => c.ID == charID);
            if(member && member.GetAI() != null)
                member.GetAI().follow(null);
            partyMembers.Remove(member);
            partyResponses.Remove(charID);
            var clicked = new Button.ButtonClickedEvent();
            clicked.AddListener(() => AssignCharacter(charID, assignButton));
            // sukratHealth.SetActive(false);
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
            texts[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(250 * Mathf.Sin(angle), 250 * Mathf.Cos(angle));
        }
    }

    bool initialized = false;
    public GameObject characterBarPrefab;
    public GameObject characterScrollContents;
    void Start()
    {
        AddOption("Attack Enemy", OrderAttack);
        AddOption("Defend Stations", OrderDefendStation);
        AddOption("Collect Power", OrderCollection);
        AddOption("Build Turret", OrderBuildTurrets);
        AddOption("Follow Me", OrderFollow);
        instance = this;
        initialized = true;
    }

    public void CharacterScrollSetup()
    {
        for(int i = 0; i < characterScrollContents.transform.childCount; i++)
        {
            Destroy(characterScrollContents.transform.GetChild(i).gameObject);
        }

        foreach(var id in PlayerCore.Instance.cursave.unlockedPartyIDs)
        {
            var inst = Instantiate(characterBarPrefab, characterScrollContents.transform).transform;
            var button = inst.Find("Assign").GetComponent<Button>();
            var name = inst.Find("Name").GetComponent<Text>();
            foreach(var ch in SectorManager.instance.characters)
            {
                if(ch.ID == id)
                    name.text = ch.name.ToUpper();
            }
            
            if(partyMembers.Exists(c => c.ID == id))
            {
                button.GetComponentInChildren<Text>().text = "UNASSIGN";
                button.onClick.AddListener(() => Unassign(id, button));
            }
            else 
                button.onClick.AddListener(() => AssignCharacter(id, button));
        }
    }

    private int index = -1;
    void Update()
    {

        blocker.SetActive(false);

        // distance maximum for party members - teleport them close to the player
        if(SectorManager.instance?.current?.type != Sector.SectorType.BattleZone && !DialogueSystem.isInCutscene)
            foreach(var member in partyMembers)
            {
                if(member.GetAI().getMode() == AirCraftAI.AIMode.Follow &&
                Vector3.SqrMagnitude(member.transform.position - PlayerCore.Instance.transform.position) > 2500)
                {
                    // Use warp to teleport AirCraft
                    member.Warp(PlayerCore.Instance.transform.position + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2)));
                }   
            }


        if(InputManager.GetKey(KeyName.CommandWheel) && partyMembers.Count > 0 && partyMembers.TrueForAll((member)=> { return member; }))
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

        CharacterScrollSetup();
    }

}
