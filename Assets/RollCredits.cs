using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    void Start()
    {
        AudioManager.PlayMusic("music_whimper", false);
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(60 * 4 + 12);
        AudioManager.PlayMusic("music_overworld_old");
    }


    void Update()
    {
        timer += Time.deltaTime;
        Time.timeScale = 8;
        if (timer > time1)
            cont1.gameObject.SetActive(true);

        if (timer > time2)
            cont2.gameObject.SetActive(true);

        if (timer > time3)
            cont3.gameObject.SetActive(true);
        if (timer > 10.5F)
        {
            var cst = 35;
            if (Input.GetKey(KeyCode.Space)) cst *= 2;
            container.transform.position = container.transform.position + Vector3.up * Time.deltaTime * cst;
        }

        if (container.anchoredPosition.y > 8000)
        {
            ty.parent = transform;
        }
    }
}
