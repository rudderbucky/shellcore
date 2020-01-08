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
		HUDArrowScriptToggle.isOn = HUDArrowScript.active;
		BackgroundScriptToggle.isOn = BackgroundScript.active;
		RectangleEffectScriptToggle.isOn = RectangleEffectScript.active;
		masterSoundSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
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
}
