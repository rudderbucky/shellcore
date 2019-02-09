using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectorCreatorShellCoreEditor : MonoBehaviour, IWindow {

	private SectorCreatorMouse.PlaceableObject currentCore;
	private SectorCreatorMouse mouse;
	public GameObject window;

	public void Initialize(SectorCreatorMouse.PlaceableObject core, SectorCreatorMouse mouse) {
		PlayerViewScript.SetCurrentWindow(this);
		this.mouse = mouse;
		currentCore = core;
		window.SetActive(true);
		if(currentCore.shellcoreJSON != null) window.GetComponentInChildren<InputField>().text = currentCore.shellcoreJSON;
		window.GetComponentInChildren<Dropdown>().value = currentCore.faction;
	}

	public void SetValues() {
		SectorCreatorMouse.PlaceableObject newObj = currentCore;
		newObj.faction = window.GetComponentInChildren<Dropdown>().value;
		newObj.shellcoreJSON = window.GetComponentInChildren<InputField>().text;
		mouse.objects.Remove(currentCore);
		mouse.objects.Add(newObj);
		mouse.UpdateColors();
		CloseUI();
	}
    public void CloseUI()
    {
        window.SetActive(false);
    }
}
