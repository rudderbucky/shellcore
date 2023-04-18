using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SubcanvasSortingOrder : MonoBehaviour
{
    public void Initialize()
    {
        GetComponent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer;
    }
}
