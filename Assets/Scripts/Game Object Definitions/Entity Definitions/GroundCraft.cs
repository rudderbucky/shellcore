using UnityEngine;

public class GroundCraft : Craft
{
    float time = 0f;
    protected bool isOnGround = false;
    private float initialzangle;

    protected override void Start()
    {
        Terrain = TerrainType.Ground;
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (draggable && LandPlatformGenerator.IsOnGround(transform.position) && !draggable.dragging)
        {
            isOnGround = true;
        }
        else
        {
            if (isOnGround)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
            }

            isOnGround = false;

            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
