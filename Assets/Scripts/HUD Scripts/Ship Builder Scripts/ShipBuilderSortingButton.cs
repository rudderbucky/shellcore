using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuilderSortingButton : MonoBehaviour
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

	public void ChangeColor() {
        button.image.color = text.color = button.image.color == Color.gray ? Color.green : Color.gray;
	}
}
