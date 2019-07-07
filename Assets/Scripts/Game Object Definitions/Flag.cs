using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Flag : MonoBehaviour
{
    public string ID;

    private void OnEnable()
    {
        AIData.flags.Add(this);
        Debug.Log("Added flag!");
    }

    private void OnDisable()
    {
        AIData.flags.Remove(this);
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name != "SectorCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
