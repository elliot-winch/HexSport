using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			ScaleUIElement (s.gameObject, s.percentageOfScreen.x, s.percentageOfScreen.y);
		}
	}

	//percentage is the percentage of the screen which the element shall take up (out of 100)
	public static void ScaleUIElement(GameObject element, float percentageX, float percentageY){
		ScaleUIElementRelative (element.transform, new Vector2 (screenWidth, screenHeight), percentageX, percentageY);
	}

	public static void ScaleUIElement(Transform element, float percentageX, float percentageY){
		ScaleUIElementRelative (element, new Vector2 (screenWidth, screenHeight), percentageX, percentageY);
	}

	public static void ScaleUIElementRelative(Transform element, Vector2 relativeSize, float percentageX, float percentageY){
		RectTransform elementRT = element.GetComponent<RectTransform> ();

		Vector2 originalSize = elementRT.sizeDelta;

		element.GetComponent<RectTransform>().sizeDelta = new Vector2 ((percentageX / 100f) * relativeSize.x, (percentageY / 100f) * relativeSize.y);

		Vector2 changeInSize = new Vector2 (elementRT.sizeDelta.x / originalSize.x, elementRT.sizeDelta.y / originalSize.y);

		List<RectTransform> childRTs = element.GetComponentsInChildren<RectTransform> ().ToList();
		childRTs.Remove (elementRT);

		foreach (RectTransform rt in childRTs) {
			rt.GetComponent<RectTransform> ().sizeDelta = new Vector2 (rt.GetComponent<RectTransform> ().sizeDelta.x * changeInSize.x, rt.GetComponent<RectTransform> ().sizeDelta.y * changeInSize.y);
		}
	}
		
	//we are assuming the screen size does not change without this manager being told so
	//TODO implement OnScreenSizeChanged()
}
