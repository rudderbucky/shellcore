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
        if (draggable && LandPlatformGenerator.IsOnGround(transform.position) && !draggable.Dragging)
        {
            isOnGround = true;
            Terrain = TerrainType.Ground;
        }
        else
        {
            if (isOnGround)
            {
                time = Time.time;
                initialzangle = transform.localEulerAngles.z;
            }

            isOnGround = false;
            Terrain = TerrainType.Air;

            transform.localEulerAngles = new Vector3(0, 0, initialzangle + (Time.time - time) * -180f);
        }
    }
}
