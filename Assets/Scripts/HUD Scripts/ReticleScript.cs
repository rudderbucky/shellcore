using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GUI Reticle to display the target of the player core
/// </summary>
public class ReticleScript : MonoBehaviour
{
    private PlayerCore craft; // the player the reticle is assigned to
    private TargetingSystem targSys; // the targeting system of the player
    private bool initialized; // if the reticle has been initialized

    [SerializeField]
    private Image reticleImage;

    [SerializeField]
    private Image shellImage;

    [SerializeField]
    private Image coreImage;

    public QuantityDisplayScript quantityDisplay;
    public GameObject secondaryReticlePrefab;
    public List<(Transform, Transform)> secondariesByObject;

    public bool DebugMode = false;

    public static ReticleScript instance;

    void Awake()
    {
    }

    void Start()
    {
        secondariesByObject = new List<(Transform, Transform)>();
    }

    /// <summary>
    /// Initializes the reticle
    /// </summary>
    public void Initialize(PlayerCore player)
    {
        craft = player;
        targSys = craft.GetTargetingSystem(); // grab the targeting system
        initialized = true; // initialization complete
        instance = this;
    }

    /// <summary>
    /// Finds a target to assign to the player at the given mouse position
    /// Returns whether there was a drone interaction or not
    /// </summary>
    public bool FindTarget()
    {
        // TODO: To say this needs despaghettification would be an understatement...
        // despaghettified a little :)
        /*
         * IInteractable
         * - bool Interact() - returns whether the interaction was successful
         * - int getPriority() - returns interaction priority, for example: 
         *   a component with quest dialogue would return high priority, while 
         *   something that acts only as a target would return low priority
         * 
         * 
         * This method:
         *   Find all interactable objects in the pointer position, add to 'List'
         *   compare GameObjects int List to those found in quest interaction overrides list
         *      if there's a match : 
         *          call the quest node's Calculate()
         *          return;
         *   Sort List by priority
         *   Foreach (IInteractable interactable in List)
         *      if(interactable.Interact())
         *         break;
         */

        if (!Camera.main)
        {
            return false;
        }

        var mousePos = Input.mousePosition;
        mousePos.z = CameraScript.zLevel;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        var hits = CollisionManager.GetAllTargetsAtPosition(mouseWorldPos);

        bool droneInteraction = false;

        // This orders secondary target drones to move/follow accordingly.
        if (targSys == null)
        {
            return false;
        }
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        var primaryDroneInteraction = false;
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            foreach (var ent in targSys.GetSecondaryTargets())
            {
                if (ent != null && !ent.Equals(null) && ent)
                {
                    droneInteraction = DroneCheck(ent, hits, mousePos) || droneInteraction;
                }
            }

            // This orders primary target drones to move/follow accordingly.
            primaryDroneInteraction = DroneCheck(targSys.GetTarget(), hits, mousePos);
            droneInteraction = droneInteraction || primaryDroneInteraction;

            
        }
        if (primaryDroneInteraction || hits.Length == 0) // check if there are actually any hits
        {
            targSys.SetTarget(null); // Nothing valid found, set target to null
            return droneInteraction;
        }

        Draggable draggableTarget = null;
        for (int i = 0; i < hits.Length; i++)
        {
            draggableTarget = hits[i]?.gameObject.GetComponent<Draggable>();
            if (hits[i]?.gameObject.GetComponent<IVendor>() == null) break;
        }

        if (draggableTarget && TractorBeam.InvertTractorCheck(craft, draggableTarget) && draggableTarget.transform != craft.transform)
        {
            if (targSys.GetTarget() == draggableTarget.transform)
            {
                PlayerCore player = craft.GetComponent<PlayerCore>();
                if (player)
                {
                    player.SetTractorTarget((player.GetTractorTarget() == draggableTarget) ? null : draggableTarget);
                }
            }

            SetTarget(draggableTarget.transform); // set the target to the clicked craft's transform

            return droneInteraction; // Return so that the next check doesn't happen
        }


        ITargetable curTarg = hits[0]?.gameObject.GetComponent<ITargetable>();
        // grab the first one's craft component, others don't matter
        if (curTarg != null && !curTarg.GetIsDead() && curTarg as Entity != craft)
        // if it is not null, dead or the player itself and is interactible
        {
            // TODO: synchronize this with the proximity script
            if (curTarg as Entity && !craft.GetIsInteracting() && targSys.GetTarget() == curTarg.GetTransform()
                                            && (curTarg.GetTransform().position - craft.transform.position).sqrMagnitude < 100
                                            && (curTarg as Entity).GetInteractible()) //Interact with entity
            {
                ProximityInteractScript.ActivateInteraction(curTarg as Entity);
            }

            SetTarget(curTarg.GetTransform()); // set the target to the clicked craft's transform

            return droneInteraction; // Return so that the next check doesn't happen
        }

        return droneInteraction;
    }

    public void SetTarget(Transform target)
    {
        targSys.SetTarget(target); // set the target to the transform
        if (target)
        {
            AdjustReticleBounds(reticleImage, target);
        }
    }

    private float expandRate = 10F;
    /// <summary>
    /// Used to update the reticle representation
    /// </summary>
    private Transform lastMainTarget;
    private void SetTransform()
    {
        if (targSys == null)
        {
            return;
        }

        Transform target = targSys.GetTarget(); // get the target
        if (target != lastMainTarget) {
            reticleImage.transform.localScale = new Vector3(0, 0, 1);
            lastMainTarget = target;
        }
        if (target != null)
        {
            reticleImage.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(target.position);
            reticleImage.rectTransform.anchoredPosition *= UIScalerScript.GetScale();
            var x = reticleImage.transform.localScale;
            x.x = Mathf.Min(x.x + expandRate * Time.deltaTime, 1);
            x.y = Mathf.Min(x.y + expandRate * Time.deltaTime, 1);
            reticleImage.transform.localScale = x;
            reticleImage.enabled = true;
        }
        else
        {
            reticleImage.enabled = false;
        }

        UpdateReticleHealths(shellImage, coreImage, target);
    }

    public void Focus()
    {
        SetTransform();
        var index = 0;
        while (index < secondariesByObject.Count)
        {
            var oldCount = secondariesByObject.Count;
            SetSecondaryReticleTransform(secondariesByObject[index].Item1, secondariesByObject[index].Item2, index + 1);
            if (index < secondariesByObject.Count)
            {
                var x = secondariesByObject[index].Item2.localScale;
                x.x = Mathf.Min(x.x + expandRate * Time.deltaTime, 1);
                x.y = Mathf.Min(x.y + expandRate * Time.deltaTime, 1);
                secondariesByObject[index].Item2.localScale = x;
            }

            if (oldCount == secondariesByObject.Count)
            {
                index++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (initialized) // check if it is safe to update
        {
            if (DebugMode)
            {
                for (int i = 0; i < AIData.entities.Count; i++)
                {
                    if (!AIData.entities[i]) continue;
                    AddSecondaryTarget(AIData.entities[i].transform);
                }
            }

            var index = 0;
            while (index < secondariesByObject.Count)
            {
                var oldCount = secondariesByObject.Count;
                SetSecondaryReticleTransform(secondariesByObject[index].Item1, secondariesByObject[index].Item2, index + 1);
                if (oldCount == secondariesByObject.Count)
                {
                    index++;
                }
            }

            if (targSys.GetTarget() != null) // check if the reticle should update
            {
                ITargetable targetCraft = targSys.GetTarget().GetComponent<ITargetable>();

                if (targetCraft != null && (targetCraft.GetIsDead() || targetCraft.GetInvisible()))
                {
                    // check if the target craft is dead or invisible
                    targSys.SetTarget(null); // if so remove the target lock
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
            {
                SetTarget(null);
                ClearSecondaryTargets();
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab) && (craft is PlayerCore player))
            {
                SetTarget(null);
                ClearSecondaryTargets();
                Debug.LogWarning("test");
                foreach (var ownable in player.GetUnitsCommanding())
                {
                    if (ownable == null || ownable.Equals(null) || !(ownable is Entity ent) || ent.GetIsDead()) continue;
                    AddSecondaryTarget(ent.transform);
                }
            }


            // Toggle tractor beam
            if (InputManager.GetKeyDown(KeyName.ToggleTractorBeam))
            {
                if (targSys.GetTarget())
                {
                    Draggable draggable = targSys.GetTarget().GetComponent<Draggable>();

                    // it's draggable if it's not an entity or it's a draggable entity with the same faction
                    if (draggable && (targSys.GetTarget().position - craft.transform.position).sqrMagnitude <= 400
                                  && (!targSys.GetTarget().GetComponent<Entity>()
                                      || FactionManager.IsAllied(targSys.GetTarget().GetComponent<Entity>().faction, craft.faction) || craft.tractorSwitched))
                    {
                        craft.SetTractorTarget((craft.GetTractorTarget() == draggable) ? null : draggable);
                    }
                    else
                    {
                        craft.SetTractorTarget(null);
                    }
                }
                else
                {
                    craft.SetTractorTarget(null);
                }
            }
        }
    }

    private void SetSecondaryReticleTransform(Transform target, Transform reticle, int count)
    {
        if (target != null && (!(target.GetComponent<Entity>()) || (!target.GetComponent<Entity>().GetIsDead() && !target.GetComponent<Entity>().GetInvisible())))
        {
            reticle.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(target.position); // update reticle position
            reticle.GetComponent<RectTransform>().anchoredPosition *= UIScalerScript.GetScale();
            reticle.Find("Number Marker").GetComponent<Text>().enabled = true;
            reticle.Find("Number Marker").GetComponent<Text>().text = count + "";
            reticle.Find("Number Marker").GetComponent<Text>().color = new Color32(0, 150, 250, 255);
        }
        else
        {
            RemoveSecondaryTarget((target, reticle));
        }

        // TIL slashes allow Find searches to work like directories
        var shellImage = reticle.Find("Container/ShellImage").GetComponent<Image>();
        var coreImage = reticle.Find("Container/CoreImage").GetComponent<Image>();

        if (DebugMode)
        {
            var energyimage = reticle.Find("Container/EnergyImage").GetComponent<Image>();
            UpdateReticleHealths(shellImage, coreImage, target, energyimage);
        }
        else
        {
            UpdateReticleHealths(shellImage, coreImage, target);
        }
    }

    ///
    /// Checks if the passed Transform is a Drone that the player owns. If so, orders it to move/follow accordingly.
    ///
    public bool DroneCheck(Transform possibleDrone, Transform[] hits, Vector3 worldMovementVector)
    {
        var check = possibleDrone && possibleDrone.GetComponent<Drone>() &&
                    possibleDrone.GetComponent<Drone>().GetOwner() != null
                    && possibleDrone.GetComponent<Drone>().GetOwner().Equals(craft)
                    && (hits == null || hits.Length == 0 || hits[0] != possibleDrone);
        if (check)
        {
            // Move the drone if the hit array is empty. Otherwise, if the hit array's first element is the player,
            // order a follow.
            if (hits == null || hits.Length == 0 || hits[0] != craft.transform)
            {
                possibleDrone.GetComponent<Drone>().CommandMovement(worldMovementVector);
                targSys.SetTarget(null);
            }
            else if (hits[0] == craft.transform) // Order a follow if this passes
            {
                possibleDrone.GetComponent<Drone>().CommandFollowOwner();
                targSys.SetTarget(null);
            }
        }

        return check;
    }

    private void AdjustReticleBounds(Image image, Transform ent)
    {
        Vector3 targSize = ent.GetComponent<SpriteRenderer>().bounds.size; // adjust the size of the reticle
        float followedSize = Mathf.Max(targSize.x + 1.5F, targSize.y + 1.5F); // grab the maximum bounded size of the target
        image.rectTransform.sizeDelta = Vector2.one * followedSize * 33;
        if (transform.Find("Number Marker"))
        {
            transform.Find("Number Marker").localPosition = new Vector3(followedSize / 2 + 0.1F, followedSize / 2 + 0.05F, 0);
        }
    }

    private void UpdateReticleHealths(Image shellImage, Image coreImage, Transform targetCraft, Image energyImage = null)
    {
        if (targetCraft != null && targetCraft.GetComponent<Entity>())
        {
            var ent = targetCraft.GetComponent<Entity>();

            // show craft related information
            shellImage.enabled = coreImage.enabled = true;
            shellImage.color = FactionManager.GetFactionColor(ent.GetFaction());
            coreImage.color = new Color(0.8F, 0.8F, 0.8F);


            float[] targHealth = ent.GetHealth(); // get the target current health
            float[] targMax = ent.GetMaxHealth(); // get the target max health

            shellImage.rectTransform.localScale = new Vector3(targHealth[0] / targMax[0], 1, 1);
            coreImage.rectTransform.localScale = new Vector3(targHealth[1] / targMax[1], 1, 1);

            // adjust the image scales according to the health ratios

            // Warning: does not account for the shell/core/energy number objects not on primary reticle
            if (DebugMode && energyImage)
            {
                energyImage.enabled = true;
                var parent = coreImage.transform.parent.parent;
                var texts = parent.GetComponentsInChildren<Text>();
                foreach (Text t in texts)
                {
                    t.enabled = true;
                }


                parent.Find("Shell Number").GetComponentInChildren<Text>().text = Mathf.Round(targHealth[0]) + "/" + targMax[0];
                parent.Find("Core Number").GetComponentInChildren<Text>().text = Mathf.Round(targHealth[1]) + "/" + targMax[1];
                parent.Find("Energy Number").GetComponentInChildren<Text>().text = Mathf.Round(targHealth[2]) + "/" + targMax[2];
            }
        }
        else
        {
            // disable the craft related info
            shellImage.enabled = coreImage.enabled = false;
            if (energyImage)
            {
                energyImage.enabled = false;
            }
        }
    }

    public void AddSecondaryTarget(Transform ent)
    {
        var success = targSys.AddSecondaryTarget(ent);
        if (success)
        {
            var reticle = Instantiate(secondaryReticlePrefab, ent.position, Quaternion.identity, transform.parent);
            reticle.transform.localScale = new Vector3(0, 0, 1);
            AdjustReticleBounds(reticle.GetComponent<Image>(), ent);
            secondariesByObject.Add((ent, reticle.transform));
            //SetSecondaryReticleTransform(ent, reticle.transform, secondariesByObject.Count);
            if (!DebugMode)
            {
                quantityDisplay.AddSecondaryInfo(ent, this);
            }
        }
    }

    public int GetTargetIndex(Transform target)
    {
        var x = 0;
        foreach (var tuple in secondariesByObject)
        {
            if (tuple.Item1 && tuple.Item1 == target)
            {
                return x;
            }

            x++;
        }

        return -1;
    }

    public void RemoveSecondaryTarget(Transform entity)
    {
        foreach (var secondary in secondariesByObject)
        {
            if (secondary.Item1 == entity)
            {
                RemoveSecondaryTarget(secondary);
                break;
            }
        }
    }

    public void RemoveSecondaryTarget((Transform, Transform) tuple)
    {
        if (secondariesByObject.Contains(tuple))
        {
            secondariesByObject.Remove(tuple);
        }

        targSys.RemoveSecondaryTarget(tuple.Item1);
        quantityDisplay.RemoveEntityInfo(tuple.Item1);
        Destroy(tuple.Item2.gameObject);
    }

    public void ClearSecondaryTargets()
    {
        while (secondariesByObject.Count > 0)
        {
            RemoveSecondaryTarget(secondariesByObject[0]);
        }

        targSys.ClearSecondaryTargets();
    }
}
