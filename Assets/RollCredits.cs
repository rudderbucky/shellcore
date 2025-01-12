using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    void Awake()
    {
        instance = this;
    }

    public bool init;
    private float ext = 1;
    public void Init()
    {
        GetComponent<Image>().enabled = true;
        container.gameObject.SetActive(true);
        DevConsoleScript.Instance.SetInactive();
        AudioManager.PlayMusic("music_whimper", false);
#if UNITY_EDITOR
        Time.timeScale = ext;
        var x = 0f;
        AudioManager.instance.music.audioMixer.GetFloat("Pitch2", out x);
        Debug.LogWarning(x);
        AudioManager.instance.music.audioMixer.SetFloat("Pitch2", ext);
#endif
        StartCoroutine(Wait());
        init = true;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(60 * 4 + 4);
#if UNITY_EDITOR
        Time.timeScale = 1;
        AudioManager.instance.music.audioMixer.SetFloat("Pitch2", 1);
#endif
        yield return new WaitForSeconds(12);
        AudioManager.PlayMusic("music_overworld_old");
    }

    IEnumerator Wait2()
    {
        yield return new WaitForSeconds(30);
        SaveHandler.instance.Save();
        SceneManager.LoadScene("MainMenu");
    }

    string s1 = $"ShellCore\nCommand";
    string s2 = "REMASTERED";
    string s3 = "TACTICAL RETRO COMBAT.";
    [SerializeField]
    float rollStart;
    [SerializeField]
    float finalRoll = 10.3F;

// 10.3
    void Update()
    {
        if (init)
            timer += Time.deltaTime;
        cont1.gameObject.SetActive(true);
        cont2.gameObject.SetActive(true);
        cont3.gameObject.SetActive(true);
        // (m * rs + c = 1, m * time2 + c = 0 )
        var v21 = Mathf.Max(0, timer / (rollStart - time2) - (time2 / (rollStart - time2)));
        var v2 = Mathf.Min(s2.Length, Mathf.FloorToInt(v21 * s2.Length));

        var v31 = Mathf.Max(0, timer / (rollStart - time1) - (time1 / (rollStart - time1)));
        var v3 = Mathf.Min(s3.Length, Mathf.FloorToInt(v31 * s3.Length));
        cont1.GetComponentInChildren<Text>().text = s1.Substring(0, Mathf.Min(s1.Length, Mathf.FloorToInt((timer / rollStart) * s1.Length)));
        cont2.GetComponentInChildren<Text>().text = s2.Substring(0, v2);
        cont3.GetComponentInChildren<Text>().text = s3.Substring(0, v3);

        if (timer > time1)
        {
        }

        if (timer > time2)
        {

        }

        if (timer > time3)
        {
        }
        if (timer > finalRoll)
        {
            var cst = 25;
            if (Input.GetKey(KeyCode.Space)) cst *= 20;
            container.transform.position = container.transform.position + Vector3.up * Time.deltaTime * cst;
        }

        if (ty.parent != transform && ty.transform.position.y > 0.5 * Screen.height)
        {
            ty.SetParent(transform, false);
            ty.anchorMax = Vector2.one * 0.5F;
            ty.anchorMin = Vector2.one * 0.5F;
            ty.anchoredPosition = Vector2.zero;
            StartCoroutine(Wait2());
        }
    }
}
