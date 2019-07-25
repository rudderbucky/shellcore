using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShardCountScript : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform imageTransform;
    public Text number;
    public PlayerCore core;
    public static ShardCountScript instance;
    private bool slidingIn;
    private bool slidingOut;
    void Start() {
        instance = this;
    }
    void FixedUpdate() {
        imageTransform.rotation = Quaternion.Euler(0, 0, Time.fixedTime * 100);
        if(slidingIn) {
            instance.StartCoroutine("SlideIn");
        } else if(slidingOut) {
            instance.StartCoroutine("SlideOut");
        }
    }

    public static void DisplayCount(int count) {
        instance.number.text = count + "";
        instance.slidingIn = true;
    }

    IEnumerator SlideIn() {
        while(rectTransform.anchoredPosition.x < -13.5F) {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition + new Vector2(0.5F, 0);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        slidingIn = false;
        slidingOut = true;
    }
    IEnumerator SlideOut() {
        while(rectTransform.anchoredPosition.x > -71F) {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition - new Vector2(0.5F, 0);
            yield return null;
        }
        slidingOut = false;
    }
}
