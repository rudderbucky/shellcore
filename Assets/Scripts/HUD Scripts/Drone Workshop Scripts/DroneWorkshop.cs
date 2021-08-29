using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: If there are duplicate spawning parts this probably breaks since I haven't checked how that works, fix that
enum DroneWorkshopPhase
{
    SelectionPhase,
    BuildPhase
}

public class DroneWorkshop : GUIWindowScripts, IBuilderInterface
{
    DroneWorkshopPhase phase;
    public Vector3 yardPosition;
    bool initialized;
    public ShipBuilderCursorScript cursorScript;
    Transform[] contentsArray;
    public Transform smallContents;
    public Transform mediumContents;
    public Transform largeContents;
    GameObject[] contentTexts;
    public GameObject smallText;
    public GameObject mediumText;
    public GameObject largeText;
    public PlayerCore player;
    protected Dictionary<DWInventoryButton, EntityBlueprint.PartInfo> partDict;
    public GameObject displayButtonPrefab;
    public DWSelectionDisplayHandler selectionDisplay;
    public GameObject selectionPhaseParent;
    public GameObject buildPhaseParent;
    public Image coreImage;
    public Image shellImage;
    public GameObject partPrefab;
    public Transform smallBuilderContents;
    public Transform mediumBuilderContents;
    public Transform largeBuilderContents;
    public GameObject smallBuilderText;
    public GameObject mediumBuilderText;
    public GameObject largeBuilderText;
    public GameObject buttonPrefab;
    protected Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript> builderPartDict;
    EntityBlueprint.PartInfo currentPart;
    public Image miniDroneShooter;
    public Image reconstructImage;
    public Text reconstructText;
    private DroneSpawnData currentData;
    private string searcherString;
    private bool[] displayingTypes;

    private enum ReconstructButtonStatus
    {
        Valid,
        PartInvalidPosition,
        PastPartLimit
    }

    private void SetReconstructButton(ReconstructButtonStatus status)
    {
        switch (status)
        {
            case ReconstructButtonStatus.Valid:
                reconstructText.color = Color.green;
                reconstructText.text = "Reconstruct";
                break;
            case ReconstructButtonStatus.PartInvalidPosition:
                reconstructText.color = Color.red;
                reconstructText.text = "A part is in an invalid position!";
                break;
            case ReconstructButtonStatus.PastPartLimit:
                reconstructText.color = Color.red;
                reconstructText.text = "Core cannot handle so many parts!";
                break;
        }

        reconstructText.text = reconstructText.text.ToUpper();
    }

    public void InitializeSelectionPhase()
    {
        searcherString = "";
        selectionPhaseParent.SetActive(true);
        buildPhaseParent.SetActive(false);
        //initialize window on screen
        if (initialized)
        {
            CloseUI(false); // prevent initializing twice by closing UI if already initialized
        }

        initialized = true;
        Activate();
        cursorScript.gameObject.SetActive(false);
        cursorScript.SetBuilder(this);

        contentsArray = new Transform[] {smallContents, mediumContents, largeContents};
        contentTexts = new GameObject[] {smallText, mediumText, largeText};
        foreach (GameObject obj in contentTexts)
        {
            obj.SetActive(false);
        }

        GetComponentInChildren<ShipBuilderPartDisplay>().Initialize(this);
        player.SetIsInteracting(true);
        partDict = new Dictionary<DWInventoryButton, EntityBlueprint.PartInfo>();

        // hide the buttons and yard tips if interacting with a trader

        List<EntityBlueprint.PartInfo> parts = player.GetInventory();
        if (parts != null)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i] = ShipBuilder.CullSpatialValues(parts[i]);
            }
        }

        foreach (EntityBlueprint.PartInfo part in parts)
        {
            if (part.abilityID == 10)
            {
                AddDronePart(part);
            }
        }

        foreach (EntityBlueprint.PartInfo part in player.blueprint.parts)
        {
            if (part.abilityID == 10)
            {
                AddDronePart(part);
            }
        }

        var partsToAdd = new List<ShellPart>();
        foreach (Entity ent in player.GetUnitsCommanding())
        {
            if (!((ent as Drone) && ent.GetComponentInChildren<TractorBeam>()))
            {
                continue;
            }

            var target = ent.GetComponentInChildren<TractorBeam>().GetTractorTarget();
            if (target && target.GetComponent<ShellPart>())
            {
                partsToAdd.Add(target.GetComponent<ShellPart>());
            }
        }

        if (player.GetTractorTarget() && player.GetTractorTarget().GetComponent<ShellPart>())
        {
            partsToAdd.Add(player.GetTractorTarget().GetComponent<ShellPart>());
        }

        foreach (ShellPart part in partsToAdd)
        {
            var info = part.info;
            info = ShipBuilder.CullSpatialValues(info);
            if (info.abilityID == 10)
            {
                int size = ResourceManager.GetAsset<PartBlueprint>(info.partID).size;
                var button = Instantiate(displayButtonPrefab, contentsArray[size]).GetComponent<DWInventoryButton>();
                button.handler = selectionDisplay;
                button.workshop = this;
                contentTexts[size].SetActive(true);
                button.part = info;
                partDict.Add(button, info);
            }

            player.cursave.partInventory.Add(info);
            Destroy(part.gameObject);
        }

        phase = DroneWorkshopPhase.SelectionPhase;
        // activate windows
        gameObject.SetActive(true);
    }

    // adds a DWInventoryButton, for SBInventoryButton use AddPart
    public void AddDronePart(EntityBlueprint.PartInfo part)
    {
        int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
        DWInventoryButton invButton = Instantiate(displayButtonPrefab,
            contentsArray[size]).GetComponent<DWInventoryButton>();
        invButton.handler = selectionDisplay;
        invButton.workshop = this;
        partDict.Add(invButton, part);
        contentTexts[size].SetActive(true);
        invButton.part = part;
    }

    private void AddPart(EntityBlueprint.PartInfo part)
    {
        if (!builderPartDict.ContainsKey(part))
        {
            int size = ResourceManager.GetAsset<PartBlueprint>(part.partID).size;
            ShipBuilderInventoryScript invButton = Instantiate(buttonPrefab,
                contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
            builderPartDict.Add(part, invButton);
            contentTexts[size].SetActive(true);
            invButton.part = part;
            invButton.cursor = cursorScript;
            invButton.IncrementCount();
            invButton.mode = BuilderMode.Yard;
        }
        else
        {
            builderPartDict[part].IncrementCount();
        }
    }

    public BuilderMode GetMode()
    {
        return BuilderMode.Yard;
    }

    public void DispatchPart(ShipBuilderPart part, ShipBuilder.TransferMode mode)
    {
        var culledInfo = ShipBuilder.CullSpatialValues(part.info);
        if (!builderPartDict.ContainsKey(culledInfo))
        {
            int size = ResourceManager.GetAsset<PartBlueprint>(part.info.partID).size;
            ShipBuilderInventoryScript builderPartDictInvButton = Instantiate(buttonPrefab,
                contentsArray[size]).GetComponent<ShipBuilderInventoryScript>();
            builderPartDict.Add(culledInfo, builderPartDictInvButton);
            contentTexts[size].SetActive(true);
            builderPartDict[culledInfo].part = culledInfo;
            builderPartDict[culledInfo].cursor = cursorScript;
        }

        builderPartDict[culledInfo].IncrementCount();
        cursorScript.buildValue -= EntityBlueprint.GetPartValue(part.info);
        cursorScript.parts.Remove(part);
        Destroy(part.gameObject);
    }

    public void CloseUI(bool val)
    {
        // try adding parts in the player's inventory and on their ship into the part index obtained list.
        if (val && builderPartDict != null)
        {
            var spawnParts = player.cursave.partInventory.FindAll(p => p.abilityID == 10);
            player.cursave.partInventory = spawnParts;
            foreach (EntityBlueprint.PartInfo info in builderPartDict.Keys)
            {
                if (builderPartDict[info].GetCount() > 0)
                {
                    for (int i = 0; i < builderPartDict[info].GetCount(); i++)
                    {
                        player.cursave.partInventory.Add(info);
                        PartIndexScript.AttemptAddToPartsObtained(info);
                    }
                }
            }
        }

        if (builderPartDict != null)
        {
            foreach (ShipBuilderInventoryScript inv in builderPartDict.Values)
            {
                Destroy(inv.gameObject);
            }

            builderPartDict = null;
        }

        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            Destroy(part.gameObject);
        }

        cursorScript.parts.Clear();
        if (partDict != null)
        {
            foreach (DWInventoryButton but in partDict.Keys)
            {
                Destroy(but.gameObject);
            }

            partDict = null;
        }

        player.SetIsInteracting(false);
        base.CloseUI();
        player.Rebuild();
    }

    public override void CloseUI()
    {
        CloseUI(false);
    }

    void UpdateChainHelper(ShipBuilderPart part)
    {
        var x = ShipBuilder.GetRect(part.rectTransform);
        foreach (ShipBuilderPart shipPart in cursorScript.parts)
        {
            if (!shipPart.isInChain)
            {
                var y = ShipBuilder.GetRect(shipPart.rectTransform);
                if (x.Intersects(y))
                {
                    shipPart.isInChain = true;
                    UpdateChainHelper(shipPart);
                }
            }
        }
    }

    public void UpdateChain()
    {
        SetReconstructButton(cursorScript.parts.Count > DroneUtilities.GetPartLimit(currentData.type) ? ReconstructButtonStatus.PastPartLimit : ReconstructButtonStatus.Valid);

        foreach (ShipBuilderPart shipPart in cursorScript.parts)
        {
            shipPart.isInChain = false;
            CheckPartIntersectsWithShell(shipPart);
        }

        foreach (ShipBuilderPart shipPart in cursorScript.parts)
        {
            if (shipPart.isInChain)
            {
                UpdateChainHelper(shipPart);
            }
        }

        foreach (ShipBuilderPart shipPart in cursorScript.parts)
        {
            CheckPartIntersectsWithShell(shipPart);
        }

        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            if (part.validPos)
            {
                foreach (var part2 in cursorScript.parts)
                {
                    if (part != part2 && PartIsTooClose(part, part2))
                    {
                        part.validPos = false;
                        break;
                    }
                }
            }
            else
            {
                bool stillTouching = false;
                foreach (ShipBuilderPart part2 in cursorScript.parts)
                {
                    if (part2 != part && PartIsTooClose(part, part2))
                    {
                        stillTouching = true;
                        break;
                    }
                }

                if (!stillTouching)
                {
                    part.validPos = true;
                }
            }
        }

        foreach (ShipBuilderPart shipPart in cursorScript.parts)
        {
            if (!shipPart.isInChain || !shipPart.validPos)
            {
                SetReconstructButton(ReconstructButtonStatus.PartInvalidPosition);
                return;
            }
        }
    }

    bool PartIsTooClose(ShipBuilderPart part, ShipBuilderPart otherPart)
    {
        var closeConstant = -2F;
        var rect1 = ShipBuilder.GetRect(part.rectTransform);
        var rect2 = ShipBuilder.GetRect(otherPart.rectTransform);
        // add small number (0.005) to deal with floating point issues
        rect1.Expand((closeConstant - 0.005F) * rect1.extents);
        rect2.Expand((closeConstant - 0.005F) * rect2.extents);
        return rect1.Intersects(rect2);
    }

    public EntityBlueprint.PartInfo? GetButtonPartCursorIsOn()
    {
        switch (phase)
        {
            case DroneWorkshopPhase.SelectionPhase:
                foreach (DWInventoryButton inv in partDict.Keys)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition)
                        && inv.gameObject.activeSelf)
                    {
                        return inv.part;
                    }
                }

                return null;
            case DroneWorkshopPhase.BuildPhase:
                foreach (ShipBuilderInventoryScript inv in builderPartDict.Values)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(inv.GetComponent<RectTransform>(), Input.mousePosition)
                        && inv.gameObject.activeSelf)
                    {
                        return inv.part;
                    }
                }

                return null;
            default:
                return null;
        }
    }

    public static DroneSpawnData ParseDronePart(EntityBlueprint.PartInfo part)
    {
        if (part.abilityID != 10)
        {
            Debug.Log("Passed part is not a drone spawner!");
        }

        var data = DroneUtilities.GetDroneSpawnDataByShorthand(part.secondaryData);
        //JsonUtility.FromJsonOverwrite(part.secondaryData, data);
        return data;
    }

    public void InitializeBuildPhase(EntityBlueprint blueprint, EntityBlueprint.PartInfo currentPart, DroneSpawnData data)
    {
        searcherString = "";
        displayingTypes = new bool[] {true, false, true, true, true};
        this.currentData = data;
        this.currentPart = currentPart;
        selectionPhaseParent.SetActive(false);
        buildPhaseParent.SetActive(true);
        LoadBlueprint(blueprint, data);

        builderPartDict = new Dictionary<EntityBlueprint.PartInfo, ShipBuilderInventoryScript>();
        contentsArray = new Transform[] {smallBuilderContents, mediumBuilderContents, largeBuilderContents};
        contentTexts = new GameObject[] {smallBuilderText, mediumBuilderText, largeBuilderText};
        foreach (GameObject obj in contentTexts)
        {
            obj.SetActive(false);
        }

        foreach (Transform inv in smallBuilderContents)
        {
            Destroy(inv.gameObject);
        }

        foreach (Transform inv in mediumBuilderContents)
        {
            Destroy(inv.gameObject);
        }

        foreach (Transform inv in largeBuilderContents)
        {
            Destroy(inv.gameObject);
        }

        partDict.Clear();


        foreach (EntityBlueprint.PartInfo part in player.GetInventory())
        {
            if (part.abilityID != 10 && ResourceManager.GetAsset<PartBlueprint>(part.partID).size == 0)
            {
                AddPart(part);
            }
        }

        GetComponentInChildren<ShipBuilderPartDisplay>().Initialize(this);
        phase = DroneWorkshopPhase.BuildPhase;

        cursorScript.gameObject.SetActive(true);
        cursorScript.SetMode(BuilderMode.Workshop);
    }

    public void LoadBlueprint(EntityBlueprint blueprint, DroneSpawnData data)
    {
        shellImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreShellSpriteID);
        if (shellImage.sprite)
        {
            shellImage.enabled = true;
            shellImage.color = FactionManager.GetFactionColor(0);
            shellImage.rectTransform.sizeDelta = shellImage.sprite.bounds.size * 100;
            shellImage.type = Image.Type.Sliced;
            // orient shell image so relative center stays the same regardless of shell tier
            shellImage.rectTransform.anchoredPosition = -shellImage.sprite.pivot + shellImage.rectTransform.sizeDelta / 2;
        }
        else
        {
            shellImage.enabled = false;
        }

        coreImage.rectTransform.anchoredPosition = -shellImage.rectTransform.anchoredPosition;
        coreImage.sprite = ResourceManager.GetAsset<Sprite>(blueprint.coreSpriteID);
        if (coreImage.sprite)
        {
            coreImage.enabled = true;
            coreImage.material = ResourceManager.GetAsset<Material>("material_color_swap");
            coreImage.color = FactionManager.GetFactionColor(0);
            coreImage.preserveAspect = true;
            coreImage.type = Image.Type.Sliced;
            coreImage.rectTransform.sizeDelta = coreImage.sprite.bounds.size * 100;
        }
        else
        {
            coreImage.enabled = false;
        }

        if (data.type == DroneType.Mini)
        {
            miniDroneShooter.gameObject.SetActive(true);
            miniDroneShooter.enabled = true;
            miniDroneShooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(6));
            miniDroneShooter.color = FactionManager.GetFactionColor(0);
            miniDroneShooter.rectTransform.sizeDelta = miniDroneShooter.sprite.bounds.size * 100;
            miniDroneShooter.type = Image.Type.Sliced;
        }
        else
        {
            miniDroneShooter.gameObject.SetActive(false);
            miniDroneShooter.enabled = false;
        }

        foreach (EntityBlueprint.PartInfo part in blueprint.parts)
        {
            var p = Instantiate(partPrefab, cursorScript.transform.parent).GetComponent<ShipBuilderPart>();
            p.cursorScript = cursorScript;
            cursorScript.parts.Add(p);
            p.info = part;
            p.SetLastValidPos(part.location);
            p.isInChain = true;
            p.validPos = true;
            p.InitializeMode(BuilderMode.Workshop);
        }
    }

    /// prevent dragging the window if the mouse is on the grid
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(cursorScript.grid, Input.mousePosition))
        {
            return;
        }

        base.OnPointerDown(eventData);
    }

    private void Export()
    {
        var data = ScriptableObject.CreateInstance<DroneSpawnData>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(ParseDronePart(currentPart)), data);

        var blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(DroneWorkshop.ParseDronePart(currentPart).drone, blueprint);
        blueprint.parts = new List<EntityBlueprint.PartInfo>();
        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            blueprint.parts.Add(part.info);
        }

        data.drone = JsonUtility.ToJson(blueprint);
        var list = player.GetInventory();
        var index = list.FindIndex(x => x.Equals(currentPart));
        if (index == -1)
        {
            list = player.blueprint.parts;
            index = list.FindIndex(x => x.Equals(currentPart));
        }

        currentPart.secondaryData = JsonUtility.ToJson(data);
        list[index] = currentPart;
        CloseUI(true);
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ShipBuilder.SaveBlueprint(blueprint);
        }
#endif
    }

    public string GetCurrentJSON()
    {
        var data = ScriptableObject.CreateInstance<DroneSpawnData>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(ParseDronePart(currentPart)), data);

        var blueprint = ScriptableObject.CreateInstance<EntityBlueprint>();
        JsonUtility.FromJsonOverwrite(DroneWorkshop.ParseDronePart(currentPart).drone, blueprint);
        blueprint.parts = new List<EntityBlueprint.PartInfo>();
        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            blueprint.parts.Add(part.info);
        }

        data.drone = JsonUtility.ToJson(blueprint);
        return JsonUtility.ToJson(data);
    }

    public void Deinitialize()
    {
        if (cursorScript.parts.Count > DroneUtilities.GetPartLimit(currentData.type))
        {
            return;
        }

        bool invalidState = false;
        foreach (ShipBuilderPart part in cursorScript.parts)
        {
            if (!part.validPos || !part.isInChain)
            {
                invalidState = true;
                break;
            }
        }

        if (!invalidState)
        {
            Export();
        }
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

    public void ChangeDisplayFactors()
    {
        foreach (GameObject obj in contentTexts)
        {
            obj.SetActive(false);
        }

        if (phase == DroneWorkshopPhase.BuildPhase)
        {
            foreach (ShipBuilderInventoryScript inv in builderPartDict.Values)
            {
                string partName = inv.part.partID.ToLower();
                string abilityName = AbilityUtilities.GetAbilityNameByID(inv.part.abilityID, inv.part.secondaryData).ToLower();
                if (partName.Contains(searcherString) || abilityName.Contains(searcherString) || searcherString == "")
                {
                    if (displayingTypes[(int)AbilityUtilities.GetAbilityTypeByID(inv.part.abilityID)])
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
        else
        {
            foreach (DWInventoryButton inv in partDict.Keys)
            {
                string partName = inv.part.partID.ToLower();
                string abilityName = AbilityUtilities.GetAbilityNameByID(inv.part.abilityID, inv.part.secondaryData).ToLower();
                if (partName.Contains(searcherString) || abilityName.Contains(searcherString) || searcherString == "")
                {
                    inv.gameObject.SetActive(true);
                    contentTexts[ResourceManager.GetAsset<PartBlueprint>(inv.part.partID).size].SetActive(true);
                }
                else
                {
                    inv.gameObject.SetActive(false);
                }
            }
        }
    }

    // TODO: Test this when the Drone Workshop is added into the game
    public bool CheckPartIntersectsWithShell(ShipBuilderPart shipPart)
    {
        var shellRect = ShipBuilder.GetRect(shellImage.rectTransform);

        var partBounds = ShipBuilder.GetRect(shipPart.rectTransform);
        if (partBounds.Intersects(shellRect))
        {
            /*
            bool z = Mathf.Abs(shipPart.rectTransform.anchoredPosition.x - shellImage.rectTransform.anchoredPosition.x) <=
            0.18F*(shipPart.rectTransform.sizeDelta.x + shellImage.rectTransform.sizeDelta.x) &&
            Mathf.Abs(shipPart.rectTransform.anchoredPosition.y - shellImage.rectTransform.anchoredPosition.y) <=
            0.18F*(shipPart.rectTransform.sizeDelta.y + shellImage.rectTransform.sizeDelta.y);
            */
            shipPart.isInChain = true; //!z;

            // reset sprite

            //return true;
            return true; //z;
        }

        return false;
    }
}
