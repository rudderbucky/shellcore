using UnityEngine;
using UnityEngine.SceneManagement;

public class ParticleFlag : MonoBehaviour
{
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
