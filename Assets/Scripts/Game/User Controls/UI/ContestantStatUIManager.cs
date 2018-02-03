using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContestantStatUIManager : MonoBehaviour {

	//one canvas and replace data or n canvases and hide all but selected? think ill go with one canvas

	static ContestantStatUIManager instance;

	Font arial;
	Transform statParent;
	Transform targetStatParent;

	public static ContestantStatUIManager Instance {
		get {
			return instance;
		}
	}

	public Transform StatParent {
		get {
			return statParent;
		}
	}

	public Transform TargetStatParent {
		get {
			return targetStatParent;
		}
	}

	void Start(){
		if (instance != null) {
			Debug.LogError ("There should only be one ContestantStatUIManager");
		}

		instance = this;

		statParent = GameManager.Instance.mainCanvas.transform.Find ("Background - Selected Stats");
		targetStatParent = GameManager.Instance.mainCanvas.transform.Find ("Background - Targeted Stats");

		arial = Resources.GetBuiltinResource (typeof(Font), "Arial.ttf") as Font;

		UserControlManager.Instance.RegisterOnSelectedCallback ((con) => {
			SelectedStatUI(con.Data);
		});
	}

	void ShowStats(IStats stats, Transform background){
		Transform offsetter = background.Find ("Offsetter");

		Text[] currentTextFields = offsetter.GetComponentsInChildren<Text> ();

		KeyValuePair<string, string>[] kvs = stats.Stats.ToArray ();

		int i;
		for(i = 0; i < kvs.Length; i++) {
			Text t;

			if (i >= currentTextFields.Length) {

				GameObject textObj = new GameObject ();
				textObj.name = "Contestant UI Stat Field";
				t = textObj.AddComponent<Text> ();

				t.font = arial;
				t.alignment = TextAnchor.MiddleCenter;
				t.GetComponent<RectTransform> ().sizeDelta = new Vector2 (240f, 25f);
				t.fontSize = 14;
				t.color = Color.white;

				textObj.transform.SetParent (offsetter);
				textObj.transform.localPosition = new Vector3 (0f, textObj.GetComponent<RectTransform> ().rect.height * (i + 0.5f));
			} 
		}

		while (i < currentTextFields.Length) {
			currentTextFields [i++].text = "";
		}

		//Rescale background
		RectTransform backgroundRect = background.GetComponent<RectTransform>();
		backgroundRect.sizeDelta = new Vector2(240f, (25f * kvs.Length));

		Text[] newTextFields = background.GetComponentsInChildren<Text> ();

		for (int j = kvs.Length - 1; j >= 0; j--) {
			newTextFields[kvs.Length - j - 1].text = string.Format ("{0}: {1}", kvs[j].Key, kvs[j].Value);
		}

		offsetter.GetComponent<RectTransform>().localPosition = new Vector2 (offsetter.GetComponent<RectTransform>().localPosition.x, 0f);
	}

	public void SelectedStatUI(ContestantData cd){
		ShowStats (cd, statParent);
	}

	public void ShowTargetUI(IStats s){
		ShowStats(s, targetStatParent);
	}
}
