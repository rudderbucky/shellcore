using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    private float damage;
    public void SetDamage(float damage) {
        this.damage = damage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        var hit = collision.gameObject;
        var craft = hit.GetComponent<Craft>();
        if (craft != null)
        {
            craft.TakeDamage(damage, 0);
        }

        Destroy(gameObject);
    }
}
