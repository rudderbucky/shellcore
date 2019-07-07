using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectorCreatorShellCoreEditor : MonoBehaviour, IWindow {

	private SectorCreatorMouse.PlaceableObject currentCore;
	private SectorCreatorMouse mouse;
	public GameObject window;
	public GameObject container;
    private bool shellcore;

	public void Initialize(SectorCreatorMouse.PlaceableObject core, SectorCreatorMouse mouse) {
        shellcore = core.assetID == "shellcore_blueprint";
        container.transform.Find("JSONField").gameObject.SetActive(shellcore);
        if (core.type == SectorCreatorMouse.ObjectTypes.Flag)
            container.transform.Find("Dropdown").gameObject.SetActive(false);

        PlayerViewScript.SetCurrentWindow(this);
		this.mouse = mouse;
		currentCore = core;
		window.SetActive(true);
		if(currentCore.shellcoreJSON != null)
            container.transform.Find("JSONField").GetComponent<InputField>().text = currentCore.shellcoreJSON;
        if (core.type != SectorCreatorMouse.ObjectTypes.Flag)
            container.GetComponentInChildren<Dropdown>().value = currentCore.faction;
        container.transform.Find("IDField").GetComponent<InputField>().text = core.ID;
    }

	public void SetValues() {
		SectorCreatorMouse.PlaceableObject newObj = currentCore;
        if (currentCore.type != SectorCreatorMouse.ObjectTypes.Flag)
            newObj.faction = container.GetComponentInChildren<Dropdown>().value;
        if(shellcore)
		    newObj.shellcoreJSON = container.transform.Find("JSONField").GetComponent<InputField>().text;
        newObj.ID = container.transform.Find("IDField").GetComponent<InputField>().text;
        mouse.objects.Remove(currentCore);
		mouse.objects.Add(newObj);
		mouse.UpdateColors();
		CloseUI();
	}

	public bool GetActive() {
		return gameObject.activeSelf;
	}
    public void CloseUI()
    {
		ResourceManager.PlayClipByID("clip_back");
        window.SetActive(false);
    }
}
