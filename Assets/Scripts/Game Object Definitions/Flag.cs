using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class Flag : MonoBehaviour
{
    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            AIData.flags.Add(this);
        }
    }

    private void OnDisable()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            AIData.flags.Remove(this);
        }
    }

    private void Start()
    {
        if(SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
