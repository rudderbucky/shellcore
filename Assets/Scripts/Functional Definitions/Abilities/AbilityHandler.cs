using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handler for abilities used by THE PLAYER on the GUI
/// </summary>
public class AbilityHandler : MonoBehaviour {

    public GameObject betterBGbox;
    public Dictionary<string, AbilityButtonScript> betterBGboxArray;
    public AbilityHotkeyStruct visibleAbilityOrder = new AbilityHotkeyStruct();
    public Image HUDbg; // the grey area in which the ability icons sit
    private bool initialized; // check for the update method
    private PlayerCore core; // the player
    private Ability[] abilities; // ability array of the core
    private Image image; // image prefab for the actual ability image


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
    public static float tileSpacing;

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

        betterBGboxArray = new Dictionary<string, AbilityButtonScript>();

        tileSpacing = betterBGbox.GetComponent<Image>().sprite.bounds.size.x * 30; // Used to space out the abilities on the GUI
        
        for (int i = 0; i < visibleAbilities.Count; i++)
        { // iterate through to display all the abilities
            if (visibleAbilities[i] == null) break;
            
            // position them all, do not keep the world position
            var id = (AbilityID)visibleAbilities[i].GetID();
            var key = id != AbilityID.SpawnDrone ? $"{(int)id}" : GetAHSpawnData((visibleAbilities[i] as SpawnDrone).spawnData.drone);

            if(!betterBGboxArray.ContainsKey(key))
            {
                Vector3 pos = new Vector3(GetAbilityPos(betterBGboxArray.Count), tileSpacing*0.8F, this.transform.position.z); // find where to position the images
                betterBGboxArray.Add(key,
                    Instantiate(betterBGbox, pos, Quaternion.identity).GetComponent<AbilityButtonScript>());
                betterBGboxArray[key].transform.SetParent(transform, false); // set parent (do not keep world position)
                betterBGboxArray[key].Init(visibleAbilities[i], i < 9 && currentVisibles != AbilityTypes.Passive ? keybindList[betterBGboxArray.Count-1] + "" : null, core,
                    KeyName.Ability0 + (betterBGboxArray.Count - 1));
            }
            else betterBGboxArray[key].AddAbility(visibleAbilities[i]);
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

        visibleAbilityOrder = player.cursave.abilityHotkeys;
        if(visibleAbilityOrder.skills == null)
        {
            visibleAbilityOrder = new AbilityHotkeyStruct()
            {
                skills = new List<AbilityID>(),
                spawns = new List<string>(),
                weapons = new List<AbilityID>(),
                passive = new List<AbilityID>()
            };
        }

        if(visibleAbilityOrder.GetList((int)currentVisibles) == null || visibleAbilityOrder.GetList((int)currentVisibles).Count == 0)
        {
            visibleAbilityOrder.GetList((int)currentVisibles).Clear();
            foreach(var i in instance.betterBGboxArray.Keys)
            {
                if(currentVisibles == AbilityTypes.Spawns)
                    (visibleAbilityOrder.GetList((int)currentVisibles) as List<string>).Add(i);
                else
                    (visibleAbilityOrder.GetList((int)currentVisibles) as List<AbilityID>).Add((AbilityID)int.Parse(i));
            }
        }
        else
        {
            Rearrange();
        }
    }

    private static string GetAHSpawnData(string data)
    {
        if(DroneUtilities.GetDroneSpawnDataByShorthand(data).drone != null)
            return DroneUtilities.GetDroneSpawnDataByShorthand(data).drone;
        else return data;
    }

    public static float GetAbilityPos(int index)
    {
        return tileSpacing * (0.8F*index+0.5F);
    }

    public static void RearrangeID(float xPos, AbilityID id, string droneData)
    {
        if(id != AbilityID.SpawnDrone)
        {
            var list = (instance.visibleAbilityOrder.GetList((int)instance.currentVisibles) as List<AbilityID>);
            int index = GetAbilityPosInverse(xPos);
            list.Remove(id);
            list.Insert(index, id);
        }
        else
        {
            var list = (instance.visibleAbilityOrder.GetList((int)instance.currentVisibles) as List<string>);
            int index = GetAbilityPosInverse(xPos);
            list.Remove(droneData);
            list.Insert(index, droneData);
        }
        
        instance.Rearrange();
    }

    private static int GetAbilityPosInverse(float xPos)
    {
        return Mathf.Min(Mathf.Max(0, Mathf.RoundToInt(((xPos / tileSpacing) - 0.5F) / 0.8F)), instance.betterBGboxArray.Count - 1);
    }

    private void Rearrange()
    {
        if(!core.GetIsInteracting())
        {
            core.cursave.abilityHotkeys = visibleAbilityOrder;
        }
    

        int i = 0;
        var list =  visibleAbilityOrder.GetList((int)currentVisibles);
        while(i < list.Count)
        {
            string key = ConvertObjectToString(list, i);

            if(!betterBGboxArray.ContainsKey(key))
            {
                list.RemoveAt(i);
            }
            else i++;
        }

        foreach(var key in betterBGboxArray.Keys)
        {
            if(currentVisibles == AbilityTypes.Spawns)
            {
                if(!list.Contains(key))
                {
                    list.Add(key);
                }
            }
            else
            {
                if(!list.Contains((AbilityID)int.Parse(key)))
                {
                    list.Add((AbilityID)int.Parse(key));
                } 
            }
        }

        ReorientAbilityBoxes();
    }
    
    public void ReorientAbilityBoxes()
    {
        var list = visibleAbilityOrder.GetList((int)currentVisibles);
        for(int i = 0; i < list.Count; i++)
        {
            betterBGboxArray[ConvertObjectToString(list, i)].transform.position = new Vector3(GetAbilityPos(i), 
                tileSpacing*0.8F, transform.position.z);
            betterBGboxArray[ConvertObjectToString(list, i)].ReflectHotkey(KeyName.Ability0 + i);
        }
    }

    public string ConvertObjectToString(IList list, int i)
    {
        var idList = visibleAbilityOrder.GetList((int)currentVisibles) as List<AbilityID>;
        var spawnList = visibleAbilityOrder.GetList((int)currentVisibles) as List<string>;
        if(idList != null)
        {
            return $"{(int)idList[i]}";
        }
        else
        {
            return spawnList[i];
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
                if (visibleAbilities.Contains(abilities[i]))
                {

                }
                else //abilities[i].UpdateState();
                    abilities[i].Tick();
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
