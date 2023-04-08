using System.Collections.Generic;
using UnityEngine;

public enum RectangleEffectSkin
{
    Squares,
    Crosses,
    Triangles
}

/// <summary>
/// Rectangle effect script used to create the cool rectangles
/// </summary>
public class RectangleEffectScript : MonoBehaviour
{
    public ParticleSystem partSys; // particle system that stores the particles
    private Vector3 displacement; // used to wrap particles around
    public static bool active = true;
    bool built = false;
    public static RectangleEffectSkin currentSkin = RectangleEffectSkin.Squares;
    public static List<RectangleEffectScript> instances = new List<RectangleEffectScript>();

    void Awake()
    {
        active = PlayerPrefs.GetString("RectangleEffectScript_active", "True") == "True";
        instances = instances.FindAll((i) => i.partSys);
        instances.Add(this);
    }

    public static void SetActive(bool act)
    {
        active = act;
    }

    private Rect pixelRect;

    // Use this for initialization
    private void Build()
    {
        
        pixelRect = Camera.main.pixelRect;
        partSys.Clear();
        timesByParticle.Clear();
        var rd = partSys.GetComponentInChildren<ParticleSystemRenderer>().material =
            ResourceManager.GetAsset<Material>($"RectangleEffectScript_skin{(int)currentSkin}");
        var sh = partSys.shape; // grab the shape of the particle system

        Vector3 dimensions = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth,
            Camera.main.pixelHeight, sh.position.z + CameraScript.GetMaxZoomLevel()));
        Vector3 pos = Camera.main.transform.position;
        pos.z = 0;
        dimensions -= pos;
        // gets the dimensions of the screen the game is playing on in x and y values
        sh.scale = new Vector3(dimensions[1] * 2, dimensions[0] * 2, 0);
        // scales up the emitters so that particles are uniformly emitted across the screen, I suspect the scale is halved for some reason 
        transform.position = Camera.main.GetComponent<RectTransform>().anchoredPosition;
        transform.position -= new Vector3(0, 0, transform.position.z);
        partSys.Emit(25);
        built = true;
    }

    Dictionary<int, float> timesByParticle = new Dictionary<int, float>();

    /// <summary>
    /// Updater for the particles in the field, wraps them around if necessary
    /// </summary>
    /// <param name="particles">array of particles in the field</param>
    private void ParticleUpdate(ParticleSystem.Particle[] particles)
    {
        var oldZ = Camera.main.transform.position.z;
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y,
            -CameraScript.GetMaxZoomLevel());
        for (int i = 0; i < particles.Length; i++) // update all particles
        {
            if (!timesByParticle.ContainsKey(i))
            {
                timesByParticle.Add(i, 0);
            }
            else
            {
                timesByParticle[i] -= Time.deltaTime;
            }

            ParticleWrapper(ref particles[i], 0, i); // call particle wrapper for both dimensions
            ParticleWrapper(ref particles[i], 1, i);
        }

        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, oldZ);
    }

    /// <summary>
    /// particleUpdate helper method: wraps the particles to the other side of the screen if they are not in the correct bounds
    /// </summary>
    /// <param name="particle">the particle to wrap</param>
    /// <param name="dimension">the dimension (0 is x, 1 is y)</param>
    private void ParticleWrapper(ref ParticleSystem.Particle particle, int dimension, int index)
    {
        if (!PlayerCore.Instance || PlayerCore.Instance.IsMoving())
        {
            timesByParticle[index] = 0;
        }

        Vector3 relativeCameraPos = Camera.main.WorldToViewportPoint(particle.position);
        // grab the screen position of the particle
        if (timesByParticle[index] <= 0 && (relativeCameraPos[dimension] < 0F || relativeCameraPos[dimension] > 1F))
        {
            timesByParticle[index] = 1;
            // if the particle is past the screen limits wrap it around
            displacement = relativeCameraPos;
            displacement[dimension] = relativeCameraPos[dimension] < 0F
                ? 1F + relativeCameraPos[dimension]
                : relativeCameraPos[dimension] - 1;
            //Mathf.Abs(displacement[dimension] - limit);
            particle.position = Camera.main.ViewportToWorldPoint(displacement);
        }
    }

    public void Start()
    {
        if (!SystemLoader.AllLoaded) return;
        partSys.Clear();
        if (active)
        {
            Build();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!SystemLoader.AllLoaded) return;
        if (active)
        {
            if (!built || Camera.main.pixelRect != pixelRect)
            {
                Build();
            }

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[25]; // constantly update particle array, room for optimization here
            partSys.GetParticles(particles); // get particles
            ParticleUpdate(particles);
            partSys.SetParticles(particles, 25); // set particles
        }
        else
        {
            built = false;
            if (partSys)
            {
                partSys.Clear();
            }
        }
    }
}
