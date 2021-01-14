using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldCreatorCamera : MonoBehaviour
{
    public WorldCreatorCursor cursor;
    public EventSystem system;
    public CanvasGroup group;
    void FixedUpdate() 
    {
        //group.interactable = (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0);
        if(!system.IsPointerOverGameObject())
        {
            transform.position += Input.GetAxis("Horizontal") * new Vector3(1 + Mathf.Abs(transform.position.z + 10) / 4, 0, 0);
            if(Screen.width - Input.mousePosition.x < 3)
                transform.position += new Vector3(1 + Mathf.Abs(transform.position.z + 10) / 4, 0, 0);
            if(Screen.width - Input.mousePosition.x > Screen.width - 3)
                transform.position -= new Vector3(1 + Mathf.Abs(transform.position.z + 10) / 4, 0, 0);
            transform.position += Input.GetAxis("Vertical") * new Vector3(0, 1 + Mathf.Abs(transform.position.z + 10) / 4, 0);
            if(Screen.height - Input.mousePosition.y < 3)
                transform.position += new Vector3(0, 1 + Mathf.Abs(transform.position.z + 10) / 4, 0);
            if(Screen.height - Input.mousePosition.y > Screen.height - 3)
                transform.position -= new Vector3(0, 1 + Mathf.Abs(transform.position.z + 10) / 4, 0);

            if(Input.GetKey(KeyCode.LeftControl) && Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.z >= -150) 
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 5);
            }
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetAxis("Mouse ScrollWheel") > 0 && transform.position.z < -10) 
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 5);
            }

            if(Input.GetKeyUp(KeyCode.Space))
            {
                var vec = cursor.GetSectorCenter();
                transform.position = new Vector3(vec.x, vec.y, transform.position.z);
            }
        }        
    }
}
