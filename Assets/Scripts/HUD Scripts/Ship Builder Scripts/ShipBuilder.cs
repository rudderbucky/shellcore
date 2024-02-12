﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class ShipBuilder : GUIWindowScripts
{
    public Vector3 yardPosition;
    public Image shell;
    public Image core;
    public ShipBuilderCursorScript cursorScript;
    public GameObject buttonPrefab;
    public PlayerCore player;
    public Transform smallContents;
    public Transform mediumContents;
    public Transform largeContents;
    public Transform traderSmallContents;
    public Transform traderMediumContents;
    public Transform traderLargeContents;
    public GameObject smallText;
    public GameObject mediumText;
    public GameObject largeText;
    public GameObject traderSmallText;
    public GameObject traderMediumText;
    public GameObject traderLargeText;
    public Image reconstructImage;
    public Text reconstructText;
    public TipsFromTheYard tips;
    public GameObject traderScrollView;
    public BuilderMode mode;
    public SelectionDisplayHandler displayHandler;
    public GameObject currentPartHandler;
    public bool editorMode;
    public Text titleText;
    public GameObject editorModeButtons;
    public GameObject editorModeAddPartSection;

    public static WorldData.CharacterData currentCharacter;
    public static ShipBuilder instance;
    public static bool heavyCheat = false;

    protected bool initialized;
    private int[] abilityLimits;
    private Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> traderPartDict;
    protected Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> partDict;
    private int editorCoreTier = 0;
    protected Transform[] contentsArray; // holds scroll view sub-sections by part size
    private Transform[] traderContentsArray;
    protected GameObject[] contentTexts;
    private GameObject[] traderContentTexts;
    // private PresetButton[] presetButtons;
    protected string searcherString;
    private bool[] displayingTypes;
    [SerializeField]
    private GameObject jsonButtonHolder;
    [SerializeField]
    private ShpBuilderSearch searchBar;
    [SerializeField]
    private GameObject clearConfirmerBox;

    public void RemoveKeyFromPartDict(EntityBlueprint.PartInfo info)
    {
        if (partDict.ContainsKey(info))
        {
            partDict.Remove(info);
        }
    }

    public BuilderMode GetMode()
    {
        return mode;
    }

    public GameObject partSelectContainer;
    public Transform[] partSelectTransforms;

    // supplemental parts in addition to existing parts in inventory
    public bool ContainsParts(List<EntityBlueprint.PartInfo> parts, List<EntityBlueprint.PartInfo> supplementalParts = null)
    {
        Dictionary<EntityBlueprint.PartInfo, int> counts = new Dictionary<EntityBlueprint.PartInfo, int>();
        // get the part counts
        foreach (EntityBlueprint.PartInfo info in partDict.Keys)
        {
            var p = CullSpatialValues(info);
            if (!counts.ContainsKey(p))
            {
                counts.Add(p, partDict[p].GetCount());
            }
        }

        if (supplementalParts != null)
        // get the part counts
        foreach (EntityBlueprint.PartInfo info in supplementalParts)
        {
            var p = CullSpatialValues(info);
            if (!counts.ContainsKey(p))
            {
                counts.Add(p, 1);
            }
            else counts[p]++;
        }


        foreach (ShipBuilderPart inf in cursorScript.parts)
        {
            var p = CullSpatialValues(inf.info);
            if (!counts.ContainsKey(p))
            {
                counts.Add(p, 1);
            }
            else
            {
                counts[p]++;
            }
        }

        foreach (EntityBlueprint.PartInfo part in parts)
        {
            var p = CullSpatialValues(part);
            if (!counts.ContainsKey(p))
            {
                return false;
            }
            else if (--counts[p] < 0)
            {
                return false;
            }
        }

        return true;
    }

    public void SwapDroneParts(List<EntityBlueprint.PartInfo> defaultParts, List<EntityBlueprint.PartInfo> existingParts, ShipBuilderInventoryScript button, DroneType type, bool increment = false)
    {
        foreach (var part in existingParts)
        {
            AddPart(CullSpatialValues(part));
        }

        foreach (var part in defaultParts)
        {
            if (!DecrementPartButton(part))
                throw new Exception("Part of default drone not present in inventory.");
        }

        var p = button.part;
        p.secondaryData = DroneUtilities.GetDefaultSecondaryDataByType(type);
        p.playerGivenName = "";
        if (increment)
        {
            button.IncrementCount();
            if (!partDict.ContainsKey(p))
                throw new Exception("Default drone part not present in inventory.");
            partDict[p].DecrementCount(true);
        }
        else 
        {
            button.DecrementCount(true);
            AddPart(p);
        }
        SavePartsToInventory();
    }


    public bool DecrementPartButton(EntityBlueprint.PartInfo info)
    {
        if (partDict.ContainsKey(CullSpatialValues(info)) && partDict[CullSpatialValues(info)].GetCount() > 0)
        {
            partDict[CullSpatialValues(info)].DecrementCount();
            return true;
        }
        else
        {
            return false;
        }
    }

    public static EntityBlueprint.PartInfo CullSpatialValues(EntityBlueprint.PartInfo partToCull)
    {
        var part = new EntityBlueprint.PartInfo();
        part.partID = partToCull.partID;
        part.abilityID = partToCull.abilityID;
        if (!CheckSecondaryDataPurge(partToCull))
        {
            part.secondaryData = partToCull.secondaryData;
        }

        part.tier = partToCull.tier;
        part.shiny = partToCull.shiny;
        part.playerGivenName = partToCull.playerGivenName;
        if (part.playerGivenName == null) part.playerGivenName = "";
        return part;
    }

    // Does calculation on whether the part's secondary data should be purged
    public static bool CheckSecondaryDataPurge(EntityBlueprint.PartInfo part)
    {
        if (part.abilityID == (int)AbilityID.SpawnDrone) return false;
        if (part.abilityID == (int)AbilityID.Missile && part.secondaryData == "missile_station_shooter")
            return false;
        if (part.abilityID == (int)AbilityID.Beam && part.secondaryData == "beamgroundshooter_sprite")
            return false;
        if (part.abilityID == (int)AbilityID.SiegeBullet
            && (part.secondaryData == "siegegroundshooter_sprite" || part.secondaryData == "siegeshooter_sprite"))
            return false;
        return true;
    }

    public enum TransferMode
    {
        Sell,
        Return,
        Buy
    }

    public void DispatchPart(ShipBuilderPart part, TransferMode transferMode, bool updateChain = true)
    {
        var culledInfo = CullSpatialValues(part.info);
        Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> dict;
        Transform[] dictContentsArray;
        GameObject[] dictContentTexts;
        switch (transferMode)
        {
            case TransferMode.Sell:
                cursorScript.buildCost -= EntityBlueprint.GetPartValue(part.info);
                dictContentsArray = traderContentsArray;
                dict = traderPartDict;
                dictContentTexts = traderContentTexts;
                break;
            case TransferMode.Buy:
                dictContentsArray = contentsArray;
                dict = partDict;
                dictContentTexts = contentTexts;
                break;
            default: // transfer back to original inventory
                if (part.mode == BuilderMode.Trader)
                {
                    cursorScript.buildCost -= EntityBlueprint.GetPartValue(part.info);
                }

                dict = (part.mode != BuilderMode.Trader ? partDict : traderPartDict);
                dictContentsArray = (part.mode != BuilderMode.Trader ? contentsArray : traderContentsArray);
                dictContentTexts = (part.mode != BuilderMode.Trader ? contentTexts : traderContentTexts);
                break;
        }

        if (!dict.ContainsKey(culledInfo))
        {
            int size = ResourceManager.GetAsset<PartBlueprint>(part.info.partID).size;
            ShipBuilderInventoryScript dictInvButton = Instantiate(buttonPrefab,
                dictContentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
            switch (transferMode)
            {
                // set the new button's transfer mode
                case TransferMode.Buy:
                    dictInvButton.mode = BuilderMode.Yard;
                    break;
                case TransferMode.Sell:
                    dictInvButton.mode = BuilderMode.Trader;
                    break;
                case TransferMode.Return:
                    dictInvButton.mode = part.mode;
                    break;
            }

            dict.Add(culledInfo, dictInvButton);
            dictInvButton.gameObject.SetActive(displayingTypes[(int)AbilityUtilities.GetAbilityTypeByID(part.info.abilityID)]);
            if (dictInvButton.gameObject.activeSelf) dictContentTexts[size].SetActive(dictInvButton.gameObject.activeSelf);
            dict[culledInfo].part = culledInfo;
            dict[culledInfo].cursor = cursorScript;
        }

        dict[culledInfo].IncrementCount(mode == BuilderMode.Workshop && !GetDroneWorkshopSelectPhase());
        cursorScript.buildValue -= EntityBlueprint.GetPartValue(part.info);
        cursorScript.parts.Remove(part);
        Destroy(part.gameObject);
        if (updateChain) UpdateChain();
    }

    private EntityBlueprint cachedPrint;
    private void CacheBlueprint()
    {
        try
        {
            cachedPrint = SectorManager.TryGettingEntityBlueprint(GetCurrentJSON());
        }
        catch(Exception e)
        {
            Debug.Log(e);
            cachedPrint = null;
        }
    }

    public EntityBlueprint GetBlueprint()
    {
        return cachedPrint;
    }

    public static Bounds GetRect(RectTransform rectTransform)
    {
        Bounds rect = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, rectTransform);
        rect.center = rectTransform.anchoredPosition;
        rect.size = rect.size * 1F;
        return rect;
    }

    public enum ReconstructButtonStatus
    {
        Valid,
        PartInvalidPosition,
        NotEnoughCredits,
        PastSkillsLimit,
        PastSpawnsLimit,
        PastWeaponsLimit,
        PastPassivesLimit,
        PartTooHeavy,
        DWTooManyParts,
    }

    public ReconstructButtonStatus reconstructStatus;

    private void SetReconstructButton(ReconstructButtonStatus status)
    {
        reconstructStatus = status;

        switch (status)
        {
            case ReconstructButtonStatus.Valid:
                reconstructText.color = Color.green;
                if (editorMode)
                {
                    reconstructText.text = "CONFIRM CHARACTER BLUEPRINT";
                }
                else if (mode == BuilderMode.Workshop)
                {
                    reconstructText.text = "SET PART DRONE";
                }
                else
                {
                    reconstructText.text = "RECONSTRUCT";
                }
                break;
            case ReconstructButtonStatus.PartInvalidPosition:
                reconstructText.color = Color.red;
                reconstructText.text = "A PART IS IN AN INVALID POSITION";
                break;
            case ReconstructButtonStatus.NotEnoughCredits:
                reconstructText.color = Color.red;
                reconstructText.text = "NOT ENOUGH CREDITS";
                break;
            case ReconstructButtonStatus.PastSkillsLimit:
                reconstructText.color = Color.red;
                reconstructText.text = "TOO MANY SKILLS";
                break;
            case ReconstructButtonStatus.PastSpawnsLimit:
                reconstructText.color = Color.red;
                reconstructText.text = "TOO MANY SPAWNS";
                break;
            case ReconstructButtonStatus.PastWeaponsLimit:
                reconstructText.color = Color.red;
                reconstructText.text = "TOO MANY WEAPONS";
                break;
            case ReconstructButtonStatus.PastPassivesLimit:
                reconstructText.color = Color.red;
                reconstructText.text = "TOO MANY PASSIVES";
                break;
            case ReconstructButtonStatus.PartTooHeavy:
                reconstructText.color = Color.red;
                reconstructText.text = "A PART IS TOO HEAVY FOR YOUR CORE";
                break;
            case ReconstructButtonStatus.DWTooManyParts:
                reconstructText.color = Color.red;
                reconstructText.text = "TOO MANY PARTS ON DRONE";
                break;
        }
    }

    static void UpdateChainHelper(ShipBuilderPart part, List<ShipBuilderPart> parts)
    {
        foreach (ShipBuilderPart shipPart in parts)
        {
            if (!shipPart.info.isInChain)
            {
                if (SATCollision.PartCollision(part, shipPart))
                {
                    shipPart.info.isInChain = true;
                    UpdateChainHelper(shipPart, parts);
                }
            }
        }
    }

    [SerializeField]
    private InputField nameInputField;
    [SerializeField]
    private GameObject nameBox;
    private ShipBuilderInventoryScript nameCandidate;

    public void OpenNameWindow(ShipBuilderInventoryScript info) 
    {
        nameCandidate = info;
        nameBox.SetActive(true);
        nameInputField.text = "";
    }

    public void CloseNameWindow(bool confirm)
    {
        nameBox.SetActive(false);
        if (!confirm)
        {
            return;
        }

        EntityBlueprint.PartInfo info = nameCandidate.part;
        if (string.IsNullOrEmpty(nameInputField.text) || partDict.Keys.ToList().Exists(p => p.playerGivenName == nameInputField.text))
        {
            nameBox.SetActive(true);
            return;
        } 

        info.playerGivenName = nameInputField.text;
        AddPart(info);
        nameCandidate.DecrementCount(true);
        SavePartsToInventory();
    }



    Vector2 GetImageBoundsBySprite(Image img)
    {
        return img.sprite.bounds.size * 100;
    }

    public static bool CheckPartIntersectsWithBound(ShipBuilderPart part1, ShipBuilderPart part2)
    {
        var pb1 = new Bounds(part1.info.location * 100, part1.image.sprite.bounds.extents * 100); 
        var pb2 = new Bounds(part2.info.location * 100, part2.image.sprite.bounds.extents * 100);
        return (pb1.Intersects(pb2));
    }

    public static bool CheckPartIntersectsWithShell(ShipBuilderPart shipPart, BuilderMode mode)
    {
        return CheckPartIntersectsWithShell(ref shipPart.info, mode, shipPart.image.sprite);
    }


    /// <summary>
    /// Check if the part is close enough and not too close to the shell and change isInChain accordinly. 
    /// Return true if too close to the shell.
    /// </summary>
    /// <param name="shipPart"></param>
    /// <returns>is the part too close</returns>
    public static bool CheckPartIntersectsWithShell(ref EntityBlueprint.PartInfo shipPart, BuilderMode mode = BuilderMode.Yard, Sprite partSprite = null)
    {
        var shellRect = new Bounds(Vector3.zero, ResourceManager.GetAsset<Sprite>("core1_shell").bounds.size * 100);

        Vector2[] partPoints = SATCollision.GetPartVertices(shipPart);

        Vector2 center = shellRect.center;
        Vector2 up = new Vector2(0f, shellRect.extents.y);
        Vector2 right = new Vector2(shellRect.extents.x, 0f);
        Vector2[] shellPoints = new Vector2[]
        {
            -right - up + center,
             right - up + center,
             right + up + center,
            -right + up + center,
        };

        // Check if too close
        if (SATCollision.RectangleCollision(partPoints, shellPoints))
        {
            if (mode == BuilderMode.Workshop)
            {
                shipPart.isInChain = true;
                return true;
            }
            float shrinkFactor = -1.389f;
            partPoints = SATCollision.GetPartVertices(shipPart, partSprite, shrinkFactor);

            up *= 0.36f;
            right *= 0.36f;
            shellPoints = new Vector2[]
            {
                -right - up + center,
                 right - up + center,
                 right + up + center,
                -right + up + center,
            };

            // Too close -> not in chain
            shipPart.isInChain = !SATCollision.RectangleCollision(partPoints, shellPoints);

            // Return true = too close
            return !shipPart.isInChain;
        }
        // No collision
        return false;
    }

    public static bool ValidateBlueprint(EntityBlueprint print, bool editorMode, string coreShellSpriteID, bool checkPartSizes = false, int[] abilityLimits = null)
    {
        if (string.IsNullOrEmpty(MainMenu.RDB_SERVER_PASSWORD) && MasterNetworkAdapter.mode != MasterNetworkAdapter.NetworkMode.Off) return true;
        var outcome = true;
        if (!print) return false;
        if (print.parts == null) return false;
        if (print.coreSpriteID != "core1_light") return false;
        if (!CoreUpgraderScript.GetCoreNames().ToList().Contains(coreShellSpriteID)) return false;
        List<ShipBuilderPart> parts = new List<ShipBuilderPart>();
        
        foreach (var part in print.parts)
        {
            ShipBuilder.LoadPart(part, null, false, parts);
        }

        ShipBuilder.ResetNeighbors(parts);
        if (!ShipBuilder.ValidateParts(parts)) 
        {
            outcome = false;
        }

        outcome &= (!checkPartSizes || CheckPartSizes(parts, coreShellSpriteID, editorMode));
        outcome &= (abilityLimits == null || CheckAbilityCaps(parts, abilityLimits, coreShellSpriteID, editorMode) == 0);
        parts.ForEach(p => Destroy(p.gameObject));
        return outcome;
    }

    public static bool ValidateParts(List<ShipBuilderPart> parts, BuilderMode mode = BuilderMode.Yard)
    {
        // Clear previous chain status & connect the parts intersecting the shell
        foreach (ShipBuilderPart shipPart in parts)
        {
            shipPart.info.isInChain = false;
            CheckPartIntersectsWithShell(shipPart, mode);
        }

        foreach (ShipBuilderPart shipPart in parts)
        {
            if (shipPart.info.isInChain)
            {
                UpdateChainHelper(shipPart, parts);
            }
        }

        foreach (ShipBuilderPart shipPart in parts)
        {
            CheckPartIntersectsWithShell(shipPart, mode);
        }

        foreach (ShipBuilderPart part in parts)
        {
            if (part.info.validPos)
            {
                foreach (var part2 in part.neighbors)
                {
                    if (!part2)
                    {
                        continue;
                    }
                    if (part != part2 && PartIsTooClose(part, part2, mode))
                    {
                        part.info.validPos = false;
                        break;
                    }
                }
            }
            else
            {
                bool stillTouching = false;
                foreach (ShipBuilderPart part2 in part.neighbors)
                {
                    if (!part2)
                    {
                        continue;
                    }

                    if (part2 != part && PartIsTooClose(part, part2, mode))
                    {
                        stillTouching = true;
                        break;
                    }
                }

                if (!stillTouching)
                {
                    part.info.validPos = true;
                }
            }
        }

        foreach (ShipBuilderPart shipPart in parts)
        {
            if (!shipPart.info.isInChain || !shipPart.info.validPos)
            {
                return false;
            }
        }
        return true;
    }

    public void UpdateChain()
    {
        CacheBlueprint();
        if (!editorMode)
        {
            SetReconstructButton(cursorScript.buildCost > player.GetCredits() ? ReconstructButtonStatus.NotEnoughCredits : ReconstructButtonStatus.Valid);
        }
        else
        {
            SetReconstructButton(ReconstructButtonStatus.Valid);
        }

        if (!ValidateParts(cursorScript.parts, mode))
        {
            SetReconstructButton(ReconstructButtonStatus.PartInvalidPosition);
            return;
        }

        var shell = PlayerCore.Instance ? PlayerCore.Instance.blueprint.coreShellSpriteID : "core1_shell";
        if (!CheckPartSizes(cursorScript.parts, shell, editorMode))
        {
            SetReconstructButton(ReconstructButtonStatus.PartTooHeavy);
            return;
        }
        if (mode == BuilderMode.Workshop && droneSpawnData && DroneUtilities.GetPartLimit(droneSpawnData.type) < cursorScript.parts.Count())
        {
            SetReconstructButton(ReconstructButtonStatus.DWTooManyParts);
            return;
        }
        var ability = CheckAbilityCaps(cursorScript.parts, abilityLimits, shell, editorMode);
        if (ability != 0)
        {
            SetReconstructButton((ReconstructButtonStatus)((int)ReconstructButtonStatus.PastSkillsLimit+ability-1));
        }
    }

    static bool PartIsTooClose(ShipBuilderPart part, ShipBuilderPart otherPart, BuilderMode mode)
    {
        if (mode == BuilderMode.Workshop) 
        {
            return false;
        }
        var closeConstant = mode == BuilderMode.Workshop ? -1.2F : -1F;
        // add small number (0.005) to deal with floating point issues
        closeConstant -= 0.005f;
        //var rect1 = ShipBuilder.GetRect(part.rectTransform);
        //var rect2 = ShipBuilder.GetRect(otherPart.rectTransform);
        //rect1.Expand(closeConstant * rect1.extents);
        //rect2.Expand(closeConstant * rect2.extents);
        //return rect1.Intersects(rect2);

        return SATCollision.TooCloseCollision(part, otherPart, closeConstant);
    }

    public static bool CheckPartSizes(List<ShipBuilderPart> parts, string coreShellSpriteID, bool editorMode)
    {
        int maxTier = !editorMode && !heavyCheat ? CoreUpgraderScript.GetPartTierLimit(coreShellSpriteID) : 3;
        foreach (ShipBuilderPart shipPart in parts)
        {
            if (ResourceManager.GetAsset<PartBlueprint>(shipPart.info.partID).size > maxTier)
            {
                return false;
            }
        }

        return true;
    }

    public static int CheckAbilityCaps(List<ShipBuilderPart> parts, int[] abilityLimits, string coreShellSpriteID, bool editorMode)
    {
        if (editorMode)
        {
            return 0;
        }

        var currentAbilitynumbers = new int[] { 0, 0, 0, 0, 0 };

        foreach (ShipBuilderPart shipBuilderPart in parts)
        {
            var type = (int)AbilityUtilities.GetAbilityTypeByID(shipBuilderPart.info.abilityID);
            currentAbilitynumbers[type]++;
        }

        var extras = CoreUpgraderScript.GetExtraAbilities(coreShellSpriteID);

        for (int i = 0; i < 4; i++)
        {
            if (currentAbilitynumbers[i] > abilityLimits[i] + extras[i])
            {
                return i+1;
            }
        }

        return 0;
    }

    static Dictionary<string, List<(string, int)>> originsofParts = new Dictionary<string, List<(string, int)>>();

    public static void AddOriginToDictionary(ShellPart part)
    {
        if (!originsofParts.ContainsKey(part.droppedSectorName))
        {
            var list = new List<(string, int)>();
            list.Add((part.info.partID, part.info.abilityID));
            originsofParts.Add(part.droppedSectorName, list);
        }
        else
        {
            originsofParts[part.droppedSectorName].Add((part.info.partID, part.info.abilityID));
        }
    }

    public static bool CheckForOrigin(string sectorName, (string, int) tuple)
    {
        if (originsofParts.ContainsKey(sectorName))
        {
            return originsofParts[sectorName].Contains(tuple);
        }
        else
        {
            return false;
        }
    }

    public static void RemoveOrigin(string sectorName, (string, int) tuple)
    {
        if (originsofParts.ContainsKey(sectorName))
        {
            if (originsofParts[sectorName].Contains(tuple))
            {
                originsofParts[sectorName].Remove(tuple);
            }
        }
    }

    [SerializeField]
    private GameObject droneWorkshopPhaseHider;

    private EntityBlueprint.PartInfo dronePart;
    private DroneSpawnData droneSpawnData;
    private int dronePartCount;
    [SerializeField]
    private ShipBuilderSortingButton[] sortingButtons;
    [SerializeField]
    private GameObject sortingObject;

    public int GetDronePartCount()
    {
        return dronePartCount;
    }

    public void InitializeDronePart(EntityBlueprint.PartInfo info)
    {
        CloseNameWindow(false);
        dronePartCount = partDict[info].GetCount();
        dronePart = info;
        droneSpawnData = DroneUtilities.GetDroneSpawnDataByShorthand(dronePart.secondaryData);
        droneWorkshopPhaseHider.SetActive(false);
        cursorScript.ClearAllParts();
        searcherString = "";
        if (sortingObject)
        {
            sortingObject.SetActive(true);
            
            for (int i = 0; i < displayingTypes.Length; i++)
            {
                displayingTypes[i] = true;
            }
            
            foreach (var sortingButton in sortingButtons) 
            {
                sortingButton.DroneWorkshopModifications();
            }

            searchBar.input.text = "";
        }

        var parts = new List<EntityBlueprint.PartInfo>();
        SetUpInventory(parts);
        foreach(var part in parts)
        {
            AddPart(part);
        }
        LoadBlueprint(DroneUtilities.GetDroneSpawnDataByShorthand(info.secondaryData).drone);
        foreach (var part in cursorScript.parts)
        {
            part.mode = BuilderMode.Workshop;
        }
    }

    private string blueprintCoreShellSpriteId;

    public void Initialize(BuilderMode mode, List<EntityBlueprint.PartInfo> traderInventory = null, EntityBlueprint blueprint = null)
    {
        SetSelectPartActive(false);
        // set editor mode if testing
        if (SectorManager.testJsonPath != null && !Input.GetKey(KeyCode.LeftShift) && mode == BuilderMode.Yard)
        {
            editorMode = true;
        }
        else if (SceneManager.GetActiveScene().name != "WorldCreator")
        {
            editorMode = false;
        }

        // initialize window on screen
        if (initialized)
        {
            CloseUI(false); // prevent initializing twice by closing UI if already initialized
        }

        droneSpawnData = null;
        initialized = true;
        instance = this;
        dronePartCount = 0;
        Activate();
        cursorScript.gameObject.SetActive(false);
        cursorScript.SetBuilder(this, droneWorkshopPhaseHider);

        GetComponentInChildren<ShipBuilderPartDisplay>().Initialize(this);

        // set up actual stats
        this.mode = mode;
        cursorScript.SetMode(mode);
        searcherString = "";
        contentsArray = new Transform[] { smallContents, mediumContents, largeContents };
        traderContentsArray = new Transform[] { traderSmallContents, traderMediumContents, traderLargeContents };
        contentTexts = new GameObject[] { smallText, mediumText, largeText };
        traderContentTexts = new GameObject[] { traderSmallText, traderMediumText, traderLargeText };
        foreach (GameObject obj in contentTexts)
        {
            obj.SetActive(false);
        }

        foreach (GameObject traderObj in traderContentTexts)
        {
            traderObj.SetActive(false);
        }

        displayingTypes = new bool[] { true, true, true, true, true };
        if (player)
        {
            player.SetIsInteracting(true);
        }

        partDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();
        traderPartDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();

        // set shell sprite and color
        if (!editorMode && !blueprint)
        {
            blueprint = player.blueprint;
        }

        if (!editorMode)
        {
            shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
            core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        }
        else
        {
            shell.sprite = ResourceManager.GetAsset<Sprite>(GetEditorShellString());
            core.sprite = ResourceManager.GetAsset<Sprite>(GetEditorCoreString());
        }

        shell.color = FactionManager.GetFactionColor(0);
        shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;

        // orient shell image so relative center stays the same regardless of shell tier
        OrientShellAndCore();

        core.material = ResourceManager.GetAsset<Material>("material_color_swap");
        core.color = FactionManager.GetFactionColor(0);
        core.preserveAspect = true;
        core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;
        List<EntityBlueprint.PartInfo> parts = new List<EntityBlueprint.PartInfo>();

        droneWorkshopPhaseHider.SetActive(mode == BuilderMode.Workshop);
        sortingObject.SetActive(mode != BuilderMode.Workshop);
        if (!editorMode)
        {
            SetUpInventory(parts);
            cursorScript.player = player;
            cursorScript.handler = player.GetAbilityHandler();

            abilityLimits = player.abilityCaps;
        }

        if (mode == BuilderMode.Trader)
        {
            var traderInv = new List<EntityBlueprint.PartInfo>();
            foreach (var part in traderInventory)
            {
                traderInv.Add(CullSpatialValues(part));

                // Player has now seen all the parts in the trader's inventory.
                PartIndexScript.AttemptAddToPartsSeen(PartIndexScript.CullToPartIndexValues(part));
            }

            traderInventory = traderInv;
        }

        // hide the buttons and yard tips if interacting with a trader

        tips.gameObject.SetActive(mode == BuilderMode.Yard);
        traderScrollView.gameObject.SetActive(mode == BuilderMode.Trader);

        /*
        if(traderInventory.Count == 0) {
            EntityBlueprint.PartInfo info = new EntityBlueprint.PartInfo();
            foreach(string name in ResourceManager.allPartNames) {
                for(int i = 0; i < 3; i++) 
                {
                    info.partID = name;
                    info.abilityID = Random.Range(0,21);
                    if((info.abilityID >= 14 && info.abilityID <= 16) || info.abilityID == 3) info.abilityID = 0;
                    if(info.abilityID == 10) {
                        DroneSpawnData data = DroneUtilities.GetDefaultData((DroneType)Random.Range(0, 8));
                        info.secondaryData = JsonUtility.ToJson(data);
                    }
                    if(info.abilityID == 0 || info.abilityID == 10) info.tier = 0;
                    else info.tier = Random.Range(1, 4);
                    traderInventory.Add(info);
                }
            }
        }
        */

        if (traderInventory != null)
        {
            foreach (EntityBlueprint.PartInfo info in traderInventory)
            {
                // TODO: cull values to prevent possible problems
                if (!traderPartDict.ContainsKey(info))
                {
                    int size = ResourceManager.GetAsset<PartBlueprint>(info.partID).size;
                    ShipBuilderInventoryScript traderInvButton = Instantiate(buttonPrefab, // instantiate part button for trader
                        traderContentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
                    traderContentTexts[size].SetActive(true);
                    traderInvButton.part = info;
                    traderInvButton.cursor = cursorScript;
                    traderInvButton.mode = BuilderMode.Trader;
                    traderPartDict.Add(info, traderInvButton);
                    for (int i = 0; i < 500; i++)
                    {
                        traderInvButton.IncrementCount();
                    }
                }
            }
        }

        var partsToAdd = new List<ShellPart>();
        if (!editorMode)
        {
            var initialShards = player.cursave.shards;

            foreach (Entity ent in player.GetUnitsCommanding())
            {
                if (!(ent as Drone))
                {
                    continue;
                }

                var beam = ent.GetComponentInChildren<TractorBeam>();
                if (beam)
                {
                    var target = beam.GetTractorTarget();
                    if (target && target.GetComponent<ShellPart>())
                    {
                        partsToAdd.Add(target.GetComponent<ShellPart>());
                        AddOriginToDictionary(target.GetComponent<ShellPart>());
                    }
                    else if (target && target.GetComponent<Shard>())
                    {
                        AddShard(target.GetComponent<Shard>());
                        Destroy(target.gameObject);
                    }
                }
            }

            var playerTarget = player.GetTractorTarget();
            if (playerTarget && playerTarget.GetComponent<ShellPart>())
            {
                partsToAdd.Add(playerTarget.GetComponent<ShellPart>());
                AddOriginToDictionary(playerTarget.GetComponent<ShellPart>());
            }
            else if (playerTarget && playerTarget.GetComponent<Shard>())
            {
                AddShard(playerTarget.GetComponent<Shard>());
                Destroy(playerTarget.gameObject);
            }

            if (initialShards != player.cursave.shards)
            {
                ShardCountScript.DisplayCount();
            }

            foreach (ShellPart part in partsToAdd)
            {
                var info = part.info;
                info = ShipBuilder.CullSpatialValues(info);
                AddPart(info);
                player.cursave.partInventory.Add(info);

                PartIndexScript.AttemptAddToPartsObtained(info);
                PartIndexScript.AttemptAddToPartsSeen(info);
                Destroy(part.gameObject);
            }

            if (mode != BuilderMode.Workshop)
                LoadBlueprint(blueprint);
        }

        // activate windows
        gameObject.SetActive(true);

        /*
        if (SceneManager.GetActiveScene().name == "SampleScene" && (presetButtons == null || presetButtons.Length == 0))
        {
            presetButtons = GetComponentsInChildren<PresetButton>();
            foreach (PresetButton button in presetButtons)
            {
                button.SBPrefab = ResourceManager.GetAsset<GameObject>("ship_builder_part_prefab");
                button.player = player;
                button.cursorScript = cursorScript;
                button.builder = this;
                button.displayHandler = displayHandler;
                button.currentPartHandler = currentPartHandler;
                button.Initialize();
            }
        }
        */

        if (!editorMode)
        {
            if (editorModeButtons)
            {
                editorModeButtons.SetActive(false);
            }

            if (jsonButtonHolder)
            {
                jsonButtonHolder.SetActive(mode == BuilderMode.Yard);
            }
        }
        else
        {
            if (jsonButtonHolder)
            {
                jsonButtonHolder.SetActive(true);
            }

            if (editorModeButtons)
            {
                editorModeButtons.SetActive(true);
            }
        }

        // set title text
        switch (mode)
        {
            case BuilderMode.Yard:
                if (!editorMode)
                {
                    titleText.text = "SHIP BUILDER - YARD";
                }
                else
                {
                    titleText.text = "SHIP BUILDER - SUPER DUPER EDITOR MODE";
                }

                break;
            case BuilderMode.Trader:
                titleText.text = "SHIP BUILDER - TRADER";
                break;
            case BuilderMode.Workshop:
                titleText.text = "SHIP BUILDER - DRONE WORKSHOP";
                break;
            default:
                break;
        }

        var dropdown = editorModeAddPartSection.transform.Find("Ability ID").GetComponent<Dropdown>();
        if (editorMode && dropdown.options.Count == 0)
        {
            foreach (var id in System.Enum.GetValues(typeof(AbilityID)))
            {
                dropdown.options.Add(new Dropdown.OptionData
                    (id.ToString()));
            }
        }

        cursorScript.gameObject.SetActive(true);
        ResetNeighbors(cursorScript.parts);
        cursorScript.UpdateHandler();
        UpdateChain();
    }

    public static void ResetNeighbors(List<ShipBuilderPart> parts)
    {
        foreach (var part1 in parts)
        {
            part1.neighbors = new List<ShipBuilderPart>();
            foreach (var part2 in parts)
            {
                if (part1 == part2)
                {
                    continue;
                }
                if (CheckPartIntersectsWithBound(part1, part2))
                {
                    part1.neighbors.Add(part2);
                }
            }
        }
    }

    public bool GetDroneWorkshopSelectPhase()
    {
        return droneWorkshopPhaseHider.activeSelf;
    }

    private void SetUpInventory(List<EntityBlueprint.PartInfo> parts)
    {
        foreach (ShipBuilderInventoryScript button in partDict.Values)
        {
            Destroy(button.gameObject);
        }

        partDict.Clear();
        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            Destroy(part.gameObject);
        }

        cursorScript.parts.Clear();
        parts = player.GetInventory();

        if (parts != null)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i] = CullSpatialValues(parts[i]);
            }
        }

        foreach (EntityBlueprint.PartInfo part in parts)
        {
            AddPart(part);
        }
    }

    private void OrientShellAndCore()
    {
        shell.rectTransform.anchoredPosition = -shell.sprite.pivot + shell.rectTransform.sizeDelta / 2;
        core.rectTransform.anchoredPosition = -shell.rectTransform.anchoredPosition;
    }

    public void AddShard(Shard shard)
    {
        AddShard(shard.tier);
    }

    public void AddShard(int tier)
    {
        var tiers = new int[] { 1, 5, 20 };
        player.cursave.shards += tiers[tier];
    }

    private void AddPart(EntityBlueprint.PartInfo part)
    {
        if (!part.Equals(CullSpatialValues(part)))
        {
            Debug.LogWarning("part was not passed as culled");
            part = CullSpatialValues(part);
        }
        if (!partDict.ContainsKey(part))
        {
            int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
            ShipBuilderInventoryScript invButton = Instantiate(buttonPrefab,
                contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
            partDict.Add(part, invButton);
            invButton.gameObject.SetActive(displayingTypes[(int)AbilityUtilities.GetAbilityTypeByID(part.abilityID)]);
            if (invButton.gameObject.activeSelf) contentTexts[size].SetActive(true);
            invButton.part = part;
            invButton.cursor = cursorScript;
            invButton.IncrementCount();
            if (mode != BuilderMode.Workshop)
            {
                invButton.mode = BuilderMode.Yard;
            }
            else
            {
                invButton.mode = BuilderMode.Workshop;
            }
        }
        else
        {
            partDict[part].IncrementCount();
        }

        if (editorMode)
        {
            for (int i = 0; i < 99; i++)
            {
                partDict[part].IncrementCount();
            }
        }
    }

    public void AddPartByEditorSection()
    {
        var part = new EntityBlueprint.PartInfo();

        if (!int.TryParse(editorModeAddPartSection.transform.Find("Ability Tier").GetComponent<InputField>().text, out part.tier))
        {
            part.tier = 0;
        }

        var secondaryData = editorModeAddPartSection.transform.Find("Secondary Data").GetComponent<InputField>().text;
        part.secondaryData = secondaryData != null ? secondaryData : "";
        part.abilityID = editorModeAddPartSection.transform.Find("Ability ID").GetComponent<Dropdown>().value;
        part.shiny = editorModeAddPartSection.transform.Find("Is Shiny").GetComponent<Toggle>().isOn;
        var x = editorModeAddPartSection.transform.Find("Part ID").GetComponent<InputField>().text;
        if (ResourceManager.allPartNames.Contains(x))
        {
            part.partID = editorModeAddPartSection.transform.Find("Part ID").GetComponent<InputField>().text;
            AddPart(CullSpatialValues(part));
        }
    }

    public void ChangePartImage(string id)
    {
        if (ResourceManager.allPartNames.Contains(id))
        {
            var im = editorModeAddPartSection.transform.Find("Part Image").GetComponent<Image>();
            im.sprite = ResourceManager.GetAsset<Sprite>(id + "_sprite");
            im.color = PlayerCore.GetPlayerFactionColor();
            im.rectTransform.sizeDelta = im.sprite.bounds.size * 50;
        }
    }

    public override void CloseUI()
    {
        CloseUI(false);
    }

    public void CloseUI(bool validClose)
    {
        var jsonWindow = GetComponentInChildren<ShipBuilderJSONButton>()?.window;
        if (jsonWindow)
        {
            jsonWindow.SetActive(false);
        }

        if (nameBox && nameBox.activeSelf)
        {
            CloseNameWindow(false);
        }

        if (editorMode)
        {
            SetSelectPartActive(false);
        }

        if (!validClose)
        {
            AudioManager.PlayClipByID("clip_back");
        }
        else
        {
            AudioManager.PlayClipByID("clip_select");
        }

        initialized = false;
        if (player)
        {
            player.SetIsInteracting(false);
        }

        gameObject.SetActive(false);
        if (!editorMode && validClose)
        {
            SavePartsToInventory();

            foreach (EntityBlueprint.PartInfo info2 in player.blueprint.parts)
            {
                PartIndexScript.AttemptAddToPartsObtained(info2);
            }
        }

        foreach (ShipBuilderInventoryScript button in partDict.Values)
        {
            Destroy(button.gameObject);
        }

        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            Destroy(part.gameObject);
        }

        foreach (ShipBuilderInventoryScript traderButton in traderPartDict.Values)
        {
            Destroy(traderButton.gameObject);
        }

        cursorScript.parts = new List<ShipBuilderPart>();
        if (((!editorMode || player) && !validClose) || mode == BuilderMode.Workshop)
        {
            AbilityHandler handler = player.GetAbilityHandler(); // reset handler to correct representation
            handler.Deinitialize();
            handler.Initialize(player);
        }
    }

    public void TriggerReconstruct()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            
        }
    }

    private void SavePartsToInventory()
    {
        if (mode == BuilderMode.Workshop && !GetDroneWorkshopSelectPhase())
        {
            var blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
            blueprint.parts = new List<EntityBlueprint.PartInfo>();
            blueprint.coreShellSpriteID = blueprintCoreShellSpriteId;
            blueprint.coreSpriteID = "drone_light";
            blueprint.entityName = DroneUtilities.GetAbilityNameByType(droneSpawnData.type);

            foreach (ShipBuilderPart part in cursorScript.parts)
            {
                blueprint.parts.Add(part.info);
            }
            var button = partDict[dronePart];
            var oldData = Instantiate(DroneUtilities.GetDroneSpawnDataByShorthand(button.part.secondaryData));
            oldData.drone = JsonUtility.ToJson(blueprint);
            button.part.secondaryData = JsonUtility.ToJson(oldData);
            partDict.Remove(dronePart);
            if (partDict.ContainsKey(button.part)) throw new Exception("Part dict contained the modified drone. Since different parts cannot have the same names this shouldn't be possible.");
            else partDict.Add(button.part, button);
        }

        // try adding parts in the player's inventory and on their ship into the part index obtained list.
        player.cursave.partInventory = new List<EntityBlueprint.PartInfo>();
        foreach (EntityBlueprint.PartInfo info in partDict.Keys)
        {
            if (partDict[info].GetCount() > 0)
            {
                for (int i = 0; i < partDict[info].GetCount(); i++)
                {
                    player.cursave.partInventory.Add(info);
                }
                PartIndexScript.AttemptAddToPartsObtained(info);
            }
        }      
    }


    public static void LoadPart(EntityBlueprint.PartInfo part, ShipBuilderCursorScript cursorScript, bool editorMode, List<ShipBuilderPart> partsList = null)
    {
        var p = Instantiate(ResourceManager.GetAsset<GameObject>("ship_builder_part_prefab"), cursorScript ? cursorScript.transform.parent : null).GetComponent<ShipBuilderPart>();
        p.info = part;
        p.info.isInChain = true;
        p.info.validPos = true;
        if (cursorScript)
        {
            p.cursorScript = cursorScript;
            cursorScript.parts.Add(p);
        }
        if (partsList != null)
        {
            partsList.Add(p);
        }
        p.SetLastValidPos(part.location);
        if (editorMode)
            cursorScript.buildValue += EntityBlueprint.GetPartValue(part);
        p.Initialize();
    }

    public void LoadBlueprint(EntityBlueprint blueprint)
    {

        if (editorMode)
        {
            cursorScript.ClearAllParts();
            cursorScript.buildValue = 0;
        }

        foreach (EntityBlueprint.PartInfo part in blueprint.parts)
        {
            LoadPart(part, cursorScript, editorMode);
        }

        if (editorMode)
        {
            for (int i = 0; i < CoreUpgraderScript.GetCoreNames().Length; i++)
            {
                if (CoreUpgraderScript.GetCoreNames()[i] == blueprint.coreShellSpriteID)
                {
                    editorCoreTier = i;
                }
            }
        }

        core.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        editorCoreTier = Mathf.Max(GetEditorCoreList().FindIndex(x => x == blueprint.coreShellSpriteID), 0);
        if (mode == BuilderMode.Workshop)
            blueprintCoreShellSpriteId = blueprint.coreShellSpriteID;
        shell.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        shell.color = FactionManager.GetFactionColor(0);
        shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;
        core.enabled = core.sprite;
        if (core && core.sprite) core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;

        OrientShellAndCore();
        CacheBlueprint();
    }

    public static void SaveBlueprint(EntityBlueprint blueprint = null, string fileName = null, string json = null)
    {
        if (fileName != null)
        {
            System.IO.File.WriteAllText(fileName, json);
        }
#if UNITY_EDITOR
        else
        {
            //AssetDatabase.CreateAsset(blueprint, System.IO.Path.Combine("Assets", "SavedPrint.asset"));
        }
#endif
    }

    private List<ShipBuilderInventoryNameSelect> nameSelectObjs = new List<ShipBuilderInventoryNameSelect>();

    public void SetSelectPartActive(bool active)
    {
        if (partSelectContainer.activeSelf && active)
        {
            return;
        }

        partSelectContainer.SetActive(active);

        if (active)
        {
            nameSelectObjs.Clear();
            foreach (var name in ResourceManager.allPartNames)
            {
                var size = ResourceManager.GetAsset<PartBlueprint>(name).size;
                GameObject invButton = Instantiate(buttonPrefab,
                    partSelectTransforms[size]);

                // remove the inventory button and add a name select button. Carry over the refs in the inventory button
                var oldComp = invButton.GetComponent<ShipBuilderInventoryScript>();
                var shiny = oldComp.isShiny;
                Destroy(oldComp);
                var comp = invButton.AddComponent<ShipBuilderInventoryNameSelect>();
                comp.part = new EntityBlueprint.PartInfo();
                comp.part.partID = name;
                comp.isShiny = shiny;
                comp.builder = this;
                comp.field = editorModeAddPartSection.transform.Find("Part ID").GetComponent<InputField>();
                nameSelectObjs.Add(comp);
            }
        }
        else
        {
            foreach (var obj in nameSelectObjs)
            {
                Destroy(obj.gameObject);
            }

            nameSelectObjs.Clear();
        }
    }

    public void LoadBlueprint(string json)
    {
        EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(json, blueprint);
        LoadBlueprint(blueprint);
    }

#if UNITY_EDITOR
    public void SavePrintWithPrompt()
    {
        var path = UnityEditor.EditorUtility.SaveFilePanel("Save Blueprint", System.IO.Path.Combine(Application.streamingAssetsPath, "Entities"),
            "DefaultPrint", "json");
        SaveBlueprint(null, path, GetCurrentJSON());
    }

    public void LoadPrintWithPrompt()
    {
        var path = UnityEditor.EditorUtility.OpenFilePanel("Load Blueprint", System.IO.Path.Combine(Application.streamingAssetsPath, "Entities"), "json");
        LoadBlueprint(System.IO.File.ReadAllText(path));
    }

#endif
    public InputField inField;

    public void SetBlueprint()
    {
        if (inField.text == "")
        {
            return;
        }

        var blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(inField.text, blueprint);
        if (!blueprint)
        {
            return;
        }

        CloseUI(false);
        inField.text = "";
        Initialize(BuilderMode.Yard, null, blueprint);
#if UNITY_EDITOR
        SaveBlueprint(blueprint); // creates an asset of that blueprint for later use
#endif
    }

    public void Deinitialize()
    {
        if (!editorMode && cursorScript.buildCost > player.GetCredits())
        {
            return;
        }

        if (PlayerCore.Instance)
        {
            if (CheckAbilityCaps(cursorScript.parts, abilityLimits, PlayerCore.Instance.blueprint.coreShellSpriteID, editorMode) != 0)
            {
                return;
            }

            if (!CheckPartSizes(cursorScript.parts, PlayerCore.Instance.blueprint.coreShellSpriteID, editorMode))
            {
                return;
            }

            if (droneSpawnData && cursorScript.parts.Count() > DroneUtilities.GetPartLimit(droneSpawnData.type)) return;
        }
        

        bool invalidState = false;
        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            if (!part.info.validPos || !part.info.isInChain)
            {
                invalidState = true;
                break;
            }
        }

        if (!invalidState)
        {
            Export();
            CloseUI(true);
        }
    }

    public void Export()
    {
        if (mode == BuilderMode.Workshop) return;

        if (player)
        {
            player.AddCredits(-cursorScript.buildCost);
            player.blueprint.parts = new List<EntityBlueprint.PartInfo>();
            foreach (ShipBuilderPart part in cursorScript.parts)
            {
                player.blueprint.parts.Add(part.info);
            }

            player.Rebuild();
        }

#if UNITY_EDITOR
        if (editorMode && currentCharacter != null)
        {
            currentCharacter.blueprintJSON = GetCurrentJSON();
            WorldCreatorCursor.instance.characterHandler.reflectData();

            // null character so another change doesn't accidentally happen
            currentCharacter = null;
        }

#endif

        NodeEditorFramework.Standard.UsePartCondition.OnPlayerReconstruct.Invoke();
    }

    public void RequestClearParts()
    {
        if (clearConfirmerBox.activeSelf)
        {
            clearConfirmerBox.SetActive(false);
            cursorScript.ClearAllParts();
        }
        else
        {
            clearConfirmerBox.SetActive(true);
        }
    }


    protected override void Update()
    {
        base.Update();
        if (!editorMode)
        {
            if ((player.transform.position - yardPosition).sqrMagnitude > 200 || player.GetIsDead())
            {
                CloseUI(false);
            }
        }

        if (editorMode && Input.GetKeyDown(KeyCode.H))
        {
            TraderInventory trader = new TraderInventory();
            trader.parts = new List<EntityBlueprint.PartInfo>();
            foreach (var part in partDict.Keys)
            {
                trader.parts.Add(CullSpatialValues(part));
            }

            Debug.Log(JsonUtility.ToJson(trader));
        }

        if (editorMode && Input.GetKeyDown(KeyCode.X))
        {
            foreach (var input in GetComponentsInChildren<InputField>())
            {
                if (input.isFocused)
                {
                    return;
                }
            }

            var cores = new List<string>(CoreUpgraderScript.GetCoreNames());
            cores.Add("semicore_shell");
            cores.Add("groundcarriershell");
            cores.Add("drone_shell");

            editorCoreTier++;
            editorCoreTier %= cores.Count;
            shell.sprite = ResourceManager.GetAsset<Sprite>(cores[editorCoreTier]);
            if (editorCoreTier == cores.Count - 3)
            {
                core.sprite = ResourceManager.GetAsset<Sprite>("semicore_light");
            }
            else if (editorCoreTier == cores.Count - 2)
            {
                core.sprite = ResourceManager.GetAsset<Sprite>("groundcarriercore");
            }
            else if (editorCoreTier == cores.Count - 1)
            {
                core.sprite = ResourceManager.GetAsset<Sprite>("drone_light");
            }
            else
            {
                core.sprite = ResourceManager.GetAsset<Sprite>("core1_light");
            }

            shell.color = PlayerCore.GetPlayerFactionColor();
            shell.rectTransform.sizeDelta = shell.sprite.bounds.size * 100;

            // orient shell image so relative center stays the same regardless of shell tier
            OrientShellAndCore();

            core.material = ResourceManager.GetAsset<Material>("material_color_swap");
            core.color = FactionManager.GetFactionColor(0);

            core.preserveAspect = true;
            core.rectTransform.sizeDelta = core.sprite.bounds.size * 100;

            CacheBlueprint();
        }
    }

    [System.Serializable]
    public struct TraderInventory
    {
        public List<EntityBlueprint.PartInfo> parts;
    }

    public EntityBlueprint.PartInfo? GetButtonPartCursorIsOn()
    {
        foreach (ShipBuilderInventoryScript inv in partDict.Values)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition)
                && inv.gameObject.activeSelf)
            {
                return inv.part;
            }
        }

        foreach (ShipBuilderInventoryScript traderInv in traderPartDict.Values)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(traderInv.GetComponent<RectTransform>(), Input.mousePosition)
                && traderInv.gameObject.activeSelf)
            {
                return traderInv.part;
            }
        }

        return null;
    }

    public void ChangeDisplayFactors()
    {
        foreach (GameObject obj in contentTexts)
        {
            obj.SetActive(false);
        }

        if (mode == BuilderMode.Workshop)
        {
            if (GetDroneWorkshopSelectPhase())
            {
                displayingTypes[0] = displayingTypes[2] = displayingTypes[3] = displayingTypes[4] = false;
                displayingTypes[1] = true;
            }
            else
            {
                displayingTypes[1] = false;
            }
        }

        foreach (ShipBuilderInventoryScript inv in partDict.Values)
        {
            string partName = inv.part.partID.ToLower();
            string abilityName = AbilityUtilities.GetAbilityNameByID(inv.part.abilityID, inv.part.secondaryData).ToLower() + (inv.part.tier > 0 ? " " + inv.part.tier : "");
            if (partName.Contains(searcherString) || abilityName.Contains(searcherString) || searcherString == "")
            {
                if (displayingTypes[(int)AbilityUtilities.GetAbilityTypeByID(inv.part.abilityID)] && 
                    (mode != BuilderMode.Workshop || GetDroneWorkshopSelectPhase() || ResourceManager.GetAsset<PartBlueprint>(inv.part.partID).size == 0))
                {
                    inv.gameObject.SetActive(true);
                    contentTexts[ResourceManager.GetAsset<PartBlueprint>(inv.part.partID).size].SetActive(true);
                }
                else
                {
                    inv.gameObject.SetActive(false);
                }
            }
            else
            {
                inv.gameObject.SetActive(false);
            }
        }
    }

    // The following 3 methods use editorCoreTier to set the editor core SHELL (idk why it's shell) sprites correctly
    private List<string> GetEditorCoreList()
    {
        var cores = new List<string>(CoreUpgraderScript.GetCoreNames());
        cores.Add("semicore_shell");
        cores.Add("groundcarriershell");
        cores.Add("drone_shell");
        return cores;
    }

    private string GetEditorShellString()
    {
        var cores = GetEditorCoreList();
        return cores[editorCoreTier % cores.Count];
    }

    private string GetEditorCoreString()
    {
        var cores = GetEditorCoreList();
        if (editorCoreTier == cores.Count - 3)
        {
            return "semicore_light";
        }
        else if (editorCoreTier == cores.Count - 2)
        {
            return "groundcarriercore";
        }
        else if (editorCoreTier == cores.Count - 1)
        {
            return "drone_light";
        }
        else
        {
            return "core1_light";
        }
    }

    public string GetCurrentJSON()
    {
        EntityBlueprint blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        if (!editorMode)
        {
            blueprint.coreShellSpriteID = player.blueprint.coreShellSpriteID;
            blueprint.coreSpriteID = player.blueprint.coreSpriteID;
        }
        else
        {
            blueprint.coreShellSpriteID = GetEditorShellString();
            blueprint.coreSpriteID = GetEditorCoreString();
        }

        blueprint.parts = new List<EntityBlueprint.PartInfo>();
        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            blueprint.parts.Add(part.info);
        }

        return JsonUtility.ToJson(blueprint);
    }

    public void SetSearcherString(string searcher)
    {
        searcherString = searcher.ToLower();
        ChangeDisplayFactors();
    }

    public void UpdateDisplayingCategories(int type)
    {
        displayingTypes[type] = !displayingTypes[type];
        ChangeDisplayFactors();
    }

    public override bool GetActive()
    {
        return gameObject.activeSelf;
    }

    /// prevent dragging the window if the mouse is on the grid
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(cursorScript.grid2mask, Input.mousePosition))
        {
            return;
        }

        base.OnPointerDown(eventData);
    }

    public EntityBlueprint.PartInfo? RequestInventoryMouseOverInfo()
    {
        foreach (var part in partDict)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(part.Value.GetComponent<RectTransform>(), Input.mousePosition) &&
                part.Value.gameObject.activeSelf)
            {
                return part.Key;
            }
        }

        if (traderPartDict != null)
        {
            foreach (var part in traderPartDict)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(part.Value.GetComponent<RectTransform>(), Input.mousePosition) &&
                    part.Value.gameObject.activeSelf)
                {
                    return part.Key;
                }
            }
        }

        return null;
    }
}
