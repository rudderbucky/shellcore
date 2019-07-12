using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloaterScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.position = new Vector3(transform.position.x, transform.position.y, Random.Range(5, 9));
	}
}
