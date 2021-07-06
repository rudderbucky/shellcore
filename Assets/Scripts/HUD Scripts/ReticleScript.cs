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
    public List<(Entity, Transform)> secondariesByObject;

    public bool DebugMode = false;

    public static ReticleScript instance;

    void Awake()
    {
    }

    void Start()
    {
        secondariesByObject = new List<(Entity, Transform)>();
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // create a ray
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, 513); // get an array of all hits
        bool droneInteraction = false;

        // This orders secondary target drones to move/follow accordingly.
        foreach (var ent in targSys.GetSecondaryTargets())
        {
            if (ent)
            {
                droneInteraction = DroneCheck(ent.transform, hits) || droneInteraction;
            }
        }

        // This orders primary target drones to move/follow accordingly.
        var primaryDroneInteraction = DroneCheck(targSys.GetTarget(), hits);
        droneInteraction = droneInteraction || primaryDroneInteraction;

        if (!primaryDroneInteraction && hits.Length != 0) // check if there are actually any hits
        {
            Draggable draggableTarget = hits[0].transform.gameObject.GetComponent<Draggable>();

            if (draggableTarget && TractorBeam.InvertTractorCheck(craft, draggableTarget) && draggableTarget.transform != craft.transform)
            {
                if (targSys.GetTarget() == draggableTarget.transform)
                {
                    PlayerCore player = craft.GetComponent<PlayerCore>();
                    player.SetTractorTarget((player.GetTractorTarget() == draggableTarget) ? null : draggableTarget);
                }

                SetTarget(draggableTarget.transform); // set the target to the clicked craft's transform
                /*
                var ent = draggableTarget.GetComponent<Entity>();
                if(Input.GetKey(KeyCode.LeftControl) && ent)
                {
                    if(!secondariesByObject.Contains((ent, draggableTarget.transform)))
                    {
                        AddSecondaryTarget(ent);
                    }
                    else
                    {
                        RemoveSecondaryTarget(ent);
                    }
                }
                else
                {
                    SetTarget(draggableTarget.transform); // set the target to the clicked craft's transform
                    if(!secondariesByObject.Contains((ent, draggableTarget.transform)))
                    {
                        RemoveSecondaryTarget(ent);
                    }
                }     
                */
                return droneInteraction; // Return so that the next check doesn't happen
            }


            ITargetable curTarg = hits[0].transform.gameObject.GetComponent<ITargetable>();
            // grab the first one's craft component, others don't matter
            if (curTarg != null && !curTarg.GetIsDead() && curTarg as Entity != craft)
                // if it is not null, dead or the player itself and is interactible
            {
                // TODO: synchronize this with the proximity script
                if (!craft.GetIsInteracting() && targSys.GetTarget() == curTarg.GetTransform()
                                              && (curTarg.GetTransform().position - craft.transform.position).sqrMagnitude < 100
                                              && (curTarg as Entity).GetInteractible()) //Interact with entity
                {
                    ProximityInteractScript.ActivateInteraction(curTarg as Entity);
                }

                SetTarget(curTarg.GetTransform()); // set the target to the clicked craft's transform
                /*
                if(Input.GetKey(KeyCode.LeftControl))
                {
                    if(!secondariesByObject.Contains((curTarg as Entity, curTarg.GetTransform())))
                    {
                        AddSecondaryTarget(curTarg as Entity);
                    }
                    else
                    {
                        RemoveSecondaryTarget(curTarg as Entity);
                    }
                }
                else
                    SetTarget(curTarg.GetTransform()); // set the target to the clicked craft's transform
                */
                return droneInteraction; // Return so that the next check doesn't happen
            }
        }

        targSys.SetTarget(null); // Nothing valid found, set target to null

        // quantityDisplay.UpdatePrimaryTargetInfo();
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

    /// <summary>
    /// Used to update the reticle representation
    /// </summary>
    private void SetTransform()
    {
        if (targSys == null)
        {
            return;
        }

        Transform target = targSys.GetTarget(); // get the target
        if (target != null)
        {
            reticleImage.rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(target.position);
            //transform.position = target.position; // update reticle position
            reticleImage.enabled = true;
        }
        else
        {
            reticleImage.enabled = false;
        }

        ITargetable targetCraft = target ? target.GetComponent<ITargetable>() : null; // if target is an entity
        UpdateReticleHealths(shellImage, coreImage, targetCraft);
    }

    public void Focus()
    {
        SetTransform();
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
                    AddSecondaryTarget(AIData.entities[i]);
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

            // Toggle tractor beam
            if (InputManager.GetKeyDown(KeyName.ToggleTractorBeam))
            {
                if (targSys.GetTarget())
                {
                    Draggable draggable = targSys.GetTarget().GetComponent<Draggable>();

                    // it's draggable if it's not an entity or it's a draggable entity with the same faction
                    if (draggable && (targSys.GetTarget().position - craft.transform.position).sqrMagnitude <= 400
                                  && (!targSys.GetTarget().GetComponent<Entity>()
                                      || targSys.GetTarget().GetComponent<Entity>().faction == craft.faction || craft.tractorSwitched))
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

    private void SetSecondaryReticleTransform(Entity ent, Transform reticle, int count)
    {
        if (ent != null && !ent.GetIsDead() && !ent.GetInvisible())
        {
            reticle.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToScreenPoint(ent.transform.position); // update reticle position

            reticle.Find("Number Marker").GetComponent<Text>().enabled = true;
            reticle.Find("Number Marker").GetComponent<Text>().text = count + "";
            reticle.Find("Number Marker").GetComponent<Text>().color = new Color32((byte)0, (byte)150, (byte)250, (byte)255);
        }
        else
        {
            RemoveSecondaryTarget((ent, reticle));
        }

        // TIL slashes allow Find searches to work like directories
        var shellImage = reticle.Find("Container/ShellImage").GetComponent<Image>();
        var coreImage = reticle.Find("Container/CoreImage").GetComponent<Image>();

        if (DebugMode)
        {
            var energyimage = reticle.Find("Container/EnergyImage").GetComponent<Image>();
            UpdateReticleHealths(shellImage, coreImage, ent, energyimage);
        }
        else
        {
            UpdateReticleHealths(shellImage, coreImage, ent);
        }
    }

    ///
    /// Checks if the passed Transform is a Drone that the player owns. If so, orders it to move/follow accordingly.
    ///
    private bool DroneCheck(Transform possibleDrone, RaycastHit2D[] hits)
    {
        var check = possibleDrone && possibleDrone.GetComponent<Drone>() &&
                    possibleDrone.GetComponent<Drone>().GetOwner() != null
                    && possibleDrone.GetComponent<Drone>().GetOwner().Equals(craft)
                    && (hits.Length == 0 || hits[0].transform != possibleDrone);
        if (check)
        {
            // Move the drone if the hit array is empty. Otherwise, if the hit array's first element is the player,
            // order a follow.
            if (hits.Length == 0 || hits[0].transform != craft.transform)
            {
                var pos = Input.mousePosition;
                pos.z = CameraScript.zLevel;
                possibleDrone.GetComponent<Drone>().CommandMovement(Camera.main.ScreenToWorldPoint(pos));
                targSys.SetTarget(null);
            }
            else if (hits[0].transform == craft.transform) // Order a follow if this passes
            {
                possibleDrone.GetComponent<AirCraftAI>().follow(craft.transform);
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

    private void UpdateReticleHealths(Image shellImage, Image coreImage, ITargetable targetCraft, Image energyImage = null)
    {
        if (targetCraft != null)
        {
            // show craft related information
            shellImage.enabled = coreImage.enabled = true;
            shellImage.color = FactionManager.GetFactionColor(targetCraft.GetFaction());
            coreImage.color = new Color(0.8F, 0.8F, 0.8F);


            float[] targHealth = targetCraft.GetHealth(); // get the target current health
            float[] targMax = targetCraft.GetMaxHealth(); // get the target max health

            shellImage.rectTransform.localScale = new Vector3(targHealth[0] / targMax[0], 1, 1);
            coreImage.rectTransform.localScale = new Vector3(targHealth[1] / targMax[1], 1, 1);

            // adjust the image scales according to the health ratios


            if (DebugMode)
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

    public void AddSecondaryTarget(Entity ent)
    {
        var success = targSys.AddSecondaryTarget(ent);
        if (success)
        {
            var reticle = Instantiate(secondaryReticlePrefab, ent.transform.position, Quaternion.identity, transform.parent);
            AdjustReticleBounds(reticle.GetComponent<Image>(), ent.transform);
            secondariesByObject.Add((ent, reticle.transform));
            //SetSecondaryReticleTransform(ent, reticle.transform, secondariesByObject.Count);
            if (!DebugMode)
            {
                quantityDisplay.AddEntityInfo(ent, this);
            }
        }
    }

    public int GetTargetIndex(Entity target)
    {
        var x = 0;
        foreach (var tuple in secondariesByObject)
        {
            if (tuple.Item1 == target)
            {
                return x;
            }

            x++;
        }

        return -1;
    }

    public void RemoveSecondaryTarget(Entity entity)
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

    public void RemoveSecondaryTarget((Entity, Transform) tuple)
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
