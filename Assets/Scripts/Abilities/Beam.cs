using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : WeaponAbility {

    public LineRenderer line; // line renderer of the beam
    public Material material; // material used by the line renderer
    private bool firing; // check for line renderer drawing
    private float timer; // float timer for line renderer drawing
    private Vector3 victimPos; // second position to render the beam to

    protected override void Awake()
    {
        base.Awake(); 
        // set instance fields (values hardcoded, this may change to being modular)
        line = GetComponent<LineRenderer>() ? GetComponent<LineRenderer>() : gameObject.AddComponent<LineRenderer>();
        line.sortingLayerName = "Projectiles";
        line.material = material;
        line.startWidth = line.endWidth = 0.2F;
        cooldownDuration = CDRemaining = 5;
        energyCost = 20;
        ID = 4;
        range = 25;
        category = Entity.EntityCategory.All;
    }

    void Update()
    {
        if (firing && timer < 0.2F) // timer for drawing the beam, past the set timer float value and it stops being drawn
        {
            line.SetPosition(0, line.transform.position); // draw and increment timer
            line.SetPosition(1, victimPos);
            timer += Time.deltaTime;
        }
        else
        {
            firing = false; // reset drawing
            line.positionCount = 0;
        }
    }

    protected override bool Execute(Vector3 victimPos)
    {
        if (targetingSystem.GetTarget()) // check and get the weapon target
        {
            targetingSystem.GetTarget().GetComponent<Entity>().TakeDamage(200, 0); // deal instant damage
            line.positionCount = 2; // render the beam line
            this.victimPos = victimPos; // set the position to render the line to
            timer = 0; // start the timer
            isOnCD = true; // set booleans and return
            firing = true;
            return true;
        }
        return false;
    }
}
