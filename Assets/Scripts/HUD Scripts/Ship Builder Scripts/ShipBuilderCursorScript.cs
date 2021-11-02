using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public interface IBuilderInterface
{
    BuilderMode GetMode();
    void DispatchPart(ShipBuilderPart part, ShipBuilder.TransferMode mode, bool updateChain = true);
    void UpdateChain();
    EntityBlueprint.PartInfo? GetButtonPartCursorIsOn();
    void SetSearcherString(string text);
    bool CheckPartIntersectsWithShell(ShipBuilderPart shipPart);
    string GetCurrentJSON();
}

public class ShipBuilderCursorScript : MonoBehaviour, IShipStatsDatabase
{
    public List<ShipBuilderPart> parts = new List<ShipBuilderPart>();
    public Canvas canvas;
    public RectTransform grid;

    [SerializeField]
    ShipBuilderPart currentPart;

    ShipBuilderPart lastPart;
    public IBuilderInterface builder;
    public InputField searchField;
    public InputField jsonField;
    bool flipped;
    public AbilityHandler handler;
    public PlayerCore player;
    List<Ability> currentAbilities;
    public int buildValue;
    public int buildCost;
    public RectTransform playerInventory;
    public RectTransform traderInventory;
    public BuilderMode cursorMode = BuilderMode.Yard;
    private Vector2 offset;

    public RectTransform grid2;
    public RectTransform grid2mask;
    private Vector2 grid2lastPos;
    private Vector2 grid2mousePos;
    bool clickedOnce;
    float timer;

    public void SetMode(BuilderMode mode)
    {
        cursorMode = mode;
    }

    public void SetBuilder(IBuilderInterface builder)
    {
        this.builder = builder;
    }

    void OnEnable()
    {
        compactMode = Screen.width == 1024;
        buildCost = 0;
        currentAbilities = new List<Ability>();

        grid.anchoredPosition = Vector2.zero;
        buildValue = 0;
        foreach (ShipBuilderPart part in parts)
        {
            buildValue += EntityBlueprint.GetPartValue(part.info);
        }
    }

    public EntityBlueprint.PartInfo? GetCurrentInfo()
    {
        if (!currentPart)
        {
            return null;
        }

        return currentPart.info;
    }

    public EntityBlueprint.PartInfo? GetLastInfo()
    {
        if (!lastPart)
        {
            return null;
        }

        return lastPart.info;
    }

    public void GrabPart(ShipBuilderPart part, ShipBuilderPart symmetryPart = null)
    {
        lastPart = null;
        symmetryLastPart = null;
        if (symmetryPart)
        {
            if (parts.Contains(symmetryPart))
            {
                parts.Remove(symmetryPart);
                parts.Add(symmetryPart);
                symmetryPart.rectTransform.SetAsLastSibling();
            }
        }

        if (parts.Contains(part))
        {
            parts.Remove(part);
            parts.Add(part);
            part.rectTransform.SetAsLastSibling();
        }

        // code to ensure part does not snap to cursor immediately
        var pos = part.GetComponent<RectTransform>().anchoredPosition;

        // if the initial position is zero that means this is a new part, don't set an offset
        if (pos == Vector2.zero)
        {
            offset = Vector2.zero;
        }
        else
        {
            offset = pos - GetComponent<RectTransform>().anchoredPosition;
        }

        currentPart = part;
        symmetryCurrentPart = symmetryPart;
    }

    void PlaceCurrentPart()
    {
        currentPart.SetMaskable(true);
        var editorMode = builder is ShipBuilder shipBuilder && shipBuilder.editorMode;
        var dispatch = false;
        ShipBuilder.TransferMode mode = ShipBuilder.TransferMode.Return;
        if (cursorMode != BuilderMode.Workshop)
        {
            if (traderInventory.gameObject.activeSelf && (!editorMode || !Input.GetKey(KeyCode.LeftControl)) &&
                RectTransformUtility.RectangleContainsScreenPoint(traderInventory, Input.mousePosition))
            {
                dispatch = true;
                mode = (currentPart.mode == BuilderMode.Yard
                    ? ShipBuilder.TransferMode.Sell
                    : ShipBuilder.TransferMode.Return);
            }
            else if ((!editorMode || !Input.GetKey(KeyCode.LeftControl)) &&
                     RectTransformUtility.RectangleContainsScreenPoint(playerInventory, Input.mousePosition))
            {
                dispatch = true;
                mode = (currentPart.mode == BuilderMode.Yard
                    ? ShipBuilder.TransferMode.Return
                    : ShipBuilder.TransferMode.Buy);
            }
            else if (!RectTransformUtility.RectangleContainsScreenPoint(grid, Input.mousePosition) && !editorMode)
            {
                dispatch = true;
                mode = ShipBuilder.TransferMode.Return;
            }
            else if (builder.CheckPartIntersectsWithShell(currentPart) && currentPart.GetLastValidPos() == null)
            {
                dispatch = true;
                mode = ShipBuilder.TransferMode.Return;
            }
            else
            {
                PlaceCurrentPartInGrid();
            }
        }
        else
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(playerInventory, Input.mousePosition))
            {
                dispatch = true;
                mode = ShipBuilder.TransferMode.Return;
            }
            else
            {
                PlaceCurrentPartInGrid();
            }
        }

        if (dispatch)
        {
            builder.DispatchPart(currentPart, mode);
            if (symmetryCurrentPart)
            {
                builder.DispatchPart(symmetryCurrentPart, mode);
            }
        }

        if (handler && currentAbilities != null)
        {
            UpdateHandler();
        }
    }

    private void PlaceCurrentPartInGrid()
    {
        lastPart = currentPart;
        symmetryLastPart = symmetryCurrentPart;
        currentPart = null;
        symmetryCurrentPart = null;
        if (lastPart.isInChain && lastPart.validPos)
        {
            lastPart.SetLastValidPos(lastPart.info.location);
        }
        else if (Input.GetKey(KeyCode.LeftShift) || builder.CheckPartIntersectsWithShell(lastPart))
        {
            lastPart.Snapback();
        }
    }

    public void UpdateHandler()
    {
        if (!handler || currentAbilities == null)
        {
            return;
        }

        currentAbilities.Clear();
        foreach (Ability ab in gameObject.GetComponentsInChildren<Ability>())
        {
            Destroy(ab);
        }

        foreach (ShipBuilderPart part in parts)
        {
            if (part.info.abilityID != 0)
            {
                Ability dispAb = AbilityUtilities.AddAbilityToGameObjectByID(gameObject, part.info.abilityID,
                    part.info.secondaryData, part.info.tier);
                currentAbilities.Insert(0, dispAb);
            }
        }

        currentAbilities.Insert(0, gameObject.AddComponent<MainBullet>());
        if (handler)
        {
            handler.Deinitialize();
            handler.Initialize(player, currentAbilities.ToArray());
        }
    }

    public EntityBlueprint.PartInfo? GetPartCursorIsOn()
    {
        foreach (ShipBuilderPart part in parts)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(part.rectTransform, Input.mousePosition))
            {
                return part.info;
            }
        }

        if (builder is ShipBuilder shipBuilder)
        {
            return shipBuilder.RequestInventoryMouseOverInfo();
        }

        return null;
    }

    public void ClearAllParts()
    {
        while (parts.Count > 0)
        {
            builder.DispatchPart(parts[0], ShipBuilder.TransferMode.Return, false);
        }

        UpdateHandler();
        builder.UpdateChain();
    }

    public bool rotateMode;

    public void RotateLastPart()
    {
        var x = Input.mousePosition - lastPart.transform.position;
        var y = new Vector3(0, 0, (Mathf.Rad2Deg * Mathf.Atan(x.y / x.x) - (x.x >= 0 ? 90 : -90)));
        if (!float.IsNaN(y.z))
        {
            y.z = 15 * (Mathf.RoundToInt(y.z / 15));
            lastPart.info.rotation = y.z;
        }
        else
        {
            lastPart.info.rotation = 0;
        }

        if (symmetryLastPart && symmetryMode == SymmetryMode.X)
        {
            symmetryLastPart.info.rotation = -lastPart.info.rotation;
        }

        if (symmetryLastPart && symmetryMode == SymmetryMode.Y)
        {
            symmetryLastPart.info.rotation = -lastPart.info.rotation + 180;
        }

        return;
    }

    public void FlipLastPart()
    {
        lastPart.info.mirrored = !lastPart.info.mirrored;
        if (symmetryLastPart)
        {
            symmetryLastPart.info.mirrored = !lastPart.info.mirrored;
        }

        flipped = true;
    }

    private bool compactMode = true;
    public RectTransform inventorySection;
    public RectTransform statsSection;
    public RectTransform background;

    private void UpdateCompact()
    {
        if (compactMode)
        {
            inventorySection.anchoredPosition = new Vector2(-465.15F + 125, 0);
            statsSection.anchoredPosition = new Vector2(-125 + 6.35F, 0);
            grid2mask.sizeDelta = new Vector2(750, 1250);
            background.sizeDelta = new Vector2(1270 - 250, 670);
        }
        else
        {
            inventorySection.anchoredPosition = new Vector2(-465.15F, 0);
            statsSection.anchoredPosition = new Vector2(6.35F, 0);
            grid2mask.sizeDelta = new Vector2(1250, 1250);
            background.sizeDelta = new Vector2(1270, 670);
        }
    }

    public enum SymmetryMode
    {
        Off,
        X,
        Y
    }

    public SymmetryMode symmetryMode = SymmetryMode.Off;
    public Text symmetryButtonText;

    // symmetry mode is used by code to determine axis of symmetry
    public void ToggleSymmetryMode()
    {
        symmetryMode = (SymmetryMode)((int)++symmetryMode % System.Enum.GetNames(typeof(SymmetryMode)).Length);
        if (symmetryButtonText)
        {
            symmetryButtonText.text = "SYMMETRY MODE: " + symmetryMode.ToString().ToUpper();
        }
    }

    ShipBuilderPart symmetryCurrentPart;
    ShipBuilderPart symmetryLastPart;

    // flips the x/y value of the passed vector based on passed symmetry mode
    public Vector2 GetSymmetrizedVector(Vector2 vec, SymmetryMode mode)
    {
        switch (mode)
        {
            case SymmetryMode.X:
                return new Vector2(-vec.x, vec.y);
            case SymmetryMode.Y:
                return new Vector2(vec.x, -vec.y);
            default:
                return vec;
        }
    }

    public static readonly float stepSize = 0.1F;

    void Update()
    {
        UpdateCompact();

        if (clickedOnce)
        {
            if (timer > 0.2F)
            {
                clickedOnce = false;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        int baseMoveSize = cursorMode == BuilderMode.Yard ? 4 : 5;

        if (Input.GetKeyDown(KeyCode.C) && (!searchField.isFocused && !jsonField.isFocused && !WCWorldIO.active))
        {
            if (builder as ShipBuilder == null || !(builder as ShipBuilder).Equals(null))
            {
                if (!(new List<InputField>((builder as ShipBuilder).GetComponentsInChildren<InputField>())).Exists(f => f.isFocused))
                {
                    ClearAllParts();
                }
            }
        }

        foreach (var part in parts)
        {
            part.boundImage.enabled = Input.GetKey(KeyCode.LeftShift);
        }

        System.Func<Vector3, int, int, Vector3> roundToRatios = (x, y, z) => new Vector3(y * ((int)x.x / y), z * ((int)x.y / z), 0);
        var newOffset = roundToRatios(grid.position, baseMoveSize, baseMoveSize) - grid.position;
        transform.position = roundToRatios(Input.mousePosition, baseMoveSize, baseMoveSize) - newOffset;
        var oldPos = GetComponent<RectTransform>().anchoredPosition;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Round(oldPos.x / 10) * 10, Mathf.Round(oldPos.y / 10) * 10);
        // round to nearest 0.1
        // TODO: Make this stuff less messy. Regardless, consistency achieved!
        if (rotateMode)
        {
            RotateLastPart();
            return;
        }

        if (flipped)
        {
            grid2mousePos = Input.mousePosition;
            flipped = false;
            return;
        }

        UpdateCurrentPart();

        // drag grid
        Vector2 bounds = grid.sizeDelta / 2 - grid2mask.sizeDelta / 2;
        if (grid.GetComponent<DragDetector>().dragging
            && Input.GetMouseButton(0) && !rotateMode && !flipped && !currentPart)
        {
            grid.anchoredPosition = grid2lastPos + ((Vector2)Input.mousePosition - grid2mousePos) * 2;
            grid.anchoredPosition = new Vector2(Mathf.Max(-bounds.x, Mathf.Min(bounds.x, grid.anchoredPosition.x)),
                Mathf.Max(-bounds.y, Mathf.Min(bounds.y, grid.anchoredPosition.y))
            );
        }
    }

    public void UpdateCurrentPart()
    {
        if (currentPart)
        {
            currentPart.SetMaskable(false);
            var oldLoc = currentPart.info.location;
            Vector2 newLoc;
            currentPart.info.location = newLoc = (GetComponent<RectTransform>().anchoredPosition + offset) / 100;
            if (symmetryCurrentPart)
            {
                symmetryCurrentPart.info.location = GetSymmetrizedVector(currentPart.info.location, symmetryMode);
            }

            if (Input.GetMouseButtonUp(0))
            {
                PlaceCurrentPart();
            }

            if (oldLoc != newLoc || Input.GetMouseButtonUp(0))
            {
                if (currentPart)
                {
                    currentPart.ReflectLocation();
                }

                builder.UpdateChain();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            grid2lastPos = grid.anchoredPosition;
            grid2mousePos = Input.mousePosition;
            lastPart = null;

            var vecPos = GetComponent<RectTransform>().anchoredPosition;
            var part = FindPart(vecPos, null, true);
            // check for symmetry mode and grab parts accordingly
            if (part)
            {
                if (symmetryMode != SymmetryMode.Off)
                {
                    var symmetryPart = FindPart(GetSymmetrizedVector(part.rectTransform.anchoredPosition, symmetryMode), part);
                    if (symmetryPart == part)
                    {
                        symmetryPart = null;
                    }

                    GrabPart(part, symmetryPart);
                }
                else
                {
                    GrabPart(part);
                }
            }

            if (clickedOnce && !rotateMode && !flipped && !currentPart)
            {
                grid2lastPos = grid.anchoredPosition = Vector2.zero;
            }
            else
            {
                timer = 0;
                clickedOnce = true;
            }
        }
    }

    // Finds the part which contains the passed vector point
    // symmetry mode enables checks based on symmetryPart - same part ID, ability ID, different mirrored
    public ShipBuilderPart FindPart(Vector2 vector, ShipBuilderPart symmetryPart, bool useBounds = false)
    {
        for (int i = parts.Count - 1; i >= 0; i--)
        {
            var origPos = transform.position;
            if (!useBounds && !PositionsCloseEnough(parts[i].rectTransform.anchoredPosition, vector)) continue;
            else if (useBounds)
            {
                Bounds bound = ShipBuilder.GetRect(parts[i].rectTransform);
                bound.extents /= 1.5F;

                transform.position = Input.mousePosition;
                if (!bound.Contains(vector))
                {
                    continue;
                }
            }
            if ((!symmetryPart || (parts[i].info.partID == symmetryPart.info.partID &&
                (symmetryMode != SymmetryMode.X
                    || CheckOrientationCompatibility(parts[i].info, symmetryPart.info)))))
            {
                transform.position = origPos;
                return parts[i];
            }

            transform.position = origPos;
        }

        return null;
    }

    // returns whether two positions are close enough ship builder-wise
    public bool PositionsCloseEnough(Vector2 vec1, Vector2 vec2)
    {
        return (vec1 - vec2).sqrMagnitude <= ShipBuilderCursorScript.stepSize;
    }

    private enum PartSymmetry
    {
        None,
        MirrorXAxis,
        MirrorYAxis,
        MirrorBothAxes
    }

    // Hardcodes axial symmetry for specific part IDs
    private static PartSymmetry GetPartSymmetry(string partID)
    {
        switch (partID)
        {
            case "SmallSide1":
            case "SmallSide3":
            case "SmallSide4":
            case "MediumSide2":
                return PartSymmetry.MirrorXAxis;
            case "MediumExtra1":
            case "MediumCenter4":
                return PartSymmetry.MirrorYAxis;
            case "SmallSide2":
                return PartSymmetry.MirrorBothAxes;
            default:
                if (partID.Contains("Center")) return PartSymmetry.MirrorYAxis;
                return PartSymmetry.None;
        }
    }

    // Currently only does something for X-axis checks.
    private bool CheckOrientationCompatibility(EntityBlueprint.PartInfo part, EntityBlueprint.PartInfo symmetryPart)
    {
        var partID = part.partID;
        part.rotation %= 360;
        symmetryPart.rotation %= 360;
        switch (GetPartSymmetry(part.partID))
        {
            case PartSymmetry.MirrorYAxis:
            case PartSymmetry.MirrorBothAxes:
                return (part.rotation + symmetryPart.rotation) % 360 == 0;
            case PartSymmetry.MirrorXAxis:
                // There are cases where the parts are symmetrically aligned for both same-mirror and opposite-mirror pairs
                var diff = Mathf.Abs(part.rotation + symmetryPart.rotation);
                if (part.mirrored != symmetryPart.mirrored)
                {
                    return diff % 360 == 0;
                }
                return diff % 360 == 180;
            case PartSymmetry.None:
            default:
                return part.mirrored != symmetryPart.mirrored && ((part.rotation + symmetryPart.rotation) % 360 == 0);
        }
    }

    public void ToggleCompact()
    {
        compactMode = !compactMode;
        UpdateCompact();
    }

    public List<DisplayPart> GetParts()
    {
        return parts.ConvertAll(x => x as DisplayPart);
    }

    public BuilderMode GetMode()
    {
        return cursorMode;
    }

    public int GetBuildValue()
    {
        return buildValue;
    }

    public int GetBuildCost()
    {
        return buildCost;
    }
}
