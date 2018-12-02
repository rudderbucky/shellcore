using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GUI Reticle to display the target of the player core
/// </summary>
public class ReticleScript : MonoBehaviour {

    public PlayerCore craft; // the player the reticle is assigned to
    private TargetingSystem targSys; // the targeting system of the player
    private bool initialized; // if the reticle has been initialized
    private Transform shellimage; // the image representations of the target's shell and core health
    private Transform coreimage;

    /// <summary>
    /// Initializes the reticle
    /// </summary>
    public void Initialize()
    {
        targSys = craft.GetTargetingSystem(); // grab the targeting system
        shellimage = transform.Find("Target Shell"); // grab the sprites
        coreimage = transform.Find("Target Core");
        initialized = true; // initialization complete
    }

    /// <summary>
    /// Finds a target to assign to the player at the given mouse position
    /// </summary>
    private void FindTarget() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // create a ray
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity); // get an array of all hits
        if (hits.Length != 0) // check if there are actually any hits
        {
            Craft craftTarget = hits[0].transform.gameObject.GetComponent<Craft>(); 
            // grab the first one's craft component, others don't matter
            if (craftTarget != null && !craftTarget.GetIsDead() /*&& target != craft*/) 
                // if it is not null, dead or the player itself
            {
                targSys.SetTarget(craftTarget.transform); // set the target to the clicked craft's transform
                Vector3 targSize = craftTarget.GetComponent<SpriteRenderer>().bounds.size * 2.5F; // adjust the size of the reticle
                float followedSize = Mathf.Max(targSize.x + 1, targSize.y + 1); // grab the maximum bounded size of the target
                transform.localScale = new Vector3(followedSize, followedSize, 1); // set the scale to match the size of the target
                return; // Return so that the next check doesn't happen
            }

            Draggable draggableTarget = hits[0].transform.gameObject.GetComponent<Draggable>();
            if (draggableTarget)
            {
                if (targSys.GetTarget() == draggableTarget.transform)
                {
                    PlayerCore player = craft.GetComponent<PlayerCore>();
                    player.SetTractorTarget((player.GetTractorTarget() == draggableTarget) ? null : draggableTarget);
                }

                targSys.SetTarget(draggableTarget.transform); // set the target to the clicked craft's transform
                Vector3 targSize = draggableTarget.GetComponent<SpriteRenderer>().bounds.size * 2.5F; // adjust the size of the reticle
                float followedSize = Mathf.Max(targSize.x + 1, targSize.y + 1); // grab the maximum bounded size of the target
                transform.localScale = new Vector3(followedSize, followedSize, 1); // set the scale to match the size of the target
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

            Craft targetCraft = target.GetComponent<Craft>(); // if target is a craft
            if(targetCraft)
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
            if (Input.GetMouseButtonDown(0)) // mouse click, scan for target
            {
                FindTarget(); // find target
            }
            else if (targSys.GetTarget() != null) // check if the reticle should update
            {
                Craft targetCraft = targSys.GetTarget().GetComponent<Craft>();

                if (targetCraft && targetCraft.GetIsDead()) { 
                    // check if the target craft is dead
                    targSys.SetTarget(null); // if so remove the target lock
                }
            }
            SetTransform(); // update the transform of the reticle accordingly

            // Toggle tractor beam
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(targSys.GetTarget())
                {
                    Draggable draggable = targSys.GetTarget().GetComponent<Draggable>();
                    if(draggable)
                    {
                        craft.SetTractorTarget((craft.GetTractorTarget() == draggable) ? null : draggable);
                    }
                }
            }
        }
	}
}
