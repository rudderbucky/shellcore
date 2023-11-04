using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveDialogueSystem : MonoBehaviour
{
    public RectTransform passiveDialogueRect;
    public CanvasGroup passiveDialogueCanvasGroup;
    public GameObject passiveDialogueInstancePrefab;
    public RectTransform passiveDialogueContents;
    public Text passiveDialogueText;
    private DialogueSystem.DialogueState passiveDialogueState = DialogueSystem.DialogueState.Out;
    public RectTransform passiveDialogueScrollView;
    private struct PassiveDialogue
    {
        public string id;
        public string text;
        public int soundType;
        public bool useEntityColor;
    }

    Queue<PassiveDialogue> passiveMessages = new Queue<PassiveDialogue>();
    public GameObject passiveDialogueArchive;
    public Transform archiveContents;
    public static PassiveDialogueSystem Instance;

    private void Awake()
    {
        Instance = this;
        if (archiveContents)
        {
            archiveContents.transform.parent.gameObject.SetActive(false);
        }

        if (passiveDialogueScrollView)
        {
            passiveDialogueScrollView.localScale = new Vector3(1, 0, 1);
        }
    }

    public void SlidePassiveDialogueOut()
    {
        passiveDialogueState = DialogueSystem.DialogueState.Out;
        StartCoroutine("SlideDialogueOut");
    }

    IEnumerator SlidePassiveDialogueIn(Transform transform)
    {
        float count = transform.localScale.y;
        while (count < 1)
        {
            if (passiveDialogueState != DialogueSystem.DialogueState.In)
            {
                break;
            }

            count += 0.05F;
            transform.localScale += new Vector3(0, 0.05F);
            yield return new WaitForSeconds(0.0025F);
        }
    }

    IEnumerator FadePassiveDialogueOut()
    {
        float count = passiveDialogueScrollView.localScale.y;
        passiveDialogueState = DialogueSystem.DialogueState.Out;
        while (count > 0F)
        {
            if (passiveDialogueState != DialogueSystem.DialogueState.Out)
            {
                break;
            }

            count -= 0.05F;
            passiveDialogueScrollView.localScale -= new Vector3(0, 0.05F, 0);
            yield return new WaitForSeconds(0.0025F);
        }

        while (passiveDialogueContents.childCount > 0)
        {
            passiveDialogueContents.GetChild(0).localScale = new Vector3(1, 1, 1);
            passiveDialogueContents.GetChild(0).SetParent(archiveContents, false);
        }
    }

    public void PushPassiveDialogue(string id, string text, int soundType, bool useEntityColor = false)
    {
        if (passiveDialogueState != DialogueSystem.DialogueState.In)
        {
            passiveDialogueState = DialogueSystem.DialogueState.In;
        }

        var pd = new PassiveDialogue();
        pd.id = id;
        pd.text = text;
        pd.soundType = soundType;
        pd.useEntityColor = useEntityColor;
        passiveMessages.Enqueue(pd);
    }

    float queueTimer = 0;

    public void ResetPassiveDialogueQueueTime()
    {
        queueTimer = 0;
    }

    void Update()
    {
        queueTimer -= Time.deltaTime;
        if (passiveMessages.Count > 0)
        {
            archiveContents.transform.parent.gameObject.SetActive(false);
            passiveDialogueScrollView.localScale = new Vector3(1, 1, 1);
            if (queueTimer <= 0)
            {
                queueTimer = 3;
                var dialogue = passiveMessages.Dequeue();
                Entity speaker = AIData.entities.Find(e => e.GetID() == dialogue.id);
                int sType = dialogue.soundType;
                if (sType > 0 && sType <= 13)
                {
                    AudioManager.PlayClipByID($"clip_passiveDialogue{sType}", false, 2.5F);
                }

                if (speaker != null)
                {
                    var instance = Instantiate(passiveDialogueInstancePrefab, passiveDialogueContents);
                    var name = speaker.name;
                    if (speaker is PlayerCore player)
                    {
                        name = player.cursave.name;
                    }

                    instance.transform.Find("Name").GetComponent<Text>().text = name;
                    var text = instance.transform.Find("Text").GetComponent<Text>();
                    text.text = dialogue.text;
                    if (dialogue.useEntityColor) 
                    {
                        text.color = FactionManager.GetFactionColor(speaker.faction);
                    }
                    else
                    {
                        text.color = Color.white;
                    }

                    instance.transform.localScale -= new Vector3(0, 1);
                    StartCoroutine(SlidePassiveDialogueIn(instance.transform));

                    instance.GetComponentInChildren<SelectionDisplayHandler>().AssignDisplay(speaker.blueprint, null, speaker.faction);
                }
            }
        }
        else if (queueTimer <= -2 && passiveDialogueState != DialogueSystem.DialogueState.Out)
        {
            StartCoroutine(FadePassiveDialogueOut());
        }

        if (passiveDialogueContents != null && passiveDialogueContents.transform.childCount == 0 && InputManager.GetKeyDown(KeyName.ShowChatHistory)
            && !PlayerCore.Instance.GetIsInteracting())
        {
            var par = archiveContents.transform.parent.gameObject;
            par.SetActive(!par.activeSelf);
        }
    }
}
