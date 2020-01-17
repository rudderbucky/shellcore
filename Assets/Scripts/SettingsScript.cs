using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour {

	public Slider masterSoundSlider;
	public Slider musicSlider;
	public Slider soundSlider;
	public Toggle HUDArrowScriptToggle;
	public Toggle BackgroundScriptToggle;
	public Toggle RectangleEffectScriptToggle;

	public (int, int)[] resolutions = new (int, int)[] {(1024, 768), (1366, 768), (1920, 1080), (3840, 2160)};
	public Dropdown windowResolution;
	public Toggle windowedMode;

	void Start() {
		// get playerpref values and configure toggles and sliders based on them
		HUDArrowScriptToggle.isOn = PlayerPrefs.GetString("HUDArrowScript_active", "False") == "True";
		BackgroundScriptToggle.isOn = PlayerPrefs.GetString("BackgroundScript_active", "True") == "True";
		RectangleEffectScriptToggle.isOn = PlayerPrefs.GetString("RectangleEffectScript_active", "True") == "True";;
		masterSoundSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
		musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
		soundSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
		windowedMode.isOn = (Screen.fullScreenMode == FullScreenMode.Windowed || Screen.fullScreenMode == FullScreenMode.MaximizedWindow);
		windowResolution.value = FindResolution();
	}

	int FindResolution()
	{
		for(int i = 0; i < resolutions.Length; i++)
		{
			if(resolutions[i].Item1 == Screen.width) return i;
		}
		return -1;
	}

	public void ChangeMasterVolume(float newVol)
	{
		AudioManager.instance.ChangeMasterVolume(newVol);
	}

	public void ChangeSoundEffectsVolume(float newVol)
	{
		AudioManager.instance.ChangeSoundEffectsVolume(newVol);
	}

	public void ChangeMusicVolume(float newVol)
	{
		AudioManager.instance.ChangeMusicVolume(newVol);
	}

	public void ToggleWindowMode(bool val)
	{
		#if UNITY_EDITOR
		#else
		Screen.fullScreenMode = val ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
		#endif
	}

	public void ChangeResolution(int index)
	{
		#if UNITY_EDITOR
		#else
		Screen.SetResolution(resolutions[index].Item1, resolutions[index].Item2, Screen.fullScreenMode, 0);
		#endif
	}

	public void ChangeHUDArrowScriptActive(bool val)
	{
		PlayerPrefs.SetString("HUDArrowScript_active", val + "");
		HUDArrowScript.SetActive(val);
	}

	public void ChangeBackgroundScriptActive(bool val)
	{
		PlayerPrefs.SetString("BackgroundScript_active", val + "");
		BackgroundScript.SetActive(val);
	}

	public void ChangeRectangleEffectScriptActive(bool val)
	{
		PlayerPrefs.SetString("RectangleEffectScript_active", val + "");
		RectangleEffectScript.SetActive(val);
	}
}
