using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for abilities used by THE PLAYER on the GUI
/// </summary>
public class AbilityHandler : MonoBehaviour {

    public GameObject abilityBackground; // background of the ability
    public Image abilityCDIndicator; // used to indicate if the ability is on cooldown
    public Image abilityGleam; // gleam for the ability
    public GameObject betterBGbox;
    public Dictionary<AbilityID, AbilityButtonScript> betterBGboxArray;
    public List<AbilityID>[] visibleAbilityOrder = new List<AbilityID>[4];
    public GameObject tooltipPrefab; // Prefab for showing information when mouse hovers over the ability button
    public Image HUDbg; // the grey area in which the ability icons sit
    private bool initialized; // check for the update method
    private PlayerCore core; // the player
    private Ability[] abilities; // ability array of the core
    private Image image; // image prefab for the actual ability image
    private Image[] abilityImagesArray; // images of the abilities displayed on the GUI
    private GameObject[] abilityBackgroundArray; // all ability backgrounds displayed on the GUI
    private Image[] abilityCDIndicatorArray; // all ability cooldown indicators displayed on the GUI
    private Image[] abilityGleamArray; // all ability gleams displayed on the GUI
    private Image[] abilityTierArray; // all ability tier images displayed on the GUI 
    private bool[] gleaming; // array to check whether an ability is currently gleaming
    private bool[] gleamed; // array to check whether an ability has already gleamed in the cycle

    public enum AbilityTypes {
        Skills,
        Spawns,
        Weapons,
        Passive,
        None
    }
    
    public AbilityTypes currentVisibles;
    public List<Ability> visibleAbilities = new List<Ability>();
    Ability[] displayAbs;
    public static string[] keybindList; // list of keys for ability binds
    public static AbilityHandler instance;
    public float tileSpacing;

    public void SetCurrentVisible(AbilityTypes type) {
        if(currentVisibles != type) {
            currentVisibles = type;
            Deinitialize();
            if(displayAbs == null) Initialize(core);
            else Initialize(core, displayAbs);
        }
    }
    /// <summary>
    /// Initialization of the ability handler that is tied to the player
    /// </summary>
    public void Initialize(PlayerCore player, Ability[] displayAbilities = null) {
        instance = this;
        core = player;
        if(displayAbilities == null) abilities = core.GetAbilities(); // Get the core's ability array
        else abilities = displayAbilities;
        displayAbs = displayAbilities;
        visibleAbilities.Clear();
        foreach (Ability ab in abilities) {
            switch(currentVisibles) {
                case AbilityTypes.Skills:
                    if(ab as Ability && !(ab as SpawnDrone) && !(ab as WeaponAbility) && !(ab as PassiveAbility))
                        visibleAbilities.Add(ab);
                    break;
                case AbilityTypes.Passive:
                    if(ab as PassiveAbility)
                        visibleAbilities.Add(ab);
                    break;
                case AbilityTypes.Spawns:
                    if(ab as SpawnDrone)
                        visibleAbilities.Add(ab);
                    break;
                case AbilityTypes.Weapons:
                    if(ab as WeaponAbility)
                        visibleAbilities.Add(ab);
                    break;
            }
        }

        keybindList = new string[10];
        for(int i = 0; i < 9; i++)
        {
            keybindList[i] = PlayerPrefs.GetString("AbilityHandler_abilityKeybind" + i, (i + 1) + "");
        }

        betterBGboxArray = new Dictionary<AbilityID, AbilityButtonScript>();

        tileSpacing = betterBGbox.GetComponent<Image>().sprite.bounds.size.x * 30; // Used to space out the abilities on the GUI
        
        for (int i = 0; i < visibleAbilities.Count; i++)
        { // iterate through to display all the abilities
            if (visibleAbilities[i] == null) break;
            
            // position them all, do not keep the world position
            var id = (AbilityID)visibleAbilities[i].GetID();
            if(!betterBGboxArray.ContainsKey(id))
            {
                Vector3 pos = new Vector3(GetAbilityPos(betterBGboxArray.Count), tileSpacing*0.8F, this.transform.position.z); // find where to position the images
                betterBGboxArray.Add(id,
                    Instantiate(betterBGbox, pos, Quaternion.identity).GetComponent<AbilityButtonScript>());
                betterBGboxArray[id].transform.SetParent(transform, false); // set parent (do not keep world position)
                betterBGboxArray[id].Init(visibleAbilities[i], i < 9 && currentVisibles != AbilityTypes.Passive ? keybindList[betterBGboxArray.Count-1] + "" : null, core);
            }
            else betterBGboxArray[id].AddAbility(visibleAbilities[i]);
            
        }

        var HUDbgrectTransform = HUDbg.GetComponent<RectTransform>();
        if(visibleAbilities.Count > 0)
        {
            var y = HUDbgrectTransform.anchoredPosition;
            y.x = 0.5f * tileSpacing - 1F * tileSpacing;
            HUDbgrectTransform.anchoredPosition = y;

            var x = HUDbgrectTransform.sizeDelta;
            x.x = GetAbilityPos(betterBGboxArray.Count-1) + GetAbilityPos(0) - y.x;
            HUDbgrectTransform.sizeDelta = x;

        } else HUDbgrectTransform.sizeDelta = new Vector2(0, HUDbgrectTransform.sizeDelta.y);

        if (image) Destroy(image.gameObject);
        if(displayAbilities == null) initialized = true;
        // handler completely initialized, safe to update now
        // if display abilities were passed the handler must not update since it is merely representing
        // some abilities

        visibleAbilityOrder = new List<AbilityID>[] {player.cursave.abilityHotkeys.skills, player.cursave.abilityHotkeys.spawns,
            player.cursave.abilityHotkeys.weapons, player.cursave.abilityHotkeys.passive};
        if(visibleAbilityOrder == null || visibleAbilityOrder[0] == null)
        {
            visibleAbilityOrder = new List<AbilityID>[] {null, null, null, null};
        }

        if(visibleAbilityOrder[(int)currentVisibles] == null || visibleAbilityOrder[(int)currentVisibles].Count == 0)
        {
            visibleAbilityOrder[(int)currentVisibles] = new List<AbilityID>();
            foreach(var i in instance.betterBGboxArray.Keys)
            {
                visibleAbilityOrder[(int)currentVisibles].Add(i);
            }
        }
        else
        {
            Rearrange();
        }
    }

    public static float GetAbilityPos(int index)
    {
        return instance.tileSpacing * (0.8F*index+0.5F);
    }

    public static void RearrangeID(float xPos, AbilityID id)
    {
        int index = GetAbilityPosInverse(xPos);
        instance.visibleAbilityOrder[(int)instance.currentVisibles].Remove(id);
        instance.visibleAbilityOrder[(int)instance.currentVisibles].Insert(index, id);
        instance.Rearrange();
    }

    private static int GetAbilityPosInverse(float xPos)
    {
        instance.Rearrange();
        return Mathf.Min(Mathf.Max(0, Mathf.RoundToInt(((xPos / instance.tileSpacing) - 0.5F) / 0.8F)), instance.betterBGboxArray.Count - 1);
    }

    private void Rearrange()
    {
        if(!core.GetIsInteracting())
        {
            core.cursave.abilityHotkeys = new PlayerSave.AbilityHotkeyStruct()
            {
                skills = visibleAbilityOrder[0],
                spawns = visibleAbilityOrder[1],
                weapons = visibleAbilityOrder[2],
                passive = visibleAbilityOrder[3]
            };
        }
        
        int i = 0;
        while(i < visibleAbilityOrder[(int)currentVisibles].Count)
        {
            if(!betterBGboxArray.ContainsKey(visibleAbilityOrder[(int)currentVisibles][i]))
            {
                visibleAbilityOrder[(int)currentVisibles].RemoveAt(i);
            }
            else i++;
        }


        for(i = 0; i < visibleAbilityOrder[(int)currentVisibles].Count; i++)
        {
            instance.betterBGboxArray[visibleAbilityOrder[(int)currentVisibles][i]].transform.position = new Vector3(GetAbilityPos(i), 
                tileSpacing*0.8F, this.transform.position.z);
            instance.betterBGboxArray[visibleAbilityOrder[(int)currentVisibles][i]].ReflectHotkey(currentVisibles != AbilityTypes.Passive ? keybindList[i] : null);
        }
    }

    public static void ChangeKeybind(int index, string val)
    {
        keybindList[index] = val;
        PlayerPrefs.SetString("AbilityHandler_abilityKeybind" + index, val);
        
        instance.Deinitialize();
        if(instance.displayAbs == null) instance.Initialize(instance.core);
        else instance.Initialize(instance.core, instance.displayAbs);
    }

    /// <summary>
    /// Deinitializes the Ability Handler UI
    /// </summary>
    public void Deinitialize()
    {
        for(int i = 5; i < transform.childCount; i++)  //Start from 5, because index 1 is the background
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        initialized = false; // reset initialized
    }

    // Update is called once per frame
    private void Update () {
        if (initialized) // check if safe to update
        {
            for (int i = 0; i < core.GetAbilities().Length; i++)
            { // update all abilities
                if (abilities[i] == null || core.GetIsDead()) continue; 
                // skip ability instead of break because further abilities may not be destroyed
                if(visibleAbilities.Contains(abilities[i])) {
                    
                } else abilities[i].Tick(0);
            }
            if(core.GetIsDead() || core.GetIsInteracting()) return;
            if(InputManager.GetKeyDown(KeyName.ShowSkills)) {
                SetCurrentVisible((AbilityTypes)(0));
            }
            if(InputManager.GetKeyDown(KeyName.ShowSpawns)) {
                SetCurrentVisible((AbilityTypes)(1));
            }
            if(InputManager.GetKeyDown(KeyName.ShowWeapons)) {
                SetCurrentVisible((AbilityTypes)(2));
            }
            if(InputManager.GetKeyDown(KeyName.ShowPassives)) {
                SetCurrentVisible((AbilityTypes)(3));
            }
        }
	}
}
