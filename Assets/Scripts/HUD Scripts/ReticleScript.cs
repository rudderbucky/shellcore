using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// GUI Reticle to display the target of the player core
/// </summary>
public class ReticleScript : MonoBehaviour {

    private PlayerCore craft; // the player the reticle is assigned to
    private TargetingSystem targSys; // the targeting system of the player
    private bool initialized; // if the reticle has been initialized
    private Transform shellimage; // the image representations of the target's shell and core health
    private Transform coreimage;
    public EventSystem system;

    /// <summary>
    /// Initializes the reticle
    /// </summary>
    public void Initialize(PlayerCore player)
    {
        craft = player;
        targSys = craft.GetTargetingSystem(); // grab the targeting system
        shellimage = transform.Find("Target Shell"); // grab the sprites
        coreimage = transform.Find("Target Core");
        initialized = true; // initialization complete
    }

    /// <summary>
    /// Finds a target to assign to the player at the given mouse position
    /// </summary>
    private void FindTarget() {

        // To say this needs despaghettification would be an understatement...

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // create a ray
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity); // get an array of all hits

        // TODO: add an explicit owner check in IOwnable as well as GetOwner()

        if (targSys.GetTarget() && targSys.GetTarget().GetComponent<Drone>() && targSys.GetTarget().GetComponent<Drone>().GetOwner().Equals(craft)
            && (hits.Length == 0 || hits[0].transform != targSys.GetTarget()))
        {
            if (hits.Length == 0 ||  hits[0].transform != craft.transform) {
                var pos = Input.mousePosition;
                pos.z = 10;
                targSys.GetTarget().GetComponent<Drone>().CommandMovement(Camera.main.ScreenToWorldPoint(pos));
                targSys.SetTarget(null);
            } else if (hits[0].transform == craft.transform)
            {
                targSys.GetTarget().GetComponent<AirCraftAI>().follow(craft.transform);
                targSys.SetTarget(null);
            }
        }
        else if (hits.Length != 0) // check if there are actually any hits
        {
            Draggable draggableTarget = hits[0].transform.gameObject.GetComponent<Draggable>();

            if (draggableTarget)
            {
                if (targSys.GetTarget() == draggableTarget.transform 
                && (!targSys.GetTarget().GetComponent<Entity>() 
                || targSys.GetTarget().GetComponent<Entity>().faction == craft.faction))
                {
                    PlayerCore player = craft.GetComponent<PlayerCore>();
                    player.SetTractorTarget((player.GetTractorTarget() == draggableTarget) ? null : draggableTarget);
                }
                targSys.SetTarget(draggableTarget.transform); // set the target to the clicked craft's transform
                Vector3 targSize = draggableTarget.GetComponent<SpriteRenderer>().bounds.size; //+ Vector3.one * 5; // adjust the size of the reticle
                float followedSize = Mathf.Max(targSize.x + 1, targSize.y + 1); // grab the maximum bounded size of the target
                GetComponent<SpriteRenderer>().size = new Vector2(followedSize, followedSize); // set the scale to match the size of the target
                return; // Return so that the next check doesn't happen
            }


            Entity entityTarget = hits[0].transform.gameObject.GetComponent<Entity>();
            // grab the first one's craft component, others don't matter
            if (entityTarget != null && !entityTarget.GetIsDead() && entityTarget != craft) 
                // if it is not null, dead or the player itself
            {
                if (targSys.GetTarget() == entityTarget.transform) //Interact with entity
                {
                    if (entityTarget.dialogue as Dialogue)
                        DialogueSystem.StartDialogue(entityTarget.dialogue as Dialogue);
                    else if(entityTarget as IVendor != null && entityTarget.faction == craft.faction)
                    {
                        VendorUI outpostUI = transform.parent.Find("Dialogue").GetComponent<VendorUI>();
                        outpostUI.blueprint = (entityTarget as IVendor).GetVendingBlueprint();
                        if ((entityTarget.transform.position - craft.transform.position).magnitude < outpostUI.blueprint.range)
                        {
                            outpostUI.outpostPosition = entityTarget.transform.position;
                            outpostUI.player = craft;
                            outpostUI.openUI();
                        }
                    }
                }

                targSys.SetTarget(entityTarget.transform); // set the target to the clicked craft's transform
                Vector3 targSize = entityTarget.GetComponent<SpriteRenderer>().bounds.size * 2.5F; // adjust the size of the reticle
                float followedSize = Mathf.Max(targSize.x + 1, targSize.y + 1); // grab the maximum bounded size of the target
                GetComponent<SpriteRenderer>().size = new Vector2(followedSize, followedSize); // set the scale to match the size of the target
                return; // Return so that the next check doesn't happen
            }
            targSys.SetTarget(null); // otherwise set the target to null
        }
        else {
            targSys.SetTarget(null); // otherwise set the target to null
        }
    }
    
    /// <summary>
    /// Used to update the reticle representation
    /// </summary>
    private void SetTransform() {
        
        if (targSys.GetTarget() != null) // no point in updating if it is null
        {
            Transform target = targSys.GetTarget(); // get the target
            transform.position = target.position; // update reticle position
            GetComponent<SpriteRenderer>().enabled = true; // enable the sprite renderers

            Entity targetCraft = target.GetComponent<Entity>(); // if target is an entity
            if (targetCraft)
            {
                // show craft related information

                shellimage.GetComponentInChildren<SpriteRenderer>().enabled = true;
                coreimage.GetComponentInChildren<SpriteRenderer>().enabled = true;

                float[] targHealth = targetCraft.GetHealth(); // get the target current health
                float[] targMax = targetCraft.GetMaxHealth(); // get the target max health

                // adjust the image scales according to the health ratios
                Vector3 scale = shellimage.localScale;

                scale.x = targHealth[0] / targMax[0];

                shellimage.localScale = scale;

                scale = coreimage.localScale;
                scale.x = targHealth[1] / targMax[1];

                coreimage.localScale = scale;
            }
            else
            {
                // disable the craft related info
                shellimage.GetComponentInChildren<SpriteRenderer>().enabled = false;
                coreimage.GetComponentInChildren<SpriteRenderer>().enabled = false;
            }
        }
        else
        {
            // disable the sprite renderers
            shellimage.GetComponentInChildren<SpriteRenderer>().enabled = false;
            coreimage.GetComponentInChildren<SpriteRenderer>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
	// Update is called once per frame
	void Update () {
        if (initialized) // check if it is safe to update
        {
            if (Input.GetMouseButtonDown(0) && !system.IsPointerOverGameObject()) // mouse click, scan for target
            {
                FindTarget(); // find target
            }
            if (targSys.GetTarget() != null) // check if the reticle should update
            {
                Entity targetCraft = targSys.GetTarget().GetComponent<Entity>();

                if (targetCraft && targetCraft.GetIsDead()) { 
                    // check if the target craft is dead
                    targSys.SetTarget(null); // if so remove the target lock
                }
            }
            SetTransform(); // update the transform of the reticle accordingly

            // Toggle tractor beam
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (targSys.GetTarget())
                {
                    Draggable draggable = targSys.GetTarget().GetComponent<Draggable>();

                    // it's draggable if it's not an entity or it's a draggable entity with the same faction
                    if (draggable 
                    && (!targSys.GetTarget().GetComponent<Entity>() 
                    || targSys.GetTarget().GetComponent<Entity>().faction == craft.faction))
                    {
                        craft.SetTractorTarget((craft.GetTractorTarget() == draggable) ? null : draggable);
                    } else craft.SetTractorTarget(null);
                }
                else craft.SetTractorTarget(null);
            }
        }
	}
}
