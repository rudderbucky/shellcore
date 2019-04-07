using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditIncrementMarker : MonoBehaviour {

	Text textComp;
	Vector2 pos;
	void Awake() {
		textComp = GetComponent<Text>();
		pos = textComp.rectTransform.anchoredPosition;
	}
	public void DisplayText(int num) {
		textComp.rectTransform.anchoredPosition = pos;
		textComp.color = (num > 0 ? new Color(0,1,0,1) : new Color(1,0,0,1));
		textComp.text = (num > 0 ? "+" : "") + num;

		StartCoroutine("Fade");
	}
	IEnumerator Fade() {
		while(textComp.color.a > 0) {
			textComp.rectTransform.anchoredPosition = textComp.rectTransform.anchoredPosition + new Vector2(0,0.25F);
			textComp.color = textComp.color - new Color(0,0,0,0.025F);
			yield return null;
		}
	}
}
