using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Minimap scripts used to keep the rotation of the minimap square to a fixed point
/// </summary>
public class MinimapLockRotationScript : MonoBehaviour {

    SpriteRenderer sprRenderer; // associated minimap square sprite

    /// <summary>
    /// Used to initialize the minimap sprite representation
    /// </summary>
    public void Initialize() {
        sprRenderer = GetComponent<SpriteRenderer>();
        sprRenderer.enabled = true; // enable sprite renderer
        Transform tmp = transform.parent.Find("Shell Sprite"); // get the shell color
        sprRenderer.color = tmp.GetComponent<SpriteRenderer>().color; // adjust minimap color to that
    }

    private void Start()
    {
    }
    // Update is called once per frame
    void Update () {

        transform.rotation = Quaternion.identity; // reset rotation
	}
}
