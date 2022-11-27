using UnityEngine;

public class MissileAnimationScript : MonoBehaviour
{
    LineRenderer line; // line renderer
    private float timer; // used for transparency and projection
    private float startAngle; // angle to project the line
    private float speed; // speed at which the line is projected
    private bool initialized;
    private int iteration;
    private Vector3 initialPos;
    public Color lineColor;

    void Start()
    {
        // initialize instance fields
        speed = Random.Range(20, 25);
        startAngle = Random.Range(0, 2 * Mathf.PI);
        line = gameObject.GetComponent<LineRenderer>();
        line.startWidth = 0;
        line.positionCount = 2;
        line.useWorldSpace = false;
        line.startColor = lineColor;
        line.endColor = lineColor;
        iteration = 0;
        timer = 0;
        initialPos = Vector3.zero;
        line.startWidth = line.endWidth = 0.1F;
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
            switch (iteration)
            {
                case 3:
                    Start();
                    break;
                default:
                    if (timer < 0.020F) // time to project
                    {
                        DrawLine(speed * timer, iteration, startAngle, initialPos); // project out the line
                    }
                    else // time to shorten
                    {
                        timer = 0;
                        iteration += 1;
                        line.positionCount += 1;
                        initialPos = line.GetPosition(iteration);
                        startAngle = Random.Range(0, 2 * Mathf.PI);
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Extends the line by moving forward the front vertex of the line
    /// </summary>
    /// <param name="length">the length to lengthen by</param>
    /// <param name="index">the index of the vertex to lengthen with</param>
    /// <param name="angle">the angle at which to lengthen</param>
    void DrawLine(float length, int index, float angle, Vector3 initialPosition)
    {
        line.SetPosition(index, initialPosition);
        // set the back vertex position to zero

        Vector2 pos = initialPosition + new Vector3(length * Mathf.Cos(angle), -length * Mathf.Sin(angle));
        // find the new position to place the front vertex

        line.SetPosition(index + 1, pos);
        // extend the front vertex
    }
}
