using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionUIManager : MonoBehaviour {

	static ActionUIManager instance;

	public static ActionUIManager Instance {
		get {
			return instance;
		}
	}

	public GameObject actionButtonPrefab;
	public Sprite[] actionButtonSprites;
	public GameObject probabilitiesPopUp;
	public GameObject probabilityIcon;

	Dictionary<Contestant, List<GameObject>> constantButtonPools; // buttons that always appear (but might be disabled)
	Dictionary<Contestant, List<Button>> tempButtonPools;

 //buttons that only appear when an action is possible

	void Start () {
		if (instance != null) {
			Debug.LogError ("There should only be one ContestantButtonUIManager");
		}

		instance = this;

		constantButtonPools = new Dictionary<Contestant, List<GameObject>> ();

		/*
		ScreenManager.ScaleUIElement (actionButtonPrefab, 3f);
		ScreenManager.ScaleUIElement (probabilitiesPopUp, 20f, 10f);
		ScreenManager.ScaleUIElement (probabilityIcon, 2f);
*/
		UserControlManager.Instance.RegisterOnSelectedCallback ((con) => {
			SetButtonListActive(con, true);
		});

		UserControlManager.Instance.RegisterOnDeselectedCallback ((con) => {
			SetButtonListActive(con, false);
		});
	}

	//Player button pools
	public void CreateButtonPool(Contestant con){

		con.RegisterOnMoveCompleteCallback ( (c) => {
			if (UserControlManager.Instance.Selected == c) {
				SetButtonListActive(c, true);
			}
		});

		con.RegisterOnMoveBeganCallback ( (c) => {
			if (UserControlManager.Instance.Selected == c) {
				SetButtonListActive(c, false);
			}

			//CheckButtons
		});

		List<GameObject> buttons = new List<GameObject> ();

		foreach(IContestantAction a in con.PossibleActions){

			GameObject buttonBackground = Instantiate (actionButtonPrefab, Vector3.zero, Quaternion.identity, con.transform.Find("Canvas"));
			GameObject button = buttonBackground.transform.GetChild (0).gameObject;

			buttons.Add (buttonBackground);
			buttonBackground.name = con.name + " Button " + a.Name;
		
			button.GetComponent<Image> ().sprite = ContestantActionsFactory.GetSpriteFromType (a.ActionType);


			Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();
			bcevent.AddListener( () => {
				UserControlManager.Instance.ControlMode = a.ControlMode;
			});

			button.GetComponent<Button> ().onClick = bcevent;

			button.GetComponent<OnEnableCallbacks> ().RegisterOnEnabledConditionCallback (() => {
				return a.ControlMode.CheckValidity();
			});
				
			button.GetComponent<OnEnableCallbacks> ().RegisterOnEnabledFalseCallback (() => {
				buttonBackground.GetComponent<CanvasRenderer>().SetAlpha(0.6f); 

				button.GetComponent<Button>().interactable = false;

			});

			button.GetComponent<OnEnableCallbacks> ().RegisterOnEnabledTrueCallback (() => {
				buttonBackground.GetComponent<CanvasRenderer>().SetAlpha(1.0f); 

				button.GetComponent<Button>().interactable = true;
			});
				
		}

		if (buttons.Count > 0) {
			float totalDist = ( 1.1f * ((RectTransform)buttons[0].transform).rect.width);
			float limit = totalDist * (buttons.Count - 1); 

			for (int i = 0; i < buttons.Count; i++) {
				buttons [i].GetComponent<RectTransform> ().anchoredPosition = new Vector2 ((i * totalDist) -(limit / 2), 10);
			}
		}
			
		constantButtonPools[con] = buttons;

	}

	void SetButtonListActive(Contestant con, bool enabled){
		if (con != null) {
			foreach (GameObject b in constantButtonPools[con]) {
				b.SetActive (enabled);

				foreach (Transform t in b.transform) {
					t.gameObject.SetActive (enabled);
				}
			}
		}
	}
}
