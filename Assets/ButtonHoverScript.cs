using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;

public class ButtonHoverScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
	float timer; // used for animation
	bool mouseTag; // used for animation
	RectTransform rect; // used for animation
    public void OnPointerClick(PointerEventData eventData)
    {
			if(name == "MainMenuButton") SceneManager.LoadScene("MainMenu");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
		mouseTag = true;
		timer = 1; // needs to start at 1 because maths
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseTag = false;
    }

    // Use this for initialization
    void Start () {
		rect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		if(mouseTag) {
        	rect.sizeDelta = new Vector2(rect.sizeDelta.x + 7F/(Mathf.Pow(timer += Time.deltaTime, 4)), 30);
			// math for animation
		} else rect.sizeDelta = new Vector2(190, 30);
	}
}
