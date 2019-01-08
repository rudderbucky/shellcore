using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphereScript : MonoBehaviour {

    // TODO: make undraggable if already being dragged

    float timer;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 10) {
            Destroy(gameObject);
        }
        else if (timer > 7)
        {
            Blink();
        }
    }

    /// <summary>
    /// Makes the energy blink like in the original game
    /// </summary>
    void Blink()
    {
        GetComponent<SpriteRenderer>().enabled = Time.time % 0.25F > 0.125F; // math stuff that blinks the part
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<IHarvester>() != null)
        {
            ResourceManager.PlayClipByID("clip_powerup", transform.position);
            collision.GetComponent<IHarvester>().AddPower(20);
            Destroy(gameObject);
        }
    }
}
