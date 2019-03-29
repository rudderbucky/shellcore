using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXHandler : MonoBehaviour {
	public RectangleEffectScript rect1;
	public RectangleEffectScript rect2;
	public HUDArrowScript hUDArrow;
	public PlayerCore player;

	public void Initialize() {
		rect1.Start();
		rect2.Start();
		if(player) hUDArrow.Initialize(player);
	}
}
