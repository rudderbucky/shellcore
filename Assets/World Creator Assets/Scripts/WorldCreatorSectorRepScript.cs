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
