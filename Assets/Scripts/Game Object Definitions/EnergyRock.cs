using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRock : MonoBehaviour {
    public GameObject energySpherePrefab;


    float maxTime = 4f;
    float targetTime = 0f;

    private void Start()
    {
        targetTime = Time.time + maxTime;
    }

    void Update () {
        if (Time.time > targetTime) {
            targetTime = Time.time + maxTime;

            var x = Instantiate(energySpherePrefab, null, false);
            x.GetComponent<SpriteRenderer>().sortingLayerID = 0;
            x.transform.position = transform.position;

            float dir = Random.Range(0f, 2 * Mathf.PI);
            x.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Sin(dir), Mathf.Cos(dir)) * Random.Range(180f, 240f));
        }
	}
}
