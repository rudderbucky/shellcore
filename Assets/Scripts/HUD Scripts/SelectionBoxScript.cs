using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
/// The box for selecting multiple entities from the overworld
///</summary>
public class SelectionBoxScript : MonoBehaviour
{

    public Image image;
    private Vector2 startPoint;
    private Vector2 pivotVec;
    private Vector2 sizeVec;
    public ReticleScript reticleScript;
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            // Get reference point of selection box for drawing
            startPoint = Input.mousePosition;
            image.rectTransform.anchoredPosition = startPoint;
        }

        if(Input.GetMouseButton(0))
        {
            // Draw box
            image.enabled = true;
            sizeVec = (Vector2)Input.mousePosition - startPoint;
            sizeVec.x = Mathf.Abs(sizeVec.x);
            sizeVec.y = Mathf.Abs(sizeVec.y);
            image.rectTransform.sizeDelta = sizeVec;
            
            // Change the pivot of the size delta when the mouse goes under/before the start point
            pivotVec = new Vector2();
            pivotVec.x = Input.mousePosition.x < startPoint.x ? 1 : 0;
            pivotVec.y = Input.mousePosition.y < startPoint.y ? 1 : 0;
            image.rectTransform.pivot = pivotVec;
        } 
        else if(Input.GetMouseButtonUp(0))
        {
            // Grab the rect of the selection box
            Vector2 boxStart = 
                Camera.main.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, 10));
            Vector2 boxExtents = 
                Camera.main.ScreenToWorldPoint(
                    new Vector3((1 - 2 * pivotVec.x) * sizeVec.x + startPoint.x, 
                    (1 - 2 * pivotVec.y) * sizeVec.y + startPoint.y, 
                    10));
            Rect finalBox = Rect.MinMaxRect(
                Mathf.Min(boxStart.x, boxExtents.x),
                Mathf.Min(boxStart.y, boxExtents.y),
                Mathf.Max(boxStart.x, boxExtents.x),
                Mathf.Max(boxStart.y, boxExtents.y)
            );

            // Now scan for entities
            foreach(var ent in AIData.entities)
            {
                if(finalBox.Contains(ent.transform.position)) 
                    reticleScript.AddSecondaryTarget(ent);
            }
            image.enabled = false;
        }
    }
}
