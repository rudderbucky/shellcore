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
    public void OrderAttack()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Attack the enemy now!</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>DESTRUCTION!</color>");
        }
    }

    public void OrderDefendStation()
    {
        DialogueSystem.Instance.ResetPassiveDialogueQueueTime();
        DialogueSystem.Instance.PushPassiveDialogue("player", "<color=lime>Defend our station!</color>");
        foreach(var core in partyMembers)
        {
            if(core.ID == "sukrat")
                DialogueSystem.Instance.PushPassiveDialogue("sukrat", "<color=lime>Falling back! Speed Thrust!</color>");
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

    public void AssignSukrat()
    {
        partyMembers.Add(AIData.entities.Find(x => x.ID == "sukrat") as ShellCore);
    }

    public void Unassign()
    {
        partyMembers.Clear();
    }

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
        
        initialized = true;
    }

    private int index = -1;
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftControl))
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
            Debug.Log(x);
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
    }
}
