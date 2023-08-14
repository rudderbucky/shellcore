using UnityEngine;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    public Dropdown uiScaleSlider;
    public float[] uiScales = new float[] { 0.25f, 0.5f, 1f, 1.5f, 2f, 4f };
    public Slider masterSoundSlider;
    public Slider musicSlider;
    public Slider soundSlider;
    public Slider hudDamageIndicatorSlider;
    public Toggle HUDArrowScriptToggle;
    public Toggle BackgroundScriptToggle;
    public Toggle RectangleEffectScriptToggle;
    public Toggle overworldGridToggle;
    public Toggle coreGlowToggle;

    public (int, int)[] resolutions = new (int, int)[] {(1024, 768), (1366, 768), (1600, 900), (1920, 1080), (3840, 2160)};
    public Dropdown windowResolution;
    public Toggle windowedMode;
    public Dropdown dialogueStyle;
    public Dropdown partShader;
    public Toggle taskManagerAutoSaveEnabled;
    public Toggle simpleMouseMovementToggle;
    public Toggle allowAutocastSkillsToggle;

    public Transform controlsSection;
    //public InputField[] abilityKeybindFields;

    public GameObject keybindItemPrefab;
    public RectTransform keybindBG;
    GameObject[] keybinds;
    public bool initialized = false;

    void Start()
    {
        // get playerpref values and configure toggles and sliders based on them
        uiScaleSlider.value = findClosestScaleIndex(PlayerPrefs.GetFloat("UIScale", 1f));
        HUDArrowScriptToggle.isOn = PlayerPrefs.GetString("HUDArrowScript_active", "False") == "True";
        BackgroundScriptToggle.isOn = PlayerPrefs.GetString("BackgroundScript_active", "True") == "True";
        RectangleEffectScriptToggle.isOn = PlayerPrefs.GetString("RectangleEffectScript_active", "True") == "True";
        overworldGridToggle.isOn = PlayerPrefs.GetString("OverworldGrid_active", "False") == "True";
        coreGlowToggle.isOn = PlayerPrefs.GetString("CoreGlow_active", "True") == "True";
        masterSoundSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        hudDamageIndicatorSlider.value = PlayerPrefs.GetFloat("HealthBarScript_hudDamageIndicator", 0.5F);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        dialogueStyle.value = PlayerPrefs.GetInt("DialogueSystem_dialogueStyle", 0);
        partShader.value = PlayerPrefs.GetInt("ShellPart_partShader", 0);
        taskManagerAutoSaveEnabled.isOn = PlayerPrefs.GetString("TaskManager_autoSaveEnabled", "True") == "True";
        simpleMouseMovementToggle.isOn = PlayerPrefs.GetString("SelectionBoxScript_simpleMouseMovement", "True") == "True";
        allowAutocastSkillsToggle.isOn = PlayerPrefs.GetString("AllowAutocastSkills", "False") == "True";
        SaveSettings();

        //for(int i = 0; i < 9; i++)
        //{
        //	abilityKeybindFields[i].text = PlayerPrefs.GetString("AbilityHandler_abilityKeybind" + i, (i + 1) + "");
        //}

        keybindBG.sizeDelta = new Vector2(0f, 64f * InputManager.keys.Count);

        for (int i = 0; i < InputManager.keys.Count; i++)
        {
            var obj = Instantiate(keybindItemPrefab);
            var rt = obj.GetComponent<RectTransform>();
            rt.SetParent(keybindBG);
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(0f, 64);
            rt.anchoredPosition = new Vector2(0f, -64f * i);

            Text key = obj.transform.Find("KeyBG").GetComponentInChildren<Text>();
            key.text = InputManager.keys[(KeyName)i].overrideKey.ToString();

            Text description = obj.transform.Find("Description").GetComponent<Text>();
            description.text = InputManager.keys[(KeyName)i].description;

            var keyName = (KeyName)i;
            obj.GetComponent<Button>().onClick.AddListener(() => { InputManager.ChangeControl(keyName, key); });
        }

        windowedMode.isOn = (Screen.fullScreenMode == FullScreenMode.Windowed || Screen.fullScreenMode == FullScreenMode.MaximizedWindow);
        windowResolution.value = PlayerPrefs.GetInt("Screen_defaultResolution", FindResolution() != -1 ? FindResolution() : 3);
        initialized = true;
    }

    int findClosestScaleIndex(float scale)
    {
        int bestIndex = 0;
        float distance = float.MaxValue;
        for (int i = 0; i < uiScales.Length; i++)
        {
            float newDistance = Mathf.Abs(scale - uiScales[i]);
            if (newDistance < distance)
            {
                distance = newDistance;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    int FindResolution()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].Item1 == Screen.width)
            {
                return i;
            }
        }

        return -1;
    }

    public void SaveSettings()
    {
        UpdateVolumes();
        ChangeUIScale(uiScaleSlider.value);
        ChangeHUDArrowScriptActive(HUDArrowScriptToggle.isOn);
        ChangeBackgroundScriptActive(BackgroundScriptToggle.isOn);
        ChangeRectangleEffectScriptActive(RectangleEffectScriptToggle.isOn);
        ChangeTaskManagerAutoSaveEnabled(taskManagerAutoSaveEnabled.isOn);
        ChangeDialogueSystemDialogueStyle(dialogueStyle.value);
        ChangeSimpleMouseMovementEnabled(simpleMouseMovementToggle.isOn);
        ChangeShellPartPartShader(partShader.value);
        ChangeHudDamageIndicator(hudDamageIndicatorSlider.value);
        ChangeAllowAutocastSkillsEnabled(allowAutocastSkillsToggle.isOn);
        ChangeOverworldGridActive(overworldGridToggle.isOn);
        ChangeCoreGlowActive(coreGlowToggle.isOn);

        //for(int i = 0; i < 9; i++)
        //{
        //	ChangeAbilityKeybind(i, abilityKeybindFields[i].text);
        //}
    }

    public void ChangeMasterVolume(float newVol)
    {
        if (AudioManager.instance)
        {
            AudioManager.instance.ChangeMasterVolume(newVol);
        }
    }

    public void ChangeSoundEffectsVolume(float newVol)
    {
        if (AudioManager.instance)
        {
            AudioManager.instance.ChangeSoundEffectsVolume(newVol);
        }
    }

    public void ChangeMusicVolume(float newVol)
    {
        if (AudioManager.instance)
        {
            AudioManager.instance.ChangeMusicVolume(newVol);
        }
    }

    public void ToggleWindowMode(bool val)
    {
#if UNITY_EDITOR
#else
       //Screen.SetResolution(Display.main.renderingWidth, Display.main.renderingHeight, FullScreenMode.FullScreenWindow, 0);
	    Screen.fullScreenMode = val ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
        if (!val)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        }
#endif
    }

    public void ChangeResolution(int index)
    {
        PlayerPrefs.SetInt("Screen_defaultResolution", index);
#if UNITY_EDITOR
#else
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) 
        {
        }
        else
        {
            //
        }
#endif
    }

    public void ChangeUIScale(int index)
    {
        float scale = uiScales[index];

        PlayerPrefs.SetFloat("UIScale", scale);
        UIScalerScript.SetScale(scale);
    }

    public void ChangeHUDArrowScriptActive(bool val)
    {
        PlayerPrefs.SetString("HUDArrowScript_active", val.ToString());
        HUDArrowScript.SetActive(val);
    }

    public void ChangeBackgroundScriptActive(bool val)
    {
        PlayerPrefs.SetString("BackgroundScript_active", val.ToString());
        BackgroundScript.SetActive(val);
    }

    public void ChangeRectangleEffectScriptActive(bool val)
    {
        PlayerPrefs.SetString("RectangleEffectScript_active", val.ToString());
        RectangleEffectScript.SetActive(val);
    }


    public void ChangeOverworldGridActive(bool val)
    {
        PlayerPrefs.SetString("OverworldGrid_active", val.ToString());
        OverworldGrid.SetActive(val);
    }


    public void ChangeCoreGlowActive(bool val)
    {
        PlayerPrefs.SetString("CoreGlow_active", val.ToString());
        foreach (var ent in AIData.entities)
        {
            ent.SetCoreGlowActive(val);
        }
    }

    public void ChangeDialogueSystemDialogueStyle(int val)
    {
        PlayerPrefs.SetInt("DialogueSystem_dialogueStyle", val);
        DialogueSystem.dialogueStyle = (DialogueSystem.DialogueStyle)val;
    }

    public void ChangeHudDamageIndicator(float newVol)
    {
        PlayerPrefs.SetFloat("HealthBarScript_hudDamageIndicator", newVol);
        if (HealthBarScript.instance)
        {
            HealthBarScript.instance.ChangeHudDamageIndicator(newVol);
        }

        var text = hudDamageIndicatorSlider.GetComponentInChildren<Text>();
        text.text = "Damage Indicator ";
        if (newVol == 0F)
        {
            text.text += "(Disabled)";
        }
        else if (newVol <= 0.5F)
        {
            text.text += $"({(int)(newVol * 200)}% Core)";
        }
        else
        {
            text.text += $"({(int)((newVol - 0.5F) * 200)}% Shell)";
        }
    }

    public void ChangeShellPartPartShader(int val)
    {
        PlayerPrefs.SetInt("ShellPart_partShader", val);
        ShellPart.partShader = val;
    }

    public void ChangeTaskManagerAutoSaveEnabled(bool val)
    {
        PlayerPrefs.SetString("TaskManager_autoSaveEnabled", val.ToString());
        TaskManager.autoSaveEnabled = val;
    }

    public void ChangeSimpleMouseMovementEnabled(bool val)
    {
        PlayerPrefs.SetString("SelectionBoxScript_simpleMouseMovement", val.ToString());
        SelectionBoxScript.simpleMouseMovement = val;
    }

    public void ChangeAllowAutocastSkillsEnabled(bool val)
    {
        PlayerPrefs.SetString("AllowAutocastSkills", val.ToString());
        //SelectionBoxScript.simpleMouseMovement = val;//TODO
    }

    // Volume slider updates without having to save
    public void UpdateVolumes()
    {
        if (initialized)
        {
            ChangeMasterVolume(masterSoundSlider.value);
            ChangeMusicVolume(musicSlider.value);
            ChangeSoundEffectsVolume(soundSlider.value);
        }
    }
}
