using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySphereScript : MonoBehaviour {

    // TODO: make undraggable if already being dragged
    private bool collected = false;
    private void OnEnable()
    {
        AIData.energySpheres.Add(this);
    }

    private void OnDestroy()
    {
        AIData.energySpheres.Remove(this);
    }

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
        if (((collision.gameObject.name == "Shell Sprite" && collision.GetComponentInParent<IHarvester>() != null)
            || collision.transform.root.GetComponentInChildren<Harvester>() != null) && !collected)
        {
            AudioManager.PlayClipByID("clip_powerup", transform.position);
            var harvester = collision.GetComponentInParent<IHarvester>();
            if(harvester == null) harvester = collision.transform.root.GetComponentInChildren<IHarvester>();
            collected = true;
            harvester.AddPower(20);
            harvester.PowerHeal();
            Destroy(gameObject);
        }
    }
	
}
