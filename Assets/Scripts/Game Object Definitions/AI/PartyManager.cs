﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

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
    private bool overrideLock;

    public bool PartyLocked
    {
        get { return SectorManager.instance.GetCurrentType() == Sector.SectorType.BattleZone || overrideLock; }
    }

    public void SetOverrideLock(bool val)
    {
        overrideLock = val;
    }

    public void OrderAttack()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Attack the enemy now!</color>", 1);
        foreach (var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].attackDialogue}</color>", 2);
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Attack);
            }
        }
    }

    public void OrderDefendStation()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Defend our station!</color>", 1);
        foreach (var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].defendDialogue}</color>", 2);
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Defend);
            }
        }
    }

    public void OrderCollection()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Collect more power.</color>", 1);
        foreach (var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].collectDialogue}</color>", 2);
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Collect);
            }
        }
    }

    public void OrderBuildTurrets()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Build Turrets!</color>", 1);
        foreach (var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].buildDialogue}</color>", 2);
                core.GetAI().setMode(AirCraftAI.AIMode.Battle);
                core.GetAI().ChatOrderStateChange(BattleAI.BattleState.Fortify);
            }
        }
    }

    public void OrderFollow()
    {
        PassiveDialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        PassiveDialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Follow me!</color>", 1);
        foreach (var core in partyMembers)
        {
            if (core && !core.GetIsDead())
            {
                PassiveDialogueSystem.Instance.PushPassiveDialogue(core.ID, $"<color=lime>{partyResponses[core.ID].followDialogue}</color>", 2);
                core.GetAI().follow(PlayerCore.Instance.transform);
            }
        }
    }

    public void AssignCharacter(string charID, Button assignButton)
    {
        if (partyMembers.Count >= 2)
        {
            PlayerCore.Instance.alerter.showMessage("Cannot assign more than 2 party members!", "clip_alert");
            return;
        }

        if (!PartyLocked)
        {
            AssignBackend(charID);

            assignButton.GetComponentInChildren<Text>().text = "UNASSIGN";
            var clicked = new Button.ButtonClickedEvent();
            clicked.AddListener(() => Unassign(charID));
            assignButton.onClick = clicked;
            // sukratHealth.SetActive(true);
        }
        else
        {
            PlayerCore.Instance.alerter.showMessage("Cannot modify party currently!", "clip_alert");
        }

        UpdatePortraits();
    }

    public void ClearParty()
    {
        partyMembers.Clear();
        foreach (var val in partyIndicators.Values)
        {
            if (val)
                Destroy(val);
        }
        partyIndicators.Clear();
        partyResponses.Clear();
    }

    public void AssignBackend(string charID)
    {
        if (partyMembers.Count >= 2)
        {
            PlayerCore.Instance.alerter.showMessage("Cannot assign more than 2 party members!", "clip_alert");
            return;
        }

        // check if it is a character
        foreach (var ch in SectorManager.instance.characters)
        {
            if (ch.ID == charID)
            {
                if (!AIData.entities.Exists(x => x.ID == charID))
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

                if (!partyResponses.ContainsKey(charID))
                {
                    partyResponses.Add(charID, ch.partyData);
                }

                break;
            }
        }

        var core = AIData.entities.Find(x => x.ID == charID) as ShellCore;
        if (partyMembers.Contains(core))
            return;

        PlayerCore.Instance.alerter.showMessage("PARTY MEMBER ASSIGNED", "clip_victory");
        partyMembers.Add(core);
        if (!partyIndicators.ContainsKey(core))
            partyIndicators.Add(core, Instantiate(partyIndicatorPrefab, indicatorTransform));
        partyIndicators[core].GetComponentInChildren<Text>().text = core.name.ToUpper();
    }

    public void Unassign(string charID)
    {
        if (!PartyLocked)
        {
            var member = partyMembers.Find(c => c.ID == charID);
            UnassignBackend(charID, member);
        }
        else
        {
            PlayerCore.Instance.alerter.showMessage("Cannot modify party currently!", "clip_alert");
        }
        UpdatePortraits();
    }


    public void UnassignBackend(string charID, ShellCore member)
    {
        if (member && member.GetAI() != null)
        {
            member.GetAI().follow(null);
        }

        partyMembers.Remove(member);
        if (charID != null) partyResponses.Remove(charID);

        if (partyIndicators.ContainsKey(member))
        {
            Destroy(partyIndicators[member]);
            partyIndicators.Remove(member);
        }
    }

    public GameObject partyIndicatorPrefab;
    public Dictionary<ShellCore, GameObject> partyIndicators = new Dictionary<ShellCore, GameObject>();
    public Transform indicatorTransform;

    private void AddOption(string name, UnityAction action)
    {
        var text = Instantiate(textPrefab, wheel.transform).GetComponent<Text>();
        text.text = name;
        texts.Add(text);
        options.Add(action);


        for (int i = 0; i < texts.Count; i++)
        {
            float angle = Mathf.Deg2Rad * i * 360f / texts.Count;
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
        // This method destroys all party bars and reconstructs it after assigning, for some reason.
        for (int i = 0; i < characterScrollContents.transform.childCount; i++)
        {
            Destroy(characterScrollContents.transform.GetChild(i).gameObject);
        }

        foreach (var id in PlayerCore.Instance.cursave.unlockedPartyIDs)
        {
            var inst = Instantiate(characterBarPrefab, characterScrollContents.transform).transform;
            var button = inst.Find("Assign").GetComponent<Button>();
            var name = inst.Find("Name").GetComponent<Text>();
            foreach (var ch in SectorManager.instance.characters)
            {
                if (ch.ID == id)
                {
                    name.text = ch.name.ToUpper();
                }
            }

            if (partyMembers.Exists(c => c.ID == id))
            {
                button.GetComponentInChildren<Text>().text = "UNASSIGN";
                button.onClick.AddListener(() => Unassign(id));
            }
            else
            {
                if (!PlayerCore.Instance.cursave.disabledPartyIDs.Contains(id))
                {
                    button.onClick.AddListener(() => AssignCharacter(id, button));
                }
                else
                {
                    button.GetComponentInChildren<Text>().text = "DISABLED";
                    button.GetComponentInChildren<Text>().color = Color.red;
                }
            }
        }
    }

    private int index = -1;
    private float partyMemberTeleportThreshold = 2500;

    void Update()
    {
        blocker.SetActive(false);
        var deadMembers = partyMembers.FindAll(sc => !sc);
        foreach (var member in deadMembers)
        {
            UnassignBackend(null, member);
        }

        // distance maximum for party members - teleport them close to the player
        if (SectorManager.instance?.current?.type != Sector.SectorType.BattleZone && !DialogueSystem.isInCutscene)
        {
            foreach (var member in partyMembers)
            {
                if (!member || !member.GetAI())
                {
                    continue;
                }

                if (member.GetAI().getMode() == AirCraftAI.AIMode.Follow &&
                    Vector3.SqrMagnitude(member.transform.position - PlayerCore.Instance.transform.position) > partyMemberTeleportThreshold)
                {
                    // Use warp to teleport AirCraft
                    member.Warp(PlayerCore.Instance.transform.position + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2)));
                }
            }
        }


        if (InputManager.GetKey(KeyName.CommandWheel) && partyMembers.Count > 0 && partyMembers.TrueForAll((member) => { return member; }))
        {
            wheel.SetActive(true);
            arrow.rotation = Quaternion.Euler(0, 0, Mathf.Atan2((Input.mousePosition.y - Camera.main.pixelHeight / 2),
                (Input.mousePosition.x - Camera.main.pixelWidth / 2)) * Mathf.Rad2Deg);
            var x = 90 - arrow.rotation.eulerAngles.z;
            if (x < -180)
            {
                x = 360 + x;
            }
            else if (x < 0)
            {
                x = 360 + x;
            }

            index = Mathf.RoundToInt((x) / (360 / options.Count));
        }
        else if (initialized)
        {
            if (index != -1)
            {
                if (index == 0 || index == options.Count)
                {
                    options[0].Invoke();
                }
                else
                {
                    options[index].Invoke();
                }

                index = -1;
            }

            wheel.SetActive(false);
        }

        foreach (var kvp in partyIndicators)
        {
            if (!kvp.Key || !kvp.Key.GetAI()) continue;
            kvp.Value.GetComponentsInChildren<Text>()[1].text = kvp.Key.GetAI().GetPartyBattleStateString();
            for (int i = 0; i < 3; i++)
            {
                float barWidth = 160;
                kvp.Value.GetComponentsInChildren<Image>()[i + 1].GetComponent<RectTransform>().sizeDelta =
                    new Vector2(barWidth * kvp.Key.CurrentHealth[i] / kvp.Key.GetMaxHealth()[i], 5);
            }
        }
    }

    public Transform[] portraits;

    public void UpdatePortraits()
    {
        portraits[0].GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(PlayerCore.Instance.blueprint, null, 0);
        portraits[0].GetComponentInChildren<Text>().text = PlayerCore.Instance.cursave.name.ToUpper();
        for (int i = 1; i < 3; i++)
        {
            if (i > partyMembers.Count)
            {
                portraits[i].GetComponentInChildren<SelectionDisplayHandler>().ClearDisplay();
                portraits[i].GetComponentInChildren<Text>().text = "";
            }
            else
            {
                portraits[i].GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(partyMembers[i - 1].blueprint, null, 0);
                portraits[i].GetComponentInChildren<Text>().text = partyMembers[i - 1].entityName.ToUpper();
            }
        }

        CharacterScrollSetup();
    }
}
