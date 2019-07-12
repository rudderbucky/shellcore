using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectorCreatorShellCoreEditor : MonoBehaviour, IWindow {

	private SectorCreatorMouse.PlaceableObject currentCore;
	private SectorCreatorMouse mouse;
	public GameObject window;
    public InputField JSONField;
    public InputField IDField;
    public Dropdown factionDropdown;
    private bool shellcore;

	public void Initialize(SectorCreatorMouse.PlaceableObject core, SectorCreatorMouse mouse) {
        shellcore = core.assetID == "shellcore_blueprint";
        JSONField.transform.parent.gameObject.SetActive(shellcore);
        if (core.type == SectorCreatorMouse.ObjectTypes.Flag)
            factionDropdown.transform.parent.gameObject.SetActive(false);

        PlayerViewScript.SetCurrentWindow(this);
		this.mouse = mouse;
		currentCore = core;
		window.SetActive(true);
		if(currentCore.shellcoreJSON != null)
            JSONField.text = currentCore.shellcoreJSON;
        if (core.type != SectorCreatorMouse.ObjectTypes.Flag)
            factionDropdown.value = currentCore.faction;
        IDField.text = core.obj.name;
    }

	public void SetValues() {
		SectorCreatorMouse.PlaceableObject newObj = currentCore;
        if (currentCore.type != SectorCreatorMouse.ObjectTypes.Flag)
            newObj.faction = factionDropdown.value;
        if(shellcore)
		    newObj.shellcoreJSON = JSONField.text;
        newObj.obj.name = IDField.text;
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
