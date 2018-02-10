using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContestantStatUIManager : MonoBehaviour {

	//one canvas and replace data or n canvases and hide all but selected? think ill go with one canvas

	static ContestantStatUIManager instance;

	public GameObject contestantLabelPrefab;
	[Range(0,1)]
	public float labelAlphaLow;
	[Range(0,1)]
	public float labelAlphaHigh;
	public Vector2 minLabelSize;		//this is the size of the label at the top of the screen
	public Vector2 labelSizeIncrease;	//this + minLabelSize is the size of the label at the bottom of the screen

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

			SetLabelAlpha (con.transform.Find("Canvas").Find("Contestant Label"), labelAlphaHigh);

		});

		UserControlManager.Instance.RegisterOnDeselectedCallback ( (con) => {
			statParent.gameObject.SetActive(false);

			SetLabelAlpha (con.transform.Find("Canvas").Find("Contestant Label"), labelAlphaLow);

		});
	}

	void Update(){
		foreach (Contestant con in TeamManager.Instance.AllContestants) {
			SetLabelPosition (con, con.transform.Find("Canvas").Find("Contestant Label"));
		}
	}

	//Methods
	public void InitLabelUI(Contestant con){
		GameObject label = Instantiate (contestantLabelPrefab, con.transform.Find ("Canvas"));
		label.name = "Contestant Label";

		SetLabelAlpha (con.transform, labelAlphaLow);
	
		SetLabelPosition (con, label.transform);

		label.GetComponentInChildren<Text> ().text = con.Data.Name;
		//label.GetComponentInChildren<Slider>().value = startingHex / maxHealth
	}

	void SetLabelPosition(Contestant con, Transform label){
		Bounds conBounds = con.GetComponentInChildren<MeshRenderer> ().bounds;

		Vector3 labelWorldPos = con.Position + new Vector3(0f, (conBounds.center.y + conBounds.extents.y) * 1.4f, 0f);

		label.position = Camera.main.WorldToScreenPoint (labelWorldPos);

		//Comment these lines out to see their effect. The box doesn't change size as you move
		//the camera, which creates a funky non-persepctive effect.
		label.localScale = labelSizeIncrease * ((Screen.height - label.position.y) / Screen.height) + minLabelSize;
	}

	void SetLabelAlpha(Transform label, float alpha){
		foreach (CanvasRenderer l in label.GetComponentsInChildren<CanvasRenderer>()) {
			l.SetAlpha (alpha);
		}
	}

	void ShowStats(IStats stats, Transform background){
		statParent.gameObject.SetActive (true);

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
