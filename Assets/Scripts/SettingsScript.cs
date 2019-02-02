using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour {

	void OnEnable() {
		Toggle[] toggles = GetComponentsInChildren<Toggle>();
		Slider slider = GetComponentInChildren<Slider>();
		toggles[0].isOn = HUDArrowScript.active;
		toggles[1].isOn = BackgroundScript.active;
		toggles[2].isOn = RectangleEffectScript.active;
		slider.value = ResourceManager.soundVolume;
	}
}
