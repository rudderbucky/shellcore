using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantityDisplayScript : MonoBehaviour {

    private PlayerCore player;
    private bool initialized;
	// Use this for initialization
	void Start () {
		
	}
	
    public void Initialize(PlayerCore player)
    {
        this.player = player;
        initialized = true;
    }
	// Update is called once per frame
	void Update () {
        if (initialized)
        {
            var texts = GetComponentsInChildren<UnityEngine.UI.Text>();
            texts[0].text = player.GetPower() + "";
            texts[1].text = player.unitsCommanding.Count + "/" + player.GetTotalCommandLimit();
        }
	}
}
