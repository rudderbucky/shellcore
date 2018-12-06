using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script used to draw a line on a craft's death
/// </summary>
public class DrawLineScript : MonoBehaviour {
    LineRenderer line; // line renderer
    private float timer; // used for transparency and projection
    private float startAngle; // angle to project the line
    private float speed; // speed at which the line is projected
    private bool initialized;

    void Start()
    {
        // initialize instance fields
        speed = Random.Range(10, 20);
        startAngle = Random.Range(0, 2 * Mathf.PI);
        line = gameObject.GetComponent<LineRenderer>();
        line.startWidth = 0;
        line.positionCount = 2;
        line.useWorldSpace = false;
    }

    public void Initialize()
    {
        initialized = true;
    }
 
    private void Update()
    {
        if (initialized)
        {
            timer += Time.deltaTime; // update timer

            if (line) // if line renderer isn't destroyed
            {
                if (timer < 1) // time to project
                {
                    DrawLine(speed * timer, 0, startAngle); // project out the line
                }
                else // time to shorten
                {
                    ShortenLine(speed * (timer - 1), 0, startAngle);
                }
            }
        }  
    }

    /// <summary>
    /// Shortens the line by moving forward the back vertex of the line
    /// </summary>
    /// <param name="length">the length to shorten by</param>
    /// <param name="index">the index of the vertex to shorten with</param>
    /// <param name="angle">the angle at which to shorten</param>
    void ShortenLine(float length, int index, float angle) {

        line.startWidth += 2 * Time.deltaTime; 
        // widen the start of the line to help create a 3D effect

        Vector2 pos = new Vector2(length * Mathf.Cos(angle), -length * Mathf.Sin(angle));
        // find the new position to place the back vertex

        if (Vector2.SqrMagnitude(pos) >= Vector2.SqrMagnitude(line.GetPosition(index + 1)))
        {
            Destroy(line);
            // if it overshoots destroy the line renderer
        }
        else
        {
            line.SetPosition(index, pos);
            // extend the back vertex
        }
    }

    /// <summary>
    /// Extends the line by moving forward the front vertex of the line
    /// </summary>
    /// <param name="length">the length to lengthen by</param>
    /// <param name="index">the index of the vertex to lengthen with</param>
    /// <param name="angle">the angle at which to lengthen</param>
    void DrawLine(float length, int index, float angle) {

        line.endWidth += 2 * Time.deltaTime;
        // widen the end of the line to help create a 3D effect

        line.startColor = new Color(1, 1, 1, 1);
        line.endColor = new Color(1, 1, 1, 1 - 4 * timer);
        // make the front less opaque to create a god-ray like effect

        line.SetPosition(index, Vector3.zero);
        // set the back vertex position to zero

        Vector2 pos = new Vector2(length * Mathf.Cos(angle), -length * Mathf.Sin(angle));
        // find the new position to place the front vertex

        line.SetPosition(index+1, pos);
        // extend the front vertex
    }
}
