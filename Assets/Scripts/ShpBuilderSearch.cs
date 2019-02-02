using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShpBuilderSearch : MonoBehaviour {

	public ShipBuilder builder;
	public InputField input;

	void Start() {
		input.onValueChanged.AddListener(delegate {builder.SetSearcherString(input.text); });
	}
}
