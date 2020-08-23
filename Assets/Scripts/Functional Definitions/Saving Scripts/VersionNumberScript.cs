using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumberScript : MonoBehaviour
{
    public static string version = "Alpha 4.3.0";
    public static string mapVersion = "Alpha 4.3.0";

    static VersionNumberScript instance;

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

}
