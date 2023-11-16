using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FusionStationInventoryScript : ShipBuilderInventoryBase
{
    public Text partCreated;
    public void Restart()
    {
        Start();
        StopAllCoroutines();
        StartCoroutine(Grow());
    }

    IEnumerator Grow()
    {
        var rect = GetComponent<RectTransform>();
        if (partCreated) partCreated.enabled = true;
        rect.localScale = Vector3.zero;
        while (rect.localScale.x < 1)
        {
            rect.localScale = rect.localScale + Vector3.one / 10;
            yield return new WaitForEndOfFrame();
        }
        rect.localScale = Vector3.one;
        StartCoroutine(Shrink());
    }


    IEnumerator Shrink()
    {
        var rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        yield return new WaitForSeconds(3);
        if (partCreated) partCreated.enabled = false;
        while (rect.localScale.x > 0)
        {
            rect.localScale = rect.localScale - Vector3.one / 10;
            yield return new WaitForEndOfFrame();
        }
        rect.localScale = Vector3.zero;
    }
}
