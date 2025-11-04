using UnityEngine;
using UnityEngine.UI;

public class VersionNumberScript : MonoBehaviour
{
    public static string version = "0.1.0";
    public static string mapVersion = "0.1.0";
    public static string rdbMap = "rudderbucky server - 1";
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
            switch (episode)
            {
                case 3:
                    instance.episodeText.text = $"{trc}<color=#5C89E6>THE FINAL EPISODE</color>";
                    break;
                case 2:
                    instance.episodeText.text = $"{trc}<color=#daa620>EPISODE 3: ABANDONED</color>";
                    break;
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
