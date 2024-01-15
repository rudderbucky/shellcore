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

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > time1)
            cont1.gameObject.SetActive(true);

        if (timer > time2)
            cont2.gameObject.SetActive(true);

        if (timer > time3)
            cont3.gameObject.SetActive(true);
        if (timer > 10.5F)
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
