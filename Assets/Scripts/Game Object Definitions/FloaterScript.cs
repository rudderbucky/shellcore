using UnityEngine;
using UnityEngine.SceneManagement;

public class FloaterScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (SceneManager.GetActiveScene().name == "SampleScene" || SceneManager.GetActiveScene().name == "MainMenu")
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 5);
            spriteRenderer.color = SectorManager.instance.current.backgroundColor + Color.grey;
        }
    }

    void Update()
    {
        if (!SectorManager.instance)
        {
            return;
        }

        if (SectorManager.instance.overrideProperties != null)
        {
            spriteRenderer.color = SectorManager.instance.overrideProperties.backgroundColor + Color.grey;
        }
        else
        {
            spriteRenderer.color = SectorManager.instance.current.backgroundColor + Color.grey;
        }
    }
}
