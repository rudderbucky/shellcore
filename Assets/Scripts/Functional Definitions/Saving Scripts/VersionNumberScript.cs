using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionNumberScript : MonoBehaviour
{
    public static string version = "Alpha 3.1.0";
    // Start is called before the first frame update
    void Start()
    {
        if(GetComponent<Text>()) GetComponent<Text>().text = "Version " + version;
    }

}
