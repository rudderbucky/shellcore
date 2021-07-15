using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShardCountScript : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform imageTransform;
    public Text number;
    public PlayerCore core;
    public static ShardCountScript instance;
    private bool stickySlide;

    void Start()
    {
        instance = this;
        instance.origX = instance.GetComponent<RectTransform>().anchoredPosition.x;
        instance.sizeDeltaX = instance.GetComponent<RectTransform>().sizeDelta.x;
    }

    void FixedUpdate()
    {
        imageTransform.rotation = Quaternion.Euler(0, 0, Time.fixedTime * 100);
    }

    public static void DisplayCount(int count)
    {
        UpdateNumber(count);
        instance.StopAllCoroutines();
        instance.StartCoroutine("SlideIn");
    }

    public static void UpdateNumber(int count)
    {
        instance.number.text = count + "";
    }

    float origX;
    float sizeDeltaX;

    /// sticky slides used when you want the player to see their shard count
    public static void StickySlideIn(int count)
    {
//        instance.rectTransform.anchoredPosition += new Vector2(-instance.rectTransform.anchoredPosition.x -71F, 0);
        DisplayCount(count);
        instance.stickySlide = true;
        instance.StopAllCoroutines();
        instance.StartCoroutine("SlideIn");
    }

    public static void StickySlideOut()
    {
        instance.stickySlide = false;
        instance.StopAllCoroutines();
        instance.StartCoroutine("SlideOut");
    }

    IEnumerator SlideIn()
    {
        while (rectTransform.anchoredPosition.x < origX + sizeDeltaX)
        {
            var minint = Mathf.Min(3F, origX + sizeDeltaX - rectTransform.anchoredPosition.x);
            rectTransform.anchoredPosition = rectTransform.anchoredPosition + new Vector2(minint, 0);
            yield return null;
        }

        yield return new WaitForSeconds(3);
        if (!stickySlide)
        {
            instance.StartCoroutine("SlideOut");
        }
    }

    IEnumerator SlideOut()
    {
        while (rectTransform.anchoredPosition.x > origX)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition - new Vector2(3F, 0);
            yield return null;
        }
    }
}
