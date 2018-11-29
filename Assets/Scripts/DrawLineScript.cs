using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawLineScript : MonoBehaviour {
    LineRenderer line;
    private float timer;
    private float startAngle;
    private float speed;

    void Start()
    {
        speed = Random.Range(10, 20);
        startAngle = Random.Range(0, 2 * Mathf.PI);
        line = gameObject.GetComponent<LineRenderer>();
        line.startWidth = 0;
        line.positionCount = 2;
        line.useWorldSpace = false;
    }
 
    private void Update()
    {
        timer += Time.deltaTime;
        
        if (line)
        {
            if (timer < 1)
            {
                DrawLine(speed * timer, 0, startAngle);
            }
            else
            {
                Vector2 dist = line.GetPosition(0) - line.GetPosition(1);
                if ((line.GetPosition(1).x >= 0 && dist.x <= 0) || (line.GetPosition(1).x <= 0 && dist.x >= 0) &&
                    (line.GetPosition(1).y >= 0 && dist.y <= 0) || (line.GetPosition(1).y <= 0 && dist.y >= 0)
                    )
                {
                    ShortenLine(speed * (timer - 1), 0, startAngle);
                }
                else Destroy(line);
            }
        }
         
    }

    void ShortenLine(float length, int index, float angle) {
        line.startWidth += 2 * Time.deltaTime;
        Vector2 pos = new Vector2(length * Mathf.Cos(angle), -length * Mathf.Sin(angle));
        line.SetPosition(index, pos);
    }

    void DrawLine(float length, int index, float angle) {
        line.endWidth += 2 * Time.deltaTime;
        line.startColor = new Color(1, 1, 1, 1);
        line.endColor = new Color(1, 1, 1, 1 - 4 * timer);
        line.SetPosition(index, Vector3.zero);
        Vector2 pos = new Vector2(length * Mathf.Cos(angle), -length * Mathf.Sin(angle));
        line.SetPosition(index+1, pos);
    }

    /*void CreatePoints(float xradius, float yradius)
    {
        //alpha -= Time.deltaTime;
        //line.startColor = new Color(1, 1, 1, alpha);
        //line.endColor = new Color(1, 1, 1, alpha);
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }*/
}
