using UnityEngine;
using UnityEngine.SceneManagement;

public class ParticleFlag : MonoBehaviour
{
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "SectorCreator" && SceneManager.GetActiveScene().name != "WorldCreator")
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<ParticleSystem>().enableEmission = true;
        }
		else
		{
            GetComponent<ParticleSystem>().enableEmission = false;
		}
    }
}
