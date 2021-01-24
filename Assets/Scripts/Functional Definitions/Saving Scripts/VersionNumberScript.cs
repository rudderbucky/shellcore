using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumberScript : MonoBehaviour
{
    public static string version = "Alpha 5.2.0";
    public static string mapVersion = "Alpha 5.2.0";
    static VersionNumberScript instance;
    public Text episodeText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Refresh();
    }

    public static void Refresh()
    {
        if (instance)
        {
            if (instance.GetComponent<Text>())
            {
                instance.GetComponent<Text>().text = "Version " + version;
            }
        }
    }

    public static void SetEpisodeName(int episode)
    {
       if (instance && instance.episodeText)
        {
            string trc = "TACTICAL RETRO COMBAT.\n";
            switch(episode)
            {
                case 1:
                    instance.episodeText.text = $"{trc}<color=red>EPISODE 2: INFECTION</color>";
                    break;
                case 0:
                default:
                    instance.episodeText.text = $"{trc}<color=lime>EPISODE 1: NEWBORN</color>";
                    break;
                
            }
        } 
    }

}
