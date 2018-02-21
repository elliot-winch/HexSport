using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatUIManager : MonoBehaviour {

	static StatUIManager instance;

	public string[] statSpriteNames;
	public Sprite[] statSprites;
	public GameObject statFieldPrefab;

	//World Space
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

	public static StatUIManager Instance {
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

		statParent = ScreenManager.Instance.mainCanvas.transform.Find ("Background - Selected Stats");
		targetStatParent = ScreenManager.Instance.mainCanvas.transform.Find ("Background - Targeted Stats");

		//NB scaling Y here has no effect as it is controlled by the number of active text fields in it

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
		Debug.Log ("Currently has text fields: " + currentTextFields.Length);

		KeyValuePair<string, string>[] kvs = stats.Stats.ToArray ();

		int i;
		for(i = 0; i < kvs.Length; i++) {
			Debug.Log (i);
			if (i >= currentTextFields.Length) {

				GameObject statField = Instantiate (statFieldPrefab, offsetter);
				statField.name = "Contestant UI Stat Field";

				//statField.GetComponent<RectTransform> ().sizeDelta = new Vector2 (background.GetComponent<RectTransform>().rect.width, statFieldPrefab.GetComponent<RectTransform>().rect.height);
				ScreenManager.ScaleUIElement(statField, 10f, 7.5f);

				statField.transform.localPosition = new Vector3 (0f, statField.GetComponent<RectTransform> ().rect.height * (i + 0.5f));
			} 
		}

		while (i < currentTextFields.Length) {
			currentTextFields [i].text = "";
			currentTextFields [i].GetComponentInChildren<Image> ().sprite = new Sprite ();
			i++;
		}

		//Rescale background
		//backgroundRect.sizeDelta = new Vector2(backgroundRect.rect.width, (statFieldPrefab.GetComponent<RectTransform>().rect.height * kvs.Length));
		ScreenManager.ScaleUIElement(background, 10f, 7.5f * kvs.Length);

		Text[] newTextFields = offsetter.GetComponentsInChildren<Text> ();

		Debug.Log (newTextFields.Length + " " + kvs.Length);
		for (int j = kvs.Length - 1; j >= 0; j--) {

			Text currentTextField = newTextFields [kvs.Length - j - 1];
			Image imageField = currentTextField.GetComponentInChildren<Image> ();

			int imageForStat = -1;
			for (int s = 0; s < statSpriteNames.Length; s++) {
				if (statSpriteNames[s] == kvs [j].Key) {
					imageForStat = s;
					break;
				}
			}

			if (imageForStat >= 0) {
				imageField.sprite = statSprites[imageForStat];
				imageField.color = new Color (1f, 1f, 1f, 1f);
				currentTextField .text = string.Format ("{0}\t", kvs [j].Value);
			} else if (kvs[j].Key == "Title") {
				currentTextField.text = string.Format ("{0}", kvs [j].Value);
				currentTextField.fontStyle = FontStyle.Bold;
				currentTextField.alignment = TextAnchor.MiddleCenter;
			} else {
				imageField.color = new Color (1f, 1f, 1f, 0f);
				currentTextField.text = string.Format ("{0}: {1}\t", kvs [j].Key, kvs [j].Value);
			}
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
