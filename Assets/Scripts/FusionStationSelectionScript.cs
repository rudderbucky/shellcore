using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FusionStationSelectionScript : ShipBuilderInventoryBase
{
    public Text partCreated;
    public bool finalPartMode;
    public void Restart()
    {
        Start();
        StopAllCoroutines();
        StartCoroutine(Grow());
    }
        
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftShift) && !string.IsNullOrEmpty(part.partID) && !finalPartMode)
        {
            part.partID = null;
            StopAllCoroutines();
            StartCoroutine(Shrink());
        }
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
        if (finalPartMode) StartCoroutine(Shrink());
    }


    IEnumerator Shrink()
    {
        var rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        if (finalPartMode) yield return new WaitForSeconds(3);
        if (partCreated) partCreated.enabled = false;
        while (rect.localScale.x > 0)
        {
            rect.localScale = rect.localScale - Vector3.one / 10;
            yield return new WaitForEndOfFrame();
        }
        rect.localScale = Vector3.zero;
    }
}
