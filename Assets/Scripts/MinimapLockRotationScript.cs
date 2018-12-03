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
        gameObject.layer = 8;
        sprRenderer = GetComponent<SpriteRenderer>();
        sprRenderer.enabled = true; // enable sprite renderer
        Color factionColor = FactionColors.colors[GetComponentInParent<Entity>().faction];
        sprRenderer.color = factionColor; // adjust minimap color to that
    }

    private void Start()
    {
    }
    // Update is called once per frame
    void Update () {

        transform.rotation = Quaternion.identity; // reset rotation
	}
}
