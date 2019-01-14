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
            texts[1].text = player.GetPower() + "";
            texts[3].text = player.unitsCommanding.Count + "/" + player.GetTotalCommandLimit();
            string description;
            if(player.GetTargetingSystem().GetTarget() && player.GetTargetingSystem().GetTarget().GetComponent<Entity>()) {
                Entity ent = player.GetTargetingSystem().GetTarget().GetComponent<Entity>();
                description = ent.Terrain + " ";
                description += ent.category;
                texts[6].text = description;
                texts[7].text = ent.entityName;
                texts[6].color = texts[7].color = FactionColors.colors[ent.faction];
            } else texts[6].text = texts[7].text = "";
        }
	}
}
