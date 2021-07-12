using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipsFromTheYard : MonoBehaviour
{
    Text text;

    List<string> tipsList = new List<string>
    {
        "You can hold Shift while placing a part in an invalid location to snap it back to its last location! Use this to layer parts to your will!",
        "You can press 'C' to clear all parts from the grid! All parts are technologically swept into your inventory!",
        "ShellCores that travel to havens through deadzones are clearly delusional!",
        "Reply hazy, try again later!",
        "RUN from the Type-PP homing missiles!",
        "Be glad you don't float in one place all day doing surgery on ShellCores!",
        "Make sure your ship can supply the required energy to handle all your abilities!",
        "Slow ships are boring! Try adding some speed or removing some parts if your ship is too slow!",
        "Shift-click a preset button to clear it!",
        "The hitboxes of parts in the ship builder are pretty much just rectangles! Use this to create some cool designs!",
        "If a part is not connected in a direct line of parts to the shell, it will detach, or 'domino'. Make sure your ship design prevents this from happening!",
        "You can hit 'escape' to close this window too! That way you can cycle through my infinite wisdom faster!",
        "You can create custom ShellCores in the World Creator using the JSON you get from this ship builder! Share your JSON with friends!",
        "If you double click the Status Menu map it switches from a draggable map to a minimap-style scroll (and vice versa)!",
        "You can shift-click part buttons to instantly buy/sell them!",
        "You can press tab to cycle through your selected targets!",
        "When not receiving background dialogue you can press 'enter' to view all the background dialogue you received before!",
        "Double-click the grid to reset the center to your core!"
    };

    Color[] colors = new Color[]
    {
        Color.green,
        Color.cyan,
        Color.magenta,
        Color.yellow,
        Color.blue
    };

    void Awake()
    {
        int DaysSinceFeb3_2019 = (int)(System.DateTime.UtcNow - new System.DateTime(2019, 2, 3, 0, 0, 0, System.DateTimeKind.Utc)).TotalDays;
        text = GetComponent<Text>();
        tipsList.Add($"It's been {DaysSinceFeb3_2019} days since we first started trying to find something to place here!");
    }

    void OnEnable()
    {
        text.color = colors[Random.Range(0, 4)];
        text.text = "TIPS FROM THE YARD:\n" + tipsList[Random.Range(0, tipsList.Count)].ToUpper();
    }
}
