using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipsFromTheYard : MonoBehaviour {

	List<string> tipsList;
	Text text;
	Color[] colors;
	void Awake() {
		int DaysSinceFeb3_2019 = new System.DateTime(2019, 2, 3, 0, 0, 0, System.DateTimeKind.Utc).Day;
		text = GetComponent<Text>();
		tipsList = new List<string>();
		tipsList.Add("It's been " + (System.DateTime.UtcNow.Day - DaysSinceFeb3_2019) +  " days since we first started trying to find something to place here!");
		tipsList.Add("You can press 'C' to clear all parts!");
		tipsList.Add("ShellCores that travel to havens through deadzones are clearly delusional!");
		tipsList.Add("Reply hazy, try again later!");
		tipsList.Add("RUN from the Type-PP homing missiles!");
		tipsList.Add("Be glad you don't float in one place all day doing surgery on ShellCores!");
		tipsList.Add("Make sure your ship can supply the required energy to handle all your abilities!");
		tipsList.Add("Slow ships are boring! Try adding some speed or removing some parts if your ship is too slow!");
		colors = new Color[] {Color.green, Color.cyan, Color.magenta, Color.white, Color.blue};
	}
	
	void OnEnable() {
		text.color = colors[Random.Range(0, 4)];
		text.text = "Tips from the Yard:\n" + tipsList[Random.Range(0, tipsList.Count)];
	}
}
