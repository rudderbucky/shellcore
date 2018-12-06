using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRock : MonoBehaviour {
    public GameObject energySpherePrefab;
	
	// Update is called once per frame
	void Update () {
        if (Time.time % 3 > 3 - Time.deltaTime) {
            var x = Instantiate(energySpherePrefab, null, false);
            x.GetComponent<SpriteRenderer>().sortingLayerID = 0;
            x.transform.position = transform.position;
            x.GetComponent<Rigidbody2D>().AddForce(100 * new Vector2(Random.Range(-5F, 5), Random.Range(-5F, 5)));
        }
	}
}
