using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;

public class ShipBuilderSortingButton : MonoBehaviour, IPointerClickHandler
{
	Button button;
	Text text;
	void Start() {
		button = GetComponent<Button>();
		text = GetComponentInChildren<Text>();
		button.image.color = text.color = Color.green;
	}
	void OnEnable() {
		if(button) button.image.color = text.color = Color.green;
	}
    public void OnPointerClick(PointerEventData eventData)
    {
        button.image.color = text.color = button.image.color == Color.gray ? Color.green : Color.gray;
    }
}
