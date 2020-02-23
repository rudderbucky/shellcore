using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data.
using UnityEngine.SceneManagement;

public class ButtonHoverScript : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
	bool mouseTag; // used for animation
	RectTransform rect; // used for animation
    public void OnPointerClick(PointerEventData eventData)
    {
			if(name == "MainMenuButton") 
			{
				if(SectorManager.testJsonPath == null) SceneManager.LoadScene("MainMenu");
				else SceneManager.LoadScene("WorldCreator");
			}
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
		if(GetComponent<Image>()) GetComponent<Image>().color -= 0.25F * Color.gray;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       	if(GetComponent<Image>()) GetComponent<Image>().color += 0.25F * Color.gray;
    }
}
