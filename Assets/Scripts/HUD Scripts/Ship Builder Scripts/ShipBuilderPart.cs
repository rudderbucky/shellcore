﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
	This class exists to streamline the process of displaying an image representation of a part, and storing actual data.
	In other words, this class is made to reflect the current status of the embedded PartInfo in image form.
 */
public class ShipBuilderPart : DisplayPart, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform rectTransform;
    public ShipBuilderCursorScript cursorScript;
    public Image boundImage;
    public bool isInChain;
    public bool validPos;
    public bool highlighted;
    public BuilderMode mode;
    private Vector3? lastValidPos = null;

    public override void Initialize()
    {
        image = GetComponent<Image>();
        rectTransform = image.rectTransform;
        base.Initialize();
    }

    public void SetLastValidPos(Vector3? lastPos)
    {
        lastValidPos = lastPos;
    }

    public Vector3? GetLastValidPos()
    {
        return lastValidPos;
    }

    public void SetMaskable(bool maskable)
    {
        if (image)
        {
            image.maskable = maskable;
        }

        if (shooter)
        {
            shooter.maskable = maskable;
        }

        if (boundImage)
        {
            boundImage.maskable = maskable;
        }
    }

    public void Snapback()
    {
        if (lastValidPos != null)
        {
            info.location = (Vector3)lastValidPos;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        rectTransform = image.rectTransform;
    }

    public void InitializeMode(BuilderMode mode)
    {
        this.mode = mode;
    }

    protected override void UpdateAppearance()
    {
        // set colors
        base.UpdateAppearance();
        var mainColor = info.shiny ? FactionManager.GetFactionShinyColor(0) : FactionManager.GetFactionColor(0);
        if (highlighted)
        {
            if (isInChain && validPos)
            {
                image.material = ResourceManager.GetAsset<Material>("material_outline");
                image.color = mainColor;
            }
            else
            {
                image.color = mainColor - new Color(0, 0, 0, 0.5F);
                image.material = null;
            }
        }
        else
        {
            image.color = (isInChain && validPos ? mainColor : mainColor - new Color(0, 0, 0, 0.5F));
            switch (cursorScript.symmetryMode)
            {
                case ShipBuilderCursorScript.SymmetryMode.X:
                case ShipBuilderCursorScript.SymmetryMode.Y:
                    var symVec = cursorScript.GetSymmetrizedVector(rectTransform.anchoredPosition, cursorScript.symmetryMode);
                    var symmetryPart = cursorScript.FindPart(symVec, this);
                    var onAxis = false;
                    if (cursorScript.symmetryMode == ShipBuilderCursorScript.SymmetryMode.X)
                    {
                        onAxis = rectTransform.anchoredPosition.y == 0;
                    }

                    if (cursorScript.symmetryMode == ShipBuilderCursorScript.SymmetryMode.X)
                    {
                        onAxis = rectTransform.anchoredPosition.x == 0;
                    }

                    if ((!symmetryPart || (symmetryPart.rectTransform.anchoredPosition - symVec).sqrMagnitude >
                        ShipBuilderCursorScript.stepSize * ShipBuilderCursorScript.stepSize) && !onAxis)
                    {
                        image.color = FactionManager.GetFactionColor(1);
                    }

                    break;
                default:
                    break;
            }

            image.material = null;
        }
    }

    void OnDestroy()
    {
        if (shooter)
        {
            Destroy(shooter.gameObject);
        }
    }

    void Update()
    {
        UpdateAppearance();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlighted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlighted = false;
    }
}
