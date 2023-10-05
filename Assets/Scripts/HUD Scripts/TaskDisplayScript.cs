using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskDisplayScript : MonoBehaviour
{
    static TaskDisplayScript instance;

    public static Dictionary<string, int> rankNumberByString = new Dictionary<string, int>()
    {
        ["C"] = 0,
        ["B"] = 1,
        ["A"] = 2,
        ["S"] = 3,
        ["X"] = 4
    };

    public static Dictionary<string, Color> rankColorsByString = new Dictionary<string, Color>()
    {
        ["C"] = Color.cyan,
        ["B"] = Color.yellow,
        ["A"] = Color.red + Color.green / 2,
        ["S"] = Color.magenta,
        ["X"] = Color.red
    };

    public GameObject missionButtonPrefab;
    public GameObject missionObjectivePrefab;
    public Transform[] rankTexts;
    public Transform[] missionListContents;
    public Transform missionObjectivesContents;

    void OnEnable()
    {
        instance = this;
        Initialize();
    }

    public static void Initialize()
    {
        instance.rankHeader.transform.parent.gameObject.SetActive(false);
        foreach (var content in instance.missionListContents)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        instance.ClearMissionObjectivesSpace();
        loadedMissions.Clear();
        foreach (var mission in PlayerCore.Instance.cursave.missions)
        {
            AddMission(mission);
        }

        for (int i = 0; i < instance.missionListContents.Length; i++)
        {
            instance.rankTexts[i].gameObject.SetActive(instance.missionListContents[i].childCount != 0);
        }
    }

    public static bool EditMode;
    public static List<Mission> loadedMissions = new List<Mission>();

    public static void AddMission(Mission mission)
    {
        loadedMissions.Add(mission);
        if (mission.status == Mission.MissionStatus.Inactive && mission.prerequisites.Count > 0 && mission.prerequisites.TrueForAll(
            m => 
                !PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == m) ||
                PlayerCore.Instance.cursave.missions.Find(mi => mi.name == m).status != Mission.MissionStatus.Complete
            )) return;
        var button = Instantiate(instance.missionButtonPrefab,
            instance.missionListContents[rankNumberByString[mission.rank]]).GetComponent<Button>();
        if (mission.name.Length <= 33)
        {
            button.GetComponentInChildren<Text>().text = mission.name;
        }
        else
        {
            button.GetComponentInChildren<Text>().text = mission.name.Substring(0, 30) + "...";
        }

        switch (mission.status)
        {
            case Mission.MissionStatus.Inactive:
                button.GetComponentInChildren<Text>().color = Color.red;
                break;
            case Mission.MissionStatus.Ongoing:
                button.GetComponentInChildren<Text>().color = Color.cyan;
                break;
            case Mission.MissionStatus.Complete:
                button.GetComponentInChildren<Text>().color = Color.green;
                break;
        }
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            ShowMission(mission);
#if UNITY_EDITOR
            EditMode = true;
#else
#endif
            if (Input.GetKey(KeyCode.LeftShift) && EditMode)
            {
                mission.status = Mission.MissionStatus.Complete;
                mission.checkpoint = $"{mission.name}_complete";
                if (NodeEditorFramework.Standard.MissionCondition.OnMissionStatusChange != null)
                {
                    NodeEditorFramework.Standard.MissionCondition.OnMissionStatusChange.Invoke(mission);
                }
            }
        }));
    }

    public Text nameAndPrerequisitesHeader;
    public Text rankHeader;

    public static void ShowMission(Mission mission)
    {
        instance.ClearMissionObjectivesSpace();
        instance.nameAndPrerequisitesHeader.text = $"{mission.name}\n\nEntrypoint:\n{mission.entryPoint}\n\nPrerequisites:";
        instance.rankHeader.text = mission.rank;
        instance.rankHeader.transform.parent.gameObject.SetActive(true);
        instance.rankHeader.color = rankColorsByString[mission.rank];
        foreach (var prereq in mission.prerequisites)
        {
            instance.nameAndPrerequisitesHeader.text += $"\n{prereq}";
        }

        foreach (var task in mission.tasks)
        {
            var obj = Instantiate(instance.missionObjectivePrefab, instance.missionObjectivesContents, false);
            var strings = obj.GetComponentsInChildren<Text>();
            strings[0].text = task.dialogue;
            strings[1].text = task.objectived;
            strings[0].color = task.dialogueColor;

            if (task != mission.tasks[mission.tasks.Count - 1] || mission.status == Mission.MissionStatus.Complete)
            {
                strings[0].color /= 1.5F;
                strings[1].color /= 1.5F;
                obj.GetComponent<Image>().color /= 1.5F;
            }
        }

        if (mission.status == Mission.MissionStatus.Complete)
        {
            var obj = Instantiate(instance.missionObjectivePrefab, instance.missionObjectivesContents, false);
            var strings = obj.GetComponentsInChildren<Text>();
            strings[0].text = "Congratulations, Mission Complete!";
            strings[1].text = "";
            strings[0].color = Color.green;
        }

        Canvas.ForceUpdateCanvases();
    }

    public void ClearMissionObjectivesSpace()
    {
        for (int i = 1; i < missionObjectivesContents.childCount; i++)
        {
            Destroy(missionObjectivesContents.GetChild(i).gameObject);
        }
    }
}
