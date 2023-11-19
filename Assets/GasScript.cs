using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasScript : MonoBehaviour
{
    public float orbitalZ;
    public float angularVelocity;
    public float radial;
    public int emissionPerSecond = 10;
    public float radius;
    public float multiplier;
    public float arc = 0.3333f;
    public ParticleSystem partSys;
    public ParticleSystem auxillaryPartSys;
    
    // Start is called before the first frame update
    void Start()
    {
        arc =  Random.Range(2, 9);
        angularVelocity = Random.Range(-4f, -64f);
        radius = Random.Range(5F, 10F);
        if (SectorManager.instance.current.type == Sector.SectorType.DangerZone)
            radius *= 5;

        GetComponent<Rigidbody2D>().angularDrag = 0;
        GetComponent<Rigidbody2D>().angularVelocity = angularVelocity * multiplier;
        var main = partSys.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f * radius);
        var velo = partSys.velocityOverLifetime;
        velo.orbitalZ = orbitalZ * Mathf.Sqrt(multiplier);
        velo.radial = radial * multiplier;
        var emission = partSys.emission;
        var shape = partSys.shape;
        shape.radius = radius * multiplier;
        
        var shape2 = auxillaryPartSys.shape;
        
        shape2.radius = shape.radius;
        var main2 = auxillaryPartSys.main;
        velo = auxillaryPartSys.velocityOverLifetime;
        velo.radial = -shape.radius;
        main2.startLifetime = 1;

        shape.arcSpread = 1 / arc;
        emission.rateOverTime = 0 * emissionPerSecond * Mathf.Sqrt(multiplier);
        partSys.Stop();
        partSys.Clear();
        partSys.Simulate(partSys.main.duration);
        partSys.Play();
        
        auxillaryPartSys.Stop();
        auxillaryPartSys.Clear();
        auxillaryPartSys.Simulate(auxillaryPartSys.main.duration);
        auxillaryPartSys.Play();
        AIData.gas.Add(this);
    }

    public void Shrink(float val)
    {
        radius -= val;
        var main = partSys.main;
        main.startLifetime = 0.3f * radius; 
        var shape = partSys.shape;
        shape.radius = radius * multiplier;
        var shape2 = auxillaryPartSys.shape;
        shape2.radius = shape.radius;
        var velo = auxillaryPartSys.velocityOverLifetime;
        velo.radial = -shape.radius;
    }

    private void OnDestroy()
    {
        AIData.gas.Remove(this);
    }

    void Update()
    {

    }
}
