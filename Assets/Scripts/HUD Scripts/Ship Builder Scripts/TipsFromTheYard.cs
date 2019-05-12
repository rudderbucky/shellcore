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
		tipsList.Add("You can press 'C' to clear all parts from the grid! All parts are technologically swept into your inventory!");
		tipsList.Add("ShellCores that travel to havens through deadzones are clearly delusional!");
		tipsList.Add("Reply hazy, try again later!");
		tipsList.Add("RUN from the Type-PP homing missiles!");
		tipsList.Add("Be glad you don't float in one place all day doing surgery on ShellCores!");
		tipsList.Add("Make sure your ship can supply the required energy to handle all your abilities!");
		tipsList.Add("Slow ships are boring! Try adding some speed or removing some parts if your ship is too slow!");
		tipsList.Add("Shift-click a preset button to clear it!");
		tipsList.Add("The hitboxes of parts in the ship builder are pretty much just rectangles! Use this to create some cool designs!");
		tipsList.Add("If a part is not connected in a direct line of parts to the shell, it will detach, or 'domino'. Make sure your ship design prevents this from happening!");
		tipsList.Add("You can hit 'escape' to close this window too! That way you can cycle through my infinite wisdom faster!");
		tipsList.Add("You can create custom ShellCores in the Sector Creator using the JSON you get from this ship builder! Share your JSON with friends and dogfight against their ShellCores!");
		colors = new Color[] {Color.green, Color.cyan, Color.magenta, Color.yellow, Color.blue};
	}
	
	void OnEnable() {
		text.color = colors[Random.Range(0, 4)];
		text.text = "Tips from the Yard:\n" + tipsList[Random.Range(0, tipsList.Count)];
	}
}
