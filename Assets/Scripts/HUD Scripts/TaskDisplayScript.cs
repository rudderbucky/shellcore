using System.Collections;
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
        ["X"] = Color.green
    };

	public GameObject missionButtonPrefab;
    public GameObject missionObjectivePrefab;
	public Transform[] missionListContents;
    public Transform missionObjectivesContents;

    void OnEnable() {
        instance = this;
        Activate();
    }

    public static void Activate() {
        var test = new Mission();
        test.rank = "C";
        test.name = "Tutorial Circuit";
        test.status = Mission.MissionStatus.Inactive;
        test.prerequisites = new List<string>() {"None"};
        test.tasks = new List<Task>();
        var task = new Task();
        task.dialogueColor = Color.green;
        task.dialogue = "You are testing. Cool!";
        task.objectived = "-Test the Mission System";

        var task2 = new Task();
        task2.dialogueColor = Color.green;
        task2.dialogue = "You are testing again. Cooler!";
        task2.objectived = "-Test the Mission System again";

        test.tasks.Add(task);
        test.tasks.Add(task2);
        AddMission(test);
    }

    public static List<Mission> loadedMissions = new List<Mission>();
    public static void AddMission(Mission mission)
    {
        loadedMissions.Add(mission);
        var button = Instantiate(instance.missionButtonPrefab, 
            instance.missionListContents[rankNumberByString[mission.rank]]).GetComponent<Button>();
        button.GetComponentInChildren<Text>().text = mission.name;
        switch(mission.status)
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
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {ShowMission(mission);}));
    }

    public Text nameAndPrerequisitesHeader;
    public Text rankHeader;

    public static void ShowMission(Mission mission)
    {
        instance.ClearMissionObjectivesSpace();
        instance.nameAndPrerequisitesHeader.text = mission.name + "\n\nPrequisites:\n";
        instance.rankHeader.text = mission.rank;
        instance.rankHeader.transform.parent.gameObject.SetActive(true);
        instance.rankHeader.color = rankColorsByString[mission.rank];
        foreach(var prereq in mission.prerequisites)
        {
            instance.nameAndPrerequisitesHeader.text += prereq;
        }
        
        foreach(var task in mission.tasks)
        {
            var obj = Instantiate(instance.missionObjectivePrefab, instance.missionObjectivesContents, false);
            var strings = obj.GetComponentsInChildren<Text>();
            strings[0].text = task.dialogue;
            strings[1].text = task.objectived;
            strings[0].color = task.dialogueColor;

            if(task != mission.tasks[mission.tasks.Count - 1])
            {
                strings[0].color /= 1.5F;
                strings[1].color /= 1.5F;
                obj.GetComponent<Image>().color /= 1.5F; 
            }
        }
    }

    public void ClearMissionObjectivesSpace()
    {
        for(int i = 1; i < missionObjectivesContents.childCount; i++)
        {
            Destroy(missionObjectivesContents.GetChild(i).gameObject);
        }
    }
}
