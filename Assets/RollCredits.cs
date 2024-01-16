using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// TODO: fix escape menu during credits
public class RollCredits : MonoBehaviour
{
    [SerializeField]
    RectTransform container;
    float timer = 0;
    [SerializeField]

    RectTransform cont1;
    [SerializeField]
    RectTransform cont2;


    [SerializeField]
    RectTransform cont3;

    [SerializeField]
    float time1;

    [SerializeField]
    float time2;

    [SerializeField]
    float time3;

    [SerializeField]
    RectTransform ty;

    public static RollCredits instance;

    void Start()
    {
        instance = this;
    }

    public void Init()
    {
        GetComponent<Image>().enabled = true;
        container.gameObject.SetActive(true);
        DevConsoleScript.Instance.SetInactive();
        AudioManager.PlayMusic("music_whimper", false);
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(60 * 4 + 17);
        AudioManager.PlayMusic("music_overworld_old");
    }

    IEnumerator Wait2()
    {
        yield return new WaitForSeconds(30);
        SaveHandler.instance.Save();
        SceneManager.LoadScene("MainMenu");
    }

    string s1 = "ShellCoreCommand";
    string s2 = "REMASTERED";
    string s3 = "TACTICAL RETRO COMBAT.";
    float rollStart = 10.5F;


    void Update()
    {
        timer += Time.deltaTime;
        cont1.gameObject.SetActive(true);
        cont2.gameObject.SetActive(true);
        cont3.gameObject.SetActive(true);
        cont1.GetComponentInChildren<Text>().text = s1.Substring(0, Mathf.Min(s1.Length, Mathf.FloorToInt((timer / rollStart) * s1.Length)));
        cont2.GetComponentInChildren<Text>().text = s2.Substring(0, Mathf.Min(s2.Length, Mathf.FloorToInt((timer / rollStart) * s2.Length)));
        cont3.GetComponentInChildren<Text>().text = s3.Substring(0, Mathf.Min(s3.Length, Mathf.FloorToInt((timer / rollStart) * s3.Length)));

        if (timer > time1)
        {
        }

        if (timer > time2)
        {

        }

        if (timer > time3)
        {
        }
        if (timer > rollStart)
        {
            var cst = 35;
            if (Input.GetKey(KeyCode.Space)) cst *= 20;
            container.transform.position = container.transform.position + Vector3.up * Time.deltaTime * cst;
        }

        if (container.anchoredPosition.y > 6670)
        {
            ty.SetParent(transform, false);
            ty.anchorMax = Vector2.one * 0.5F;
            ty.anchorMin = Vector2.one * 0.5F;
            ty.anchoredPosition = Vector2.zero;
            StartCoroutine(Wait2());
        }
    }
}
