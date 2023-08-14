using UnityEngine;
using UnityEngine.UI;

public class DisplayPart : MonoBehaviour
{
    [HideInInspector]
    public Image image;
    protected Image shooter;
    public EntityBlueprint.PartInfo info;
    private bool initialized = false;
    private int faction = 0;
    private float opacity = 1f;

    protected virtual void Awake()
    {
        image = GetComponent<Image>();
    }

    // Use to avoid race condition BS
    public virtual void Initialize()
    {
        if (initialized)
        {
            return;
        }

        image = GetComponent<Image>();
        GameObject shooterObj = new GameObject("shooter");
        shooterObj.transform.SetParent(transform.parent);
        shooter = shooterObj.AddComponent<Image>();
        shooter.enabled = false;
        shooter.rectTransform.localScale = Vector3.one;
        initialized = true;
        SetAppearance();
        ReflectLocation();
    }

    void SetAppearance()
    {
        if (AbilityUtilities.GetShooterByID(info.abilityID) != null)
        {
            shooter.sprite = ResourceManager.GetAsset<Sprite>(AbilityUtilities.GetShooterByID(info.abilityID));
            shooter.rectTransform.sizeDelta = shooter.sprite.bounds.size * 100;
            shooter.enabled = true;
            shooter.raycastTarget = false;
        }

        // NEVER name something with "_sprite" at the end UNLESS it is a PART SPRITE!
        if (ResourceManager.Instance.resourceExists(info.partID + "_sprite"))
        {
            image.sprite = ResourceManager.GetAsset<Sprite>(info.partID + "_sprite");
            image.rectTransform.sizeDelta = image.sprite.bounds.size * 100;
            UpdateAppearance();
            image.enabled = true;
        }
        else
        {
            Debug.Log($"Invalid display part image: {info.partID}");
        }
    }

    void Start()
    {
        if (!initialized)
        {
            Initialize();
        }

        ReflectLocation();
    }

    public void ReflectLocation()
    {
        image.rectTransform.anchoredPosition = info.location * 100;
        if (shooter)
        {
            shooter.rectTransform.anchoredPosition = info.location * 100;
        }
    }

    public virtual void UpdateProperties(int faction, float opacity)
    {
        this.faction = faction;
        this.opacity = opacity;
    }

    protected virtual void UpdateAppearance()
    {
        // set colors
        image.color = SelectionDisplayHandler.AdjustColorOpacity(info.shiny ? FactionManager.GetFactionShinyColor(faction) : FactionManager.GetFactionColor(faction), opacity);
        // set position
        ReflectLocation();
        if (shooter)
        {
            shooter.color = SelectionDisplayHandler.AdjustColorOpacity(image.color, opacity);
            shooter.gameObject.transform.SetAsLastSibling();
            shooter.rectTransform.anchoredPosition = info.location * 100;
            if (AbilityUtilities.GetShooterByID(info.abilityID) == null)
            {
                Destroy(shooter.gameObject);
            }
        }

        // set rotation and flipping
        image.rectTransform.localEulerAngles = new Vector3(0, 0, info.rotation);
        image.rectTransform.localScale = new Vector3(info.mirrored ? -1 : 1, 1, 1);
    }
}
