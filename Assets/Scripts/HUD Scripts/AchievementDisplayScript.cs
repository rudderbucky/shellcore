using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementDisplayScript : MonoBehaviour
{
    static AchievementDisplayScript instance;
	public GameObject buttonPrefab;
    public GameObject objectivePrefab;
	public Transform[] achievementListContents;
    public Transform achievementCompletionContents;
    

    void OnEnable() {
        instance = this;
        Initialize();
    }

    public static void Initialize() {
        foreach(var content in instance.achievementListContents)
        {
            for(int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }

        instance.ClearAchievementObjectivesSpace();
        loadedAchievements.Clear();
        AddStatistic(0,"ShellCore Kills","killed","ShellCores");
        AddStatistic(1,"Parts Collected","collected","parts");
        AddStatistic(2,"Unique Parts Collected","collected","unique parts");
        AddStatistic(3,"Sectors Explored","explored","sectors");
        AddStatistic(4,"Credits Earned","earned","credits");
        AddStatistic(5,"Credits Spent","spent","credits");
        foreach(var achievement in PlayerCore.Instance.cursave.achievements)
        {
            AddAchievement(achievement);
        }
    }

    public static List<Achievement> loadedAchievements = new List<Achievement>();
    public static void AddStatistic(int index, string name, string verb, string noun){
        var button = Instantiate(instance.buttonPrefab, 
        instance.achievementListContents[0]).GetComponent<Button>();
        button.GetComponentInChildren<Text>().text = name;
        button.GetComponentInChildren<Text>().color = Color.cyan;
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {ShowStatistic(index,name,verb,noun);}));
    }
    public static void AddAchievement(Achievement achievement)
    {
        loadedAchievements.Add(achievement);
        var button = Instantiate(instance.buttonPrefab, 
            instance.achievementListContents[1]).GetComponent<Button>();
        if(achievement.name.Length <= 33)
            button.GetComponentInChildren<Text>().text = achievement.name;
        else
            button.GetComponentInChildren<Text>().text = achievement.name.Substring(0,30) + "...";
        if (achievement.completion){
            button.GetComponentInChildren<Text>().color = Color.green;
        }
        else {
            button.GetComponentInChildren<Text>().color = achievement.textColor;
        }
        #if UNITY_EDITOR
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => 
        {
            ShowAchievement(achievement); 
            if(Input.GetKey(KeyCode.LeftShift)) achievement.completion = true;
        }));
        #else
        button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {ShowAchievement(achievement);}));
        #endif
    }

    public Text nameAndPrerequisitesHeader;

    public static void ShowAchievement(Achievement achievement)
    {
        instance.ClearAchievementObjectivesSpace();
        instance.nameAndPrerequisitesHeader.text = $"{achievement.name}\n\n\n{achievement.description}\n\n";
        instance.nameAndPrerequisitesHeader.text += $"\n{achievement.progress}% complete!";
        
        if(achievement.completion)
        {
            var obj = Instantiate(instance.objectivePrefab, instance.achievementCompletionContents, false);
            var strings = obj.GetComponentsInChildren<Text>();
            strings[0].text = "Achievement Obtained!";
            strings[1].text = "";
            strings[0].color = Color.green;
        }

        Canvas.ForceUpdateCanvases();
    }
    public static void ShowStatistic(int index, string name, string verb, string noun)
    {
        Statistic statsList = GameObject.Find("StatisticManager").GetComponent<Statistic>();
        instance.ClearAchievementObjectivesSpace();
        instance.nameAndPrerequisitesHeader.text = $"You have currently {verb} a total of {statsList.returnStats(index)} {noun}!";
        instance.nameAndPrerequisitesHeader.transform.parent.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
    }

    public void ClearAchievementObjectivesSpace()
    {
        for(int i = 1; i < achievementCompletionContents.childCount; i++)
        {
            Destroy(achievementCompletionContents.GetChild(i).gameObject);
        }
    }
}
