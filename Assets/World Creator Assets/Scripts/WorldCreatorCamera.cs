using UnityEngine;
using UnityEngine.EventSystems;

public class WorldCreatorCamera : MonoBehaviour
{
    public WorldCreatorCursor cursor;
    public EventSystem system;
    public CanvasGroup group;
    public int sectorIndex = 0;
    public static int minZ = -10;
    public static int maxZ = -150;
    public static WorldCreatorCamera instance;

    void FixedUpdate()
    {
        instance = this;
        //group.interactable = (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0);
        if (!system.IsPointerOverGameObject())
        {
            transform.position += Input.GetAxis("Horizontal") * new Vector3(1 + Mathf.Abs(transform.position.z + 10) / 4, 0, 0);
            transform.position += Input.GetAxis("Vertical") * new Vector3(0, 1 + Mathf.Abs(transform.position.z + 10) / 4, 0);


            if (Input.GetMouseButton(2))
            {
                if (Screen.width - Input.mousePosition.x < 3)
                {
                    transform.position += 2 * new Vector3(1 + Mathf.Abs(transform.position.z + 10) / 4, 0, 0);
                }

                if (Screen.width - Input.mousePosition.x > Screen.width - 3)
                {
                    transform.position -= 2 * new Vector3(1 + Mathf.Abs(transform.position.z + 10) / 4, 0, 0);
                }

                if (Screen.height - Input.mousePosition.y < 3)
                {
                    transform.position += 2 * new Vector3(0, 1 + Mathf.Abs(transform.position.z + 10) / 4, 0);
                }

                if (Screen.height - Input.mousePosition.y > Screen.height - 3)
                {
                    transform.position -= 2 * new Vector3(0, 1 + Mathf.Abs(transform.position.z + 10) / 4, 0);
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetAxis("Mouse ScrollWheel") < 0 && transform.position.z >= maxZ)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 5);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetAxis("Mouse ScrollWheel") > 0 && transform.position.z < minZ)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 5);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                var vec = cursor.GetSectorCenter();
                transform.position = new Vector3(vec.x, vec.y, transform.position.z);
            }

            if (cursor.sectors.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.PageDown))
                {
                    sectorIndex--;
                    if (sectorIndex < 0)
                    {
                        sectorIndex = cursor.sectors.Count - 1;
                    }

                    var vec = cursor.sectors[sectorIndex].renderer.bounds.center;
                    vec.z = transform.position.z;
                    transform.position = vec;
                }

                if (Input.GetKeyDown(KeyCode.PageUp))
                {
                    sectorIndex++;
                    if (sectorIndex >= cursor.sectors.Count)
                    {
                        sectorIndex = 0;
                    }

                    var vec = cursor.sectors[sectorIndex].renderer.bounds.center;
                    vec.z = transform.position.z;
                    transform.position = vec;
                }
            }

        }
    }
}
