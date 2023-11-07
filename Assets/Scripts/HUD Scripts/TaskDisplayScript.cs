using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskDisplayScript : MonoBehaviour
{
    static TaskDisplayScript instance;
    public GameObject missionButtonPrefab;
    public GameObject missionObjectivePrefab;
    public Transform[] rankTexts;
    public Transform missionListContents;
    public Transform missionObjectivesContents;

    void OnEnable()
    {
        instance = this;
        Initialize();
    }

    public static void Initialize()
    {
        instance.rankHeader.transform.parent.gameObject.SetActive(false);
        for (int i = 0; i < instance.missionListContents.childCount; i++)
        {
            Destroy(instance.missionListContents.GetChild(i).gameObject);
        }

        instance.ClearMissionObjectivesSpace();
        loadedMissions.Clear();
        foreach (var mission in PlayerCore.Instance.cursave.missions)
        {
            AddMission(mission);
        }
    }

    public static bool EditMode;
    public static List<Mission> loadedMissions = new List<Mission>();

    public static Mission.MissionStatus GetMissionStatus(string missionName)
    {
        Mission m = null;
        var lmap = CoreScriptsManager.instance.GetLocalMapString(missionName);
        if (PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == missionName))
        {
            m = PlayerCore.Instance.cursave.missions.Find(mi => mi.name == missionName);
        }
        else if (PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == lmap))
        {
            m = PlayerCore.Instance.cursave.missions.Find(mi => mi.name == lmap);
        }
        else return Mission.MissionStatus.Inactive;
        return m.status;
    }

    public static Mission GetMission(string missionName)
    {
        Mission m = null;
        var lmap = CoreScriptsManager.instance.GetLocalMapString(missionName);
        if (PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == missionName))
        {
            m = PlayerCore.Instance.cursave.missions.Find(mi => mi.name == missionName);
        }
        else if (PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == lmap))
        {
            m = PlayerCore.Instance.cursave.missions.Find(mi => mi.name == lmap);
        }
        return m;
    }

    public static void AddMission(Mission mission)
    {
        loadedMissions.Add(mission);
        Func<string, bool> missionDoesNotExist = (missionName) => !PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == missionName);
        Func<string, bool> incompleteMissionLambda = (missionName) => PlayerCore.Instance.cursave.missions.Exists(mi => mi.name == missionName) &&
                PlayerCore.Instance.cursave.missions.Find(mi => mi.name == missionName).status != Mission.MissionStatus.Complete;

        if (mission.status == Mission.MissionStatus.Inactive && mission.prerequisites.Count > 0 && mission.prerequisites.TrueForAll(
            m => 
            {
                return (missionDoesNotExist(m) && missionDoesNotExist(CoreScriptsManager.instance.GetLocalMapString(m)))
                    || incompleteMissionLambda(m) || incompleteMissionLambda(CoreScriptsManager.instance.GetLocalMapString(m));
            }
            )) return;
        var button = Instantiate(instance.missionButtonPrefab,
            instance.missionListContents).GetComponent<Button>();
        var str = mission.useLocalMap ? CoreScriptsManager.instance.GetLocalMapString(mission.name) :  mission.name;
        if (mission.name.Length <= 33)
        {
            button.GetComponentInChildren<Text>().text = str;
        }
        else
        {
            button.GetComponentInChildren<Text>().text = str.Substring(0, 30) + "...";
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
                if (CoreScriptsManager.OnVariableUpdate != null)
                {
                    CoreScriptsManager.OnVariableUpdate.Invoke("MissionStatus(");
                }
            }
        }));
    }

    public Text nameAndPrerequisitesHeader;
    public Text rankHeader;

    public static void ShowMission(Mission mission)
    {
        instance.ClearMissionObjectivesSpace();
        var name = mission.useLocalMap ? CoreScriptsManager.instance.GetLocalMapString(mission.name) : mission.name;
        var entryPoint = mission.useLocalMap ? CoreScriptsManager.instance.GetLocalMapString(mission.entryPoint) : mission.entryPoint;
        instance.nameAndPrerequisitesHeader.text = $"{name}\n\nEntrypoint:\n{entryPoint}\n\nPrerequisites:";
        instance.rankHeader.transform.parent.gameObject.SetActive(true);
        foreach (var prereq in mission.prerequisites)
        {
            var prMission = PlayerCore.Instance.cursave.missions.Find((x) => { return x.name == prereq || (x.name == CoreScriptsManager.instance.GetLocalMapString(prereq)); });
            if (prMission == null) continue;
            var prName = prMission.useLocalMap ? CoreScriptsManager.instance.GetLocalMapString(prMission.name) : prMission.name;
            instance.nameAndPrerequisitesHeader.text += $"\n{(prName)}";
        }

        foreach (var task in mission.tasks)
        {
            var obj = Instantiate(instance.missionObjectivePrefab, instance.missionObjectivesContents, false);
            var strings = obj.GetComponentsInChildren<Text>();
            if (task.useLocalMap)
            {
                strings[0].text = CoreScriptsManager.instance.GetLocalMapString(task.dialogue);
                strings[1].text = CoreScriptsManager.instance.GetLocalMapString(task.objectived);
            }
            else
            {
                strings[0].text = task.dialogue;
                strings[1].text = task.objectived;
            }
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
