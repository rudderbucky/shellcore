using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SubcanvasSortingOrder : MonoBehaviour
{
    public int offset = 1;

    private void OnEnable()
    {
        GetComponent<Canvas>().sortingOrder = PlayerViewScript.currentLayer + offset;
    }
}
