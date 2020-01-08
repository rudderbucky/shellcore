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
	public bool playSoundOnClose = true;
	public virtual void CloseUI() {
		if(transform.parent.gameObject.activeSelf) {
			if(playSoundOnClose) AudioManager.PlayClipByID("clip_back", true);
			transform.parent.gameObject.SetActive(false);
		}
	}

	public virtual void Activate() {
		transform.parent.gameObject.SetActive(true);
		GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
		PlayerViewScript.SetCurrentWindow(this);
	}
	public virtual void ToggleActive() {
		bool active = transform.parent.gameObject.activeSelf;
		if(active) CloseUI();
		else {
			transform.parent.gameObject.SetActive(true);
			GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
			PlayerViewScript.SetCurrentWindow(this);
		}
	}

	public virtual bool GetActive() {
		return transform && transform.parent ? transform.parent.gameObject.activeSelf : false;
	}

	public virtual void OnPointerDown(PointerEventData eventData) {
		mousePos = (Vector2)Input.mousePosition - GetComponent<RectTransform>().anchoredPosition;
		selected = true;
	}

	public void OnPointerUp(PointerEventData eventData) {
		selected = false;
	}

	protected virtual void Update() {
		if(selected) {
			GetComponent<RectTransform>().anchoredPosition = (Vector2)Input.mousePosition - mousePos;
		}
	}
}
