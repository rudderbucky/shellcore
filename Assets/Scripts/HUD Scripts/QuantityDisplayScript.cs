using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuantityDisplayScript : MonoBehaviour {

    private PlayerCore player;
    private bool initialized;
    Text[] ob;
    public Image bg1;
    public Image bg2;
	// Use this for initialization
	void Start () {
	    ob = transform.parent.parent.Find("MinimapDisplay").GetComponentsInChildren<Text>();
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
                bg1.enabled = bg2.enabled = true;
                bg1.color = new Color32((byte)32,(byte)32,(byte)32,(byte)216);
                bg2.color = new Color32(0,0,0,(byte)216);
                Entity ent = player.GetTargetingSystem().GetTarget().GetComponent<Entity>();
                description = ent.Terrain + " ";
                description += ent.category;
                ob[0].text = ent.entityName;
                ob[1].text = description;
                ob[2].text = "Distance: " + (int)(ent.transform.position - player.transform.position).magnitude;
                ob[0].color = ob[1].color = ob[2].color = FactionColors.colors[ent.faction];
            } else {
                ob[0].text = ob[1].text = ob[2].text = "";
                bg1.enabled = bg2.enabled = false;
                }
        }
	}
}
