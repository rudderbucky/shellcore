using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScalerScript
{
    private static float scale = 1f;

    public static float GetScale()
    {
        return 1920f / Screen.width / scale;
    }

    public static void SetScale(float scale)
    {
        UIScalerScript.scale = scale;
        foreach (var canvasScaler in Resources.FindObjectsOfTypeAll<CanvasScaler>())
        {
            canvasScaler.referenceResolution = new Vector2(1920 / scale, 1080 / scale);
        }
    }
}
