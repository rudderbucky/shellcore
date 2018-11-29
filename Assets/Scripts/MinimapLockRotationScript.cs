using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapLockRotationScript : MonoBehaviour {

    SpriteRenderer sprRenderer;
    public void Initialize() {
        sprRenderer = GetComponent<SpriteRenderer>();
        sprRenderer.enabled = true;
        Transform tmp = transform.parent.Find("Shell Sprite");
        sprRenderer.color = tmp.GetComponent<SpriteRenderer>().color;
    }

    private void Start()
    {
        //Transform tmp = transform.parent.Find("Shell Sprite");
        //sprRenderer.color = tmp.GetComponent<SpriteRenderer>().color;
    }
    // Update is called once per frame
    void Update () {

        transform.rotation = Quaternion.identity;
	}
}
