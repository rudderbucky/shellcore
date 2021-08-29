using UnityEngine;

public class PartyPortraitHandler : MonoBehaviour
{
    void OnEnable()
    {
        PartyManager.instance.UpdatePortraits();
    }
}
