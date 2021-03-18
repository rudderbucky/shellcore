using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloaterScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if(SceneManager.GetActiveScene().name == "SampleScene" || SceneManager.GetActiveScene().name == "MainMenu")
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, 5);
			GetComponent<SpriteRenderer>().color = SectorManager.instance.current.backgroundColor + Color.grey;
		}
	}

	void Update()
	{
		if(!SectorManager.instance) return;
		if(SectorManager.instance.overrideProperties != null)
		{
			GetComponent<SpriteRenderer>().color = SectorManager.instance.overrideProperties.backgroundColor + Color.grey;
		}
		else GetComponent<SpriteRenderer>().color = SectorManager.instance.current.backgroundColor + Color.grey;
	}
}
