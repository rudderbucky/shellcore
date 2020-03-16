using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloaterScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if(SceneManager.GetActiveScene().name == "SampleScene")
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, 5);
			GetComponent<SpriteRenderer>().color = SectorManager.instance.current.backgroundColor + Color.grey;
		}
	}
}
