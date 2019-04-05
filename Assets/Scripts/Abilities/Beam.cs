using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : WeaponAbility {

    public LineRenderer line; // line renderer of the beam
    private Material material; // material used by the line renderer
    private bool firing; // check for line renderer drawing
    private float timer; // float timer for line renderer drawing
    private float damage = 500;

    protected override void Awake()
    {
        // set instance fields 
        base.Awake();
        abilityName = "Beam";
        description = "Instant attack that deals " + damage + " damage.";
        line = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
        line.sortingLayerName = "Projectiles";
        line.material = material;
        line.startWidth = line.endWidth = 0.15F;
        line.endColor = new Color(0.8F,0.8F,1,0.9F);
        line.startColor = new Color(0.5F, 0.5F, 1, 0.9F);
        cooldownDuration = CDRemaining = 5;
        energyCost = 20;
        ID = 4;
        range = 15;
        category = Entity.EntityCategory.All;
    }
    protected virtual void Start() {
        damage *= abilityTier; // Thanks Abnormalities for finding this bug!
        SetMaterial(ResourceManager.GetAsset<Material>("white_material"));
    }
    public void SetMaterial(Material material)
    {
        this.material = material;
        line.material = material;
    }

    void Update()
    {
        if (firing && timer < 0.2F) // timer for drawing the beam, past the set timer float value and it stops being drawn
        {
            line.SetPosition(0, line.transform.position); // draw and increment timer
            line.SetPosition(1, targetingSystem.GetTarget().position);
            timer += Time.deltaTime;
        }
        else if(firing && timer >= 0.2)
        {
            if(line.positionCount > 0 && ((line.GetPosition(1)-line.transform.position).sqrMagnitude 
                > (line.GetPosition(0)-line.transform.position).sqrMagnitude)) {
                line.SetPosition(0, line.GetPosition(0) + (line.GetPosition(1)-line.GetPosition(0)).normalized * 2); 
                if((line.GetPosition(0)-line.transform.position).sqrMagnitude 
                    > (line.GetPosition(1)-line.transform.position).sqrMagnitude) {
                    line.SetPosition(0, line.GetPosition(1));
                }
                if(targetingSystem.GetTarget()) line.SetPosition(1, targetingSystem.GetTarget().position);
            }
            else line.positionCount = 0;
        } else 
        {
            line.positionCount = 0;
            firing = false;
        }
    }

    protected override bool Execute(Vector3 victimPos)
    {
        if (targetingSystem.GetTarget()) // check and get the weapon target
        {
            ResourceManager.PlayClipByID("clip_beam", transform.position);
            targetingSystem.GetTarget().GetComponent<Entity>().TakeDamage(damage, 0, GetComponentInParent<Entity>()); // deal instant damage
            line.positionCount = 2; // render the beam line
            timer = 0; // start the timer
            isOnCD = true; // set booleans and return
            firing = true;
            return true;
        }
        return false;
    }
}
