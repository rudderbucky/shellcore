using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    public List<TriggerTraverser> traversers;
    public static TriggerManager instance;

    void Awake()
    {
        instance = this;
        traversers = new List<TriggerTraverser>();
    }
}
