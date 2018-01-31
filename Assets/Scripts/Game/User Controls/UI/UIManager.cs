﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	static UIManager instance;

	public static UIManager Instance {
		get {
			return instance;
		}
	}

	public GameObject actionButtonPrefab;
	public Canvas mainCanvas;
	public float buttonSpacing;

	Dictionary<Contestant, List<Button>> constantButtonPools; // buttons that always appear (but might be disabled)
	Dictionary<Contestant, List<Button>> tempButtonPools; //buttons that only appear when an action is possible

	void Start () {
		if (instance != null) {
			Debug.LogError ("There should only be one UI Manager");
		}

		instance = this;

		constantButtonPools = new Dictionary<Contestant, List<Button>> ();

		UserControlManager.Instance.RegisterOnSelectedCallback ((con) => {
			EnableButtonList(con);
		});

		UserControlManager.Instance.RegisterOnDeselectedCallback ((con) => {
			DisableButtonList(con);
		});
	}

	//Player button pools
	public void CreateButtonPool(Contestant con){

		con.RegisterOnMoveCompleteCallback ( (c) => {
			if (UserControlManager.Instance.Selected == c) {
				EnableButtonList(c);
			}
		});

		con.RegisterOnMoveBeganCallback ( (c) => {
			if (UserControlManager.Instance.Selected == c) {
				DisableButtonList(c);
			}
		});

		List<Button> buttons = new List<Button> ();

		foreach(IContestantAction a in con.PossibleActions){

			GameObject button = Instantiate (actionButtonPrefab, Vector3.zero, Quaternion.identity, mainCanvas.transform);
			button.name = con.name + " Button " + a.Name;
		
			button.GetComponentInChildren<Text> ().text = a.Name;

			button.SetActive (false);

			Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();
			bcevent.AddListener( () => {
				UserControlManager.Instance.ControlMode = a.ControlMode;
			});

			button.GetComponent<Button> ().onClick = bcevent;

			button.GetComponent<OnEnableButton> ().RegisterOnEnabledCallback (() => {
				return a.ControlMode.CheckValidity();
			});

			buttons.Add (button.GetComponent<Button>());

		}

		if (buttons.Count > 0) {
			float totalDist = (buttonSpacing + ((RectTransform)buttons[0].transform).rect.width);
			float limit = totalDist * (buttons.Count - 1); 

			for (int i = 0; i < buttons.Count; i++) {
				buttons [i].GetComponent<RectTransform> ().anchoredPosition = new Vector2 ((i * totalDist) -(limit / 2), 20);
			}
		}

		constantButtonPools[con] = buttons;
	}

	void EnableButtonList(Contestant con){
		if (con != null) {
			foreach (Button b in constantButtonPools[con]) {
				b.gameObject.SetActive (true);
			}
		}
	}

	void DisableButtonList(Contestant con){
		if (con != null) {
			foreach (Button b in constantButtonPools[con]) {
				b.gameObject.SetActive (false);
			}
		}
	}
}