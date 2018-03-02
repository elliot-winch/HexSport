using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour {

	static ScreenManager instance;

	public static ScreenManager Instance {
		get {
			return instance;
		}
	}

	public Canvas mainCanvas;

	static float screenWidth;
	static float screenHeight;

	void Start () {
		if (instance != null) {
			Debug.LogError ("There should only be one ScreenManager");
		}

		instance = this;

		screenWidth = Screen.width;
		screenHeight = Screen.height;

		UIScalable[] toScale = mainCanvas.GetComponentsInChildren<UIScalable> ();

		foreach (UIScalable s in toScale) {
			//ScaleUIElement (s.gameObject, s.percentageOfScreen.x, s.percentageOfScreen.y);
		}
	}

	//percentage is the percentage of the screen which the element shall take up (out of 100)
	public static void ScaleUIElement(GameObject element, float percentageX, float percentageY = -1){
		ScaleUIElementRelative (element.transform, new Vector2 (screenWidth, screenHeight), percentageX, percentageY);
	}

	public static void ScaleUIElement(Transform element, float percentageX, float percentageY = -1){
		ScaleUIElementRelative (element, new Vector2 (screenWidth, screenHeight), percentageX, percentageY);
	}

	/*
	 * To maintain relative proportions, only provide one argument. It will be scaled by x
	 */ 
	public static void ScaleUIElementRelative(Transform element, Vector2 relativeSize, float percentageX, float percentageY){
		RectTransform elementRT = element.GetComponent<RectTransform> ();

		Vector2 originalSize = elementRT.sizeDelta;

		Debug.Log ("Scaling " + element.name);

		if (percentageY < 0) {
			Debug.Log (element.name + " is using relative proportions");
			element.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((percentageX / 100f) * relativeSize.x, (percentageX / 100f) * relativeSize.x);
		} else {
			element.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((percentageX / 100f) * relativeSize.x, (percentageY / 100f) * relativeSize.y);
		}
			

		Vector2 changeInSize = new Vector2 (elementRT.sizeDelta.x / originalSize.x, elementRT.sizeDelta.y / originalSize.y);

		List<RectTransform> childRTs = element.GetComponentsInChildren<RectTransform> ().ToList();
		childRTs.Remove (elementRT);

		foreach (RectTransform rt in childRTs) {
			if (rt.GetComponent<Text> () != null) {
				rt.GetComponent<RectTransform> ().sizeDelta = new Vector2 (rt.GetComponent<RectTransform> ().sizeDelta.x * changeInSize.x, rt.GetComponent<RectTransform> ().sizeDelta.y * changeInSize.y);
			}
			//else change font size?
		}
	}
		
	//we are assuming the screen size does not change without this manager being told so
	//TODO implement OnScreenSizeChanged()
}
