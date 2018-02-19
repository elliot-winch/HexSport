using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceManager : MonoBehaviour {

	static ScreenSpaceManager instance;

	static float screenWidth;
	static float screenHeight;

	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
	}

	//percentage is the percentage of the screen which the element shall take up (out of 100)
	public static void ScaleUIElement(GameObject element, float percentageX, float percentageY){
		Debug.Log ("Width: " + (percentageX / 100f) * screenWidth);
		Debug.Log ("Width: " + (percentageY / 100f) * screenHeight);

		element.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((percentageX / 100f) * screenWidth, (percentageY / 100f) * screenHeight);
	}

	public static void ScaleUIElement(Transform element, float percentageX, float percentageY){
		ScaleUIElement(element.gameObject, percentageX, percentageY);
	}
		
	//we are assuming the screen size does not change without this manager being told so
	//TODO implement OnScreenSizeChanged()
}
