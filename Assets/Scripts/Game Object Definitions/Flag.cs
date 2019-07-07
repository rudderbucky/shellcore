using UnityEngine;

public class Flag : MonoBehaviour
{
    public string ID;

    private void OnEnable()
    {
        AIData.flags.Add(this);
    }

    private void OnDisable()
    {
        AIData.flags.Remove(this);
    }
}
