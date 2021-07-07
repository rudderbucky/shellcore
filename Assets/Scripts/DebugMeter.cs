using UnityEngine;

public class DebugMeter : MonoBehaviour
{
    public enum Mode
    {
        Points,
        Count
    }

    public static void AddDataPoint(float data)
    {
        if (instance)
        {
            instance.addDataPoint(data);
        }
    }

    private void addDataPoint(float data)
    {
        rend.SetPosition(index, new Vector3(step * index, data * 0.01f, 0f));
        index = (index + 1) % pointCount;
    }

    public static void IncreaseCount()
    {
        if (instance)
        {
            instance.increaseCount();
        }
    }

    private void increaseCount()
    {
        count++;
    }

    static DebugMeter instance;

    //float[] data = new float[1024];
    int index = 0;
    LineRenderer rend;
    int pointCount = 1024;
    float step = 0.1f;
    int count = 0;
    public Mode mode;

    private void FixedUpdate()
    {
        if (mode == Mode.Count)
        {
            rend.SetPosition(index, new Vector3(step * index, count * 0.1f, 0f));
            index = (index + 1) % pointCount;
            count = 0;
        }
    }

    private void Awake()
    {
        instance = this;
        rend = gameObject.AddComponent<LineRenderer>();
        rend.positionCount = pointCount;
        //for(int i = 0; i < pointCount; i++)
        //{
        //    data[i] = 0f;
        //}
        rend.startColor = Color.green;
        rend.endColor = Color.green;
        rend.startWidth = 0.1f;
        rend.endWidth = 0.1f;
        rend.useWorldSpace = true;

        step = 5f / pointCount;
        for (int i = 0; i < pointCount; i++)
        {
            rend.SetPosition(i, new Vector3(step * i, 0f, 0f));
        }
    }
}
