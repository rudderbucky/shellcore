using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Flag : MonoBehaviour
{
    public string ID;

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator")
        {
            AIData.flags.Add(this);
        }
    }

    private void OnDisable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator")
        {
            AIData.flags.Remove(this);
        }
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name != "SectorCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
