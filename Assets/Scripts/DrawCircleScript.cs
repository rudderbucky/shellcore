using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCircleScript : MonoBehaviour {
    private float alpha;
    private float speed;
    private LineRenderer line;
    private float timer;
    private void Start()
    {
        alpha = 1;
        speed = Random.Range(30, 40);
        line = gameObject.GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 60;
    }

    private void Update()
    {
        if (line)
        {
            timer += Time.deltaTime;
            if (timer > 1 && timer < 1.5F)
            {
                CreatePoints(speed * (timer - 1), speed * (timer - 1));
            }
            else if (timer >= 1.5F)
            {
                Destroy(line);
            }
            else {
                line.startColor = new Color(1, 1, 1, 0);
                line.endColor = new Color(1, 1, 1, 0);
            }
        }
    }

    void CreatePoints(float xradius, float yradius)
    {
        alpha -= 4*Time.deltaTime;
        line.startColor = new Color(1, 1, 1, alpha);
        line.endColor = new Color(1, 1, 1, alpha);
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (line.positionCount); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / line.positionCount+1);
        }
    }
}
