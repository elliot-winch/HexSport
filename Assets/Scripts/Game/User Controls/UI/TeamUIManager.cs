using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TeamUIManager : MonoBehaviour {

	static TeamUIManager instance;

	public static TeamUIManager Instance {
		get {
			return instance;
		}
	}
		
	public GameObject playerNamePrefab;
	public GameObject actionIconPrefab;

	Dictionary<ContestantData, GameObject> contestantButtons;
	Dictionary<Team, GameObject> teamUIs;

	void Start () {
		if (instance != null) {
			Debug.LogError ("There should not be more than one team UI manager");
		}

		instance = this;

		contestantButtons = new Dictionary<ContestantData, GameObject> ();

		Canvas mainCanvas = ScreenManager.Instance.mainCanvas;

		teamUIs = new Dictionary<Team, GameObject> ();
		List<Team> tim = TeamManager.Instance.TeamsInMatch;
		int counter = 0;

		foreach (Transform t in mainCanvas.transform) {
			if (t.name == "Team UI Parent") {
				if (counter >= tim.Count) {
					break;
				}
				teamUIs [tim [counter++]] = t.gameObject;
			}
		}

		if (counter != tim.Count) {
			Debug.LogWarning (string.Format("There are {0} team UI parents but {1} teams!", counter, tim.Count));
		}
			

		foreach(KeyValuePair<Team, GameObject> teamUI in teamUIs) {

			teamUI.Value.GetComponent<Image>().color = new Color (teamUI.Key.Color.r, teamUI.Key.Color.g, teamUI.Key.Color.b, 100f / 256); 
			teamUI.Value.transform.Find ("Text - Name").GetComponent<Text> ().text = teamUI.Key.Name;
			teamUI.Value.transform.Find ("Image - Logo").GetComponent<Image> ().sprite = teamUI.Key.Image;
		}

		//ScreenManager.ScaleUIElement (actionIconPrefab, 4.5f, 6f);

		GameManager.Instance.RegisterOnGameStart (() => {
			//Spawn in player buttons
			//ScreenManager.ScaleUIElement(playerNamePrefab, 9f, 6f);

			foreach(KeyValuePair<Team, GameObject> teamUI in teamUIs) {

				RectTransform parent = teamUI.Value.transform.Find ("Contestant Buttons").GetComponent<RectTransform>();

				float buttonWidthShiftSign = (parent.position.x < Screen.width / 2) ? 1f : -1f;
				parent.Translate(new Vector2(buttonWidthShiftSign * playerNamePrefab.GetComponent<RectTransform>().sizeDelta.x * 0.55f, -playerNamePrefab.GetComponent<RectTransform>().sizeDelta.y * 0.6f));
				
				for (int j = 0; j < teamUI.Key.Contestants.Count; j++) {

					GameObject bObj = Instantiate (playerNamePrefab, Vector3.zero, Quaternion.identity, parent);
					Rect textRect = bObj.GetComponent<RectTransform> ().rect;

					Vector2 pos = new Vector2 (0, -(j * bObj.GetComponent<RectTransform>().sizeDelta.y * 2));

					bObj.transform.localPosition = pos;

					bObj.transform.GetChild(0).GetComponent<Text> ().text = teamUI.Key.Contestants [j].Name;

					Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();
					bcevent.AddListener (CreateSelectionListenerForPlayerNameButton (teamUI.Key, j));

					bObj.GetComponent<Button> ().onClick = bcevent;

					contestantButtons [teamUI.Key.Contestants [j]] = bObj;
				}
			}
		});

		UserControlManager.Instance.RegisterOnSelectedCallback ((con) => {
			ColorBlock cb = contestantButtons[con.Data].GetComponent<Button>().colors;
			cb.normalColor = new Color(0,0,1f);
			contestantButtons[con.Data].GetComponent<Button>().colors = cb;

		});

		UserControlManager.Instance.RegisterOnDeselectedCallback ((con) => {
			ColorBlock cb = contestantButtons[con.Data].GetComponent<Button>().colors;
			cb.normalColor = new Color(1f,1f,1f);
			contestantButtons[con.Data].GetComponent<Button>().colors = cb;

		});
	}

	//Helper for start
	UnityAction CreateSelectionListenerForPlayerNameButton(Team t, int j){
		UnityAction ua = () => {

			if(t == TeamManager.Instance.CurrentTeam){		
				UserControlManager.Instance.ControlModeType = ControlModeEnum.Move;
				StartCoroutine(UserControlManager.Instance.SelectContestant(t.Contestants[j].Contestant));
			}
		};

		return ua;
	}
	
	public void UpdateScoreUI (Team t, int value) {
		Canvas mainCanvas = ScreenManager.Instance.mainCanvas;

		teamUIs[t].transform.Find ("Text - Score").GetComponent<Text> ().text = value.ToString();
	}


	#region Actions UI
	List<GameObject> GetActionIcons(Contestant con){
		List<GameObject> icons = new List<GameObject> ();

		foreach (Transform t in contestantButtons [con.Data].transform) {
			if (t.name == "Image - Action Icon") {
				icons.Add (t.gameObject);
			}
		}

		return icons;
	}

	public void UpdateActionsRemainingUI(Contestant con){
		List<GameObject> prevIcons = GetActionIcons (con);
		Transform buttonParent = contestantButtons [con.Data].transform;

		if (prevIcons.Count < con.ActionsRemaining) {
			//should save these at the start of the game
			float iconWidth = actionIconPrefab.GetComponent<RectTransform> ().rect.width;
			float direction = (buttonParent.position.x < Screen.width / 2) ? 1f : -1f;

			Vector2 startPos = new Vector2 (buttonParent.position.x - direction * iconWidth / 2f, buttonParent.position.y -actionIconPrefab.GetComponent<RectTransform> ().rect.height);

			for (int i = prevIcons.Count; i < con.ActionsRemaining; i++) {
				GameObject actionIcon = Instantiate (actionIconPrefab, buttonParent);
				//will need to set this to left or right
				actionIcon.transform.position = startPos + new Vector2 (iconWidth * i * direction, 0f);
				actionIcon.name = "Image - Action Icon";
			}
		} else if (con.ActionsRemaining < prevIcons.Count) {
			for (int i = con.ActionsRemaining; i < prevIcons.Count; i++) {
				Destroy (prevIcons [i]);
			}
		}
	}

	public void ClearActionsRemainingUI(Team t){
		foreach (ContestantData c in t.Contestants) {
			foreach (GameObject g in GetActionIcons(c.Contestant)) {
				Destroy (g);
			}
		}
	}
	#endregion //Action Icons
}
