using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SecCrExitButton : MonoBehaviour, IPointerClickHandler {

	public SectorCreatorMouse mouse;
	// Use this for initialization
	void Start () {
		
	}
	
	public void OnClick() {
		Debug.Log("hi");
	}
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        mouse.windowEnabled = false;
		transform.root.Find("DialogueBox").gameObject.SetActive(false);
    }
}
