using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleEffectScript : MonoBehaviour {

    public ParticleSystem partSys; // particle system that stores the particles
    private Vector3 displacement; // used to wrap particles around
    
    // Use this for initialization
    void Start () {
        // the single most cancerous thing I've ever written including all my English essays
        Vector3 dimensions = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, partSys.shape.position.z - Camera.main.transform.position.z));
        // gets the dimensions of the screen the game is playing on in x and y values
        var sh = partSys.shape;
        sh.scale = new Vector3(dimensions[1]*2, dimensions[0]*2, 0); // WHY *2?????????
        // scales up the emitters so that particles are uniformly emitted across the screen, I suspect the scale is halved for some reason
        partSys.Emit(25);
    }

    /// <summary>
    /// Updater for the particles in the field, wraps them around if necessary
    /// </summary>
    /// <param name="particles">array of particles in the field</param>
    private void particleUpdate(ParticleSystem.Particle[] particles) {
        for (int i = 0; i < particles.Length; i++) // update all particles
        {
            particleWrapper(ref particles[i], 0); // call particle wrapper for both dimensions
            particleWrapper(ref particles[i], 1);

            // READ: when I just used a Particle as an input for particleWrapper and changed its position, it didn't update the particle in the array.
            // even though it should've given particleWrapper the reference to the same particle.
            // This could be due to some kind of immutability that Particles have, or it could also be due to a sort of "delinking" between array
            // and particle (they are no longer both referenced in the same method) that doesn't make sense to me.
            // This must be CHILEAN JAVA
            // THE ANSWER: Particles are structs, they DO NOT implicitly pass by reference, you have to EXPLICITLY use the keyword ref or out.
            // out will force you to assign a value to the argument in the method, ref will force you to pass in a non-null value.
            // because they do not pass by reference (instead by value; passing it without a keyword creates an entirely new struct) the values didn't change
            // unless I returned, and since I was directly changing a value inside this method (working with a reference to the same struct in the for loops)
            // it worked in here.
        }
    }

    /// <summary>
    /// particleUpdate helper method: wraps the particles to the other side of the screen if they are not in the correct bounds
    /// </summary>
    /// <param name="particle">the particle to wrap</param>
    /// <param name="dimension">the dimension (0 is x, 1 is y)</param>
    private void particleWrapper(ref ParticleSystem.Particle particle, int dimension)
    {
        float limit;
        switch (dimension)
        {
            case 0:
                limit = Camera.main.pixelWidth; // x axis
                break;
            case 1:
                limit = Camera.main.pixelHeight; // y axis
                break;
            default: // not supposed to happen lol, too lazy to learn C# exception handling
                limit = 0;
                break;
        }
        Vector3 relativeCameraPos = Camera.main.WorldToScreenPoint(particle.position);
        if (relativeCameraPos[dimension] < 0 || relativeCameraPos[dimension] > limit) {
            displacement = relativeCameraPos;
            displacement[dimension] = Mathf.Abs(displacement[dimension] - limit);
            particle.position = Camera.main.ScreenToWorldPoint(displacement);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[25]; // constantly update particle array, room for optimization here
            partSys.GetParticles(particles); // get particles
            particleUpdate(particles);
            partSys.SetParticles(particles, 25); // set particles
    }
}
