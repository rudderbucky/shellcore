using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The part script for the shell around a core of a Shellcore (will be salvaged to make a more general part class)
/// </summary>
public class ShellPart : MonoBehaviour {

    float detachedTime; // time since detachment
    private bool hasDetached; // is the part detached
    // Use this for initialization

    /// <summary>
    /// Detach the part from the Shellcore
    /// </summary>
    public void Detach() {
        detachedTime = Time.time; // update detached time
        hasDetached = true; // has detached now
        gameObject.AddComponent<Rigidbody2D>(); // add a rigidbody (this might become permanent)
        GetComponent<Rigidbody2D>().gravityScale = 0; // adjust the rigid body
        GetComponent<Rigidbody2D>().drag = 0;

        // add force and torque
        GetComponent<Rigidbody2D>().AddForce(new Vector2(250 * Random.Range(-1,2), 250 * Random.Range(-1, 2)));
        GetComponent<Rigidbody2D>().AddTorque(100 * Random.Range(-20, 21));
    }
	public void Start () {
        // initialize instance fields
        hasDetached = false;
        GetComponent<SpriteRenderer>().enabled = true;
        Destroy(GetComponent<Rigidbody2D>()); // remove rigidbody
        transform.position = transform.parent.position;
        transform.rotation = Quaternion.identity;
	}

    /// <summary>
    /// Makes the part blink like in the original game
    /// </summary>
    void Blink() {
        GetComponent<SpriteRenderer>().enabled = Time.time % 0.25F > 0.125F; // math stuff that blinks the part
    }

	// Update is called once per frame
	void Update () {
        if (hasDetached && Time.time - detachedTime < 1) // checks if the part has been detached for more than a second (hardcoded)
        {
            Blink(); // blink
        }
        else if (hasDetached) { // if it has actually detached
            GetComponent<SpriteRenderer>().enabled = false; // disable sprite renderer
        }
	}
}
