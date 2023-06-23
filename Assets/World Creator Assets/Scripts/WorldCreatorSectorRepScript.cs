using UnityEngine;

public class WorldCreatorSectorRepScript : MonoBehaviour
{
    SpriteRenderer sprite;
    LineRenderer rend;
    public Sector sector;
    public Transform center;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        rend = GetComponentInParent<LineRenderer>();
        transform.position = rend.bounds.center;
        transform.localScale = rend.bounds.size;
        sprite.color = SectorColors.colors[0];
    }

    void Update()
    {
        transform.position = center.position = rend.bounds.center;
        transform.localScale = rend.bounds.size;
        var grad = 4.9F / (WorldCreatorCamera.maxZ - WorldCreatorCamera.minZ);
        if (WorldCreatorCamera.instance)
        {
            rend.startWidth = rend.endWidth = 5F - (grad * WorldCreatorCamera.maxZ) + grad * WorldCreatorCamera.instance.transform.position.z;
            center.localScale = new Vector3(rend.startWidth / 0.1F, rend.startWidth / 0.1F, 1);
        }
        ChangeColor();
    }

    void ChangeColor()
    {
        if (sector)
        {
            sprite.color = sector.backgroundColor;
        }
    }
}
