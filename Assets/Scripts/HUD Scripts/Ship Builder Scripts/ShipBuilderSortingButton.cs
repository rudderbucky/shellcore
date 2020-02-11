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
		text.color = Color.green;
	}
	void OnEnable() {
		if(button) text.color = Color.green;
	}

	public void ChangeColor() {
        text.color = text.color == Color.gray ? Color.green : Color.gray;
	}
}
