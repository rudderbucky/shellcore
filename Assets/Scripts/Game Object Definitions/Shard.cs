using UnityEngine;

public class Shard : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private bool collectible;
    private Draggable draggable;
    private Rigidbody2D rigid;
    float detachedTime; // time since detachment
    private bool rotationDirection = true;
    private float rotationOffset;
    private bool hasDetached; // is the part detached
    public int tier;

    public void SetCollectible(bool collectible)
    {
        this.collectible = collectible;
    }

    public void Detach()
    {
        detachedTime = Time.time; // update detached time
        hasDetached = true; // has detached now
        gameObject.AddComponent<Rigidbody2D>(); // add a rigidbody (this might become permanent)
        rigid = GetComponent<Rigidbody2D>();
        rigid.gravityScale = 0; // adjust the rigid body
        rigid.angularDrag = 0;
        float randomDir = Random.Range(0f, 360f);
        rigid.AddForce(new Vector2(Mathf.Cos(randomDir), Mathf.Sin(randomDir)) * 200f);
        //rigid.AddTorque(150f * ((Random.Range(0, 2) == 0) ? 1 : -1));
        rotationDirection = (Random.Range(0, 2) == 0);
        gameObject.layer = 9;
        rotationOffset = Random.Range(0f, 360f);
    }

    /// <summary>
    /// Makes the part blink like in the original game
    /// </summary>
    void Blink()
    {
        spriteRenderer.enabled = Time.time % 0.25F > 0.125F; // math stuff that blinks the part
    }

    private void OnDestroy()
    {
        if (AIData.rockFragments.Contains(draggable))
        {
            AIData.rockFragments.Remove(draggable);
        }
    }

    void Update()
    {
        if (hasDetached && Time.time - detachedTime < 1) // checks if the part has been detached for more than a second (hardcoded)
        {
            Blink(); // blink
            //rigid.rotation = rigid.rotation + (rotationDirection ? 1f : -1.0f) * 360f * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, 0, (rotationDirection ? 1.0f : -1.0f) * 100f * Time.time + rotationOffset);
        }
        else if (hasDetached)
        {
            // if it has actually detached
            if (collectible)
            {
                rigid.drag = 20;
                // add "Draggable" component so that shellcores can grab the part
                if (!draggable)
                {
                    draggable = gameObject.AddComponent<Draggable>();
                    if (!AIData.rockFragments.Contains(draggable))
                    {
                        AIData.rockFragments.Add(draggable);
                    }
                }

                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = 0;
                transform.eulerAngles = new Vector3(0, 0, (rotationDirection ? 1.0f : -1.0f) * 100f * Time.time + rotationOffset);
                //rigid.angularVelocity = rigid.angularVelocity > 0 ? 200 : -200;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
