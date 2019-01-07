using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script used to draw the circle on a craft death
/// </summary>
public class DrawCircleScript : MonoBehaviour {
    private float alpha; // used to store the circle's transparency value
    private float speed; // the speed at which the circle expands
    private LineRenderer line; // line renderer used to generate the circle
    private float timer; // timer used to expand the circle
    private bool initialized;
    public bool respawnMode;
    private float xrad = 3F;
    private float yrad = 3F;

    public void Initialize()
    {
        initialized = true;
    }

    public void SetRespawnMode(bool mode) {
        respawnMode = mode;
        if(mode) {
            speed = 20F;
            alpha = 1;
            initialized = true;
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 21;
            line.startColor = line.endColor = Color.white;
            line.startWidth = line.endWidth = 0.25F;
            CreatePoints(xrad, yrad);
        }
    }

    private void Start()
    {
        if(respawnMode) {
        } else {
        // initialize instance fields
            alpha = 1;
            speed = Random.Range(30, 40);
            line = gameObject.GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 60;
        }
    }

    private void Update()
    {
        if (initialized)
        {
            if(respawnMode) {
                line.enabled = true;
                xrad -= Time.deltaTime * speed;
                yrad -= Time.deltaTime * speed;
                if(xrad < 0) {
                    Destroy(gameObject);
                } else
                CreatePoints(xrad, yrad);
            } else {
                timer += Time.deltaTime; // increment time
                if (timer > 0.5F && timer < 1.25F) // time to draw
                {
                    CreatePoints(speed * (timer - 0.5F), speed * (timer - 0.5F)); // draw the circle
                }
                else
                {
                    line.startColor = new Color(1, 1, 1, 0); // set to transparent
                    line.endColor = new Color(1, 1, 1, 0); // set to transparent
                }
            }
        }
    }

    /// <summary>
    /// Script used to draw the circle (can be salvaged to create elipses)
    /// </summary>
    /// <param name="xradius">current x radius of the circle</param>
    /// <param name="yradius">current y radius of the circle</param>
    void CreatePoints(float xradius, float yradius)
    {
        alpha -= 4*Time.deltaTime; // decrease alpha
        line.startColor = new Color(1, 1, 1, alpha); // change transparency
        line.endColor = new Color(1, 1, 1, alpha);
        // declare some storage fields
        float x;
        float y;
        float z = 0f;

        float angle = 20f; // this would be used to angle elipses

        for (int i = 0; i < (line.positionCount); i++) // iterate through all vertices
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius; // update position
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += 360f / line.positionCount+1; // update angle for next vertex
        }
    }
}
