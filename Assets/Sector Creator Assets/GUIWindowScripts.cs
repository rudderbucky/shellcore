using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;
public interface IWindow {
	void CloseUI();
	bool GetActive();
}
public class GUIWindowScripts : MonoBehaviour, IWindow, IPointerDownHandler, IPointerUpHandler {

	Vector2 mousePos;
	bool selected;
	public RectTransform container;
	public virtual void CloseUI() {
		ResourceManager.PlayClipByID("clip_back");
		gameObject.SetActive(false);
	}

	public virtual void ToggleActive() {
		bool active = gameObject.activeSelf;
		if(active) CloseUI();
		else {
			gameObject.SetActive(true);
			GetComponent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
			PlayerViewScript.SetCurrentWindow(this);
		}
	}

	public virtual bool GetActive() {
		return gameObject ? gameObject.activeSelf : false;
	}

    public void OnPointerDown(PointerEventData eventData) {
		Debug.Log("hi");
        mousePos = (Vector2)Input.mousePosition - container.anchoredPosition;
		selected = true;
    }

	public void OnPointerUp(PointerEventData eventData) {
		Debug.Log("low");
		selected = false;
	}

	void Update() {
		if(selected) {
			container.anchoredPosition = (Vector2)Input.mousePosition - mousePos;
		}
	}
}
