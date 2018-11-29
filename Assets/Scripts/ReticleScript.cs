using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleScript : MonoBehaviour {

    public Craft craft;
    private TargetingSystem targSys;
    private bool initialized;
    private Transform shellimage;
    private Transform coreimage;

    public void Initialize()
    {
        targSys = craft.GetTargetingSystem();
        shellimage = transform.Find("Target Shell");
        coreimage = transform.Find("Target Core");
        initialized = true;
    }

    private void FindTarget() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
        if (hits.Length != 0)
        {
            Craft target = hits[0].transform.gameObject.GetComponent<Craft>();
            if (target != null && !target.GetIsDead() && target != craft)
            {
                targSys.SetTarget(target.transform);
                Vector3 targSize = target.GetComponent<SpriteRenderer>().bounds.size * 2.5F;
                float followedSize = Mathf.Max(targSize.x + 1, targSize.y + 1);
                transform.localScale = new Vector3(followedSize, followedSize, 1);
            }
            else targSys.SetTarget(null);
        }
        else {
            targSys.SetTarget(null);
        }
    }

    private void SetTransform() {
        
        if (targSys.GetTarget() != null)
        {
            Transform target = targSys.GetTarget();
            transform.position = target.position;
            GetComponent<SpriteRenderer>().enabled = true;
            shellimage.GetComponentInChildren<SpriteRenderer>().enabled = true;
            coreimage.GetComponentInChildren<SpriteRenderer>().enabled = true;
            float[] targHealth = target.GetComponent<Craft>().GetHealth();
            float[] targMax = target.GetComponent<Craft>().GetMaxHealth();

            Vector3 scale = shellimage.localScale;
            scale.x = targHealth[0] / targMax[0];

            shellimage.localScale = scale;

            scale = coreimage.localScale;
            scale.x = targHealth[1] / targMax[1];

            coreimage.localScale = scale;
        }
        else
        {
            shellimage.GetComponentInChildren<SpriteRenderer>().enabled = false;
            coreimage.GetComponentInChildren<SpriteRenderer>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
	// Update is called once per frame
	void Update () {
        if (initialized)
        {
            if (Input.GetMouseButtonDown(0))
            {
                FindTarget();
            }
            else if (targSys.GetTarget() != null)
            {
                if (targSys.GetTarget().GetComponent<Craft>().GetIsDead()) {
                    targSys.SetTarget(null);
                }
            }
            SetTransform();
        }
	}
}
