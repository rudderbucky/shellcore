using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldCreatorCursor : MonoBehaviour
{
    public ItemHandler handler;
    Item current;
    GameObject cursorObj;
    // Start is called before the first frame update
	private float tileSize = 5F;
	Vector2 cursorOffset = new Vector2(2.5F, 2.5F);

    public EventSystem system;

    // Update is called once per frame
    void Update() {
		Vector3 mousePos = Input.mousePosition;
		mousePos.z -= Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos);
		if(current.type == ItemType.Platform) {
			mousePos.x = cursorOffset.x + tileSize * (int)((mousePos.x - cursorOffset.x) / tileSize + (mousePos.x / 2> 0 ? 0.5F : -0.5F));
			mousePos.y = cursorOffset.y + tileSize * (int)((mousePos.y - cursorOffset.y) / tileSize + (mousePos.y / 2> 0 ? 0.5F : -0.5F));
		} else {
			mousePos.x = 0.5F * tileSize * Mathf.RoundToInt((mousePos.x) / (0.5F * tileSize));
			mousePos.y = 0.5F * tileSize * Mathf.RoundToInt((mousePos.y) / (0.5F * tileSize));
		}
        if(cursorObj) cursorObj.transform.position = mousePos;
        if(Input.GetMouseButtonUp(0) && !system.IsPointerOverGameObject()) Instantiate(cursorObj);
    }

    public void SetCurrent(Item item) {
        current = item;
        if(cursorObj) Destroy(cursorObj);
        cursorObj = Instantiate(current.obj);
    }
}
