using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellPart : MonoBehaviour {

    float detachedTime;
    private bool hasDetached;
    // Use this for initialization
    public void Detach() {
        detachedTime = Time.time;
        hasDetached = true;
        gameObject.AddComponent<Rigidbody2D>();
        GetComponent<Rigidbody2D>().gravityScale = 0;
        GetComponent<Rigidbody2D>().drag = 0;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(250 * Random.Range(-1,2), 250 * Random.Range(-1, 2)));
        GetComponent<Rigidbody2D>().AddTorque(100 * Random.Range(-20, 21));
    }
	public void Start () {
        hasDetached = false;
        GetComponent<SpriteRenderer>().enabled = true;
        Destroy(GetComponent<Rigidbody2D>());
        transform.position = transform.parent.position;
        transform.rotation = Quaternion.identity;
	}

    void Blink() {
        //Debug.Log(Time.time % 2 > 1);
        GetComponent<SpriteRenderer>().enabled = Time.time % 0.25F > 0.125F;
    }

	// Update is called once per frame
	void Update () {
        if (hasDetached && Time.time - detachedTime < 1)
        {
            Blink();
        }
        else if (hasDetached && Time.time - detachedTime > 1) {
            GetComponent<SpriteRenderer>().enabled = false;
        }
	}
}
