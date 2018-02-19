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
	[Range(0,100)]
	public float playerNameWidth;
	[Range(0,100)]
	public float playerNameHeight;
	public GameObject actionIconPrefab;
	[Range(0,100)]
	public float actionIconWidth;
	[Range(0,100)]
	public float actionIconHeight;
	//Team Banner
	[Range(0,100)]
	public float teamBannerWidth;
	[Range(0,100)]
	public float teamBannerHeight;
	//team name
	[Range(0,100)]
	public float teamNameWidth;
	[Range(0,100)]
	public float teamNameHeight;
	//team logo
	[Range(0,100)]
	public float teamLogoWidth;
	[Range(0,100)]
	public float teamLogoHeight;

	Dictionary<ContestantData, GameObject> contestantButtons;
	Dictionary<Team, GameObject> teamUIs;

	void Start () {
		if (instance != null) {
			Debug.LogError ("There should not be more than one team UI manager");
		}

		instance = this;

		contestantButtons = new Dictionary<ContestantData, GameObject> ();

		ScreenSpaceManager.ScaleUIElement (playerNamePrefab, playerNameWidth, playerNameHeight);
		ScreenSpaceManager.ScaleUIElement (actionIconPrefab, actionIconWidth, actionIconHeight);
	

		Canvas mainCanvas = GameManager.Instance.mainCanvas;

		teamUIs = new Dictionary<Team, GameObject> ();
		List<Team> tim = TeamManager.Instance.TeamsInMatch;
		int counter = 0;

		foreach (Transform t in GameManager.Instance.mainCanvas.transform) {
			if (t.name == "Team UI Parent") {
				if (counter >= tim.Count) {
					break;
				}
				teamUIs [tim [counter++]] = t.gameObject;
				ScreenSpaceManager.ScaleUIElement (t, teamBannerWidth, teamBannerHeight);
			}
		}

		if (counter != tim.Count) {
			Debug.LogWarning (string.Format("There are {0} team UI parents but {1} teams!", counter, tim.Count));
		}
			

		foreach(KeyValuePair<Team, GameObject> teamUI in teamUIs) {

			teamUI.Value.GetComponent<Image>().color = new Color (teamUI.Key.Color.r, teamUI.Key.Color.g, teamUI.Key.Color.b, 45f / 256); 
			teamUI.Value.transform.Find ("Text - Name").GetComponent<Text> ().text = teamUI.Key.Name;
			ScreenSpaceManager.ScaleUIElement (teamUI.Value.transform.Find ("Text - Name"), teamNameWidth, teamNameHeight);
			teamUI.Value.transform.Find ("Image - Logo").GetComponent<Image> ().sprite = teamUI.Key.Image;
			ScreenSpaceManager.ScaleUIElement (teamUI.Value.transform.Find ("Image - Logo"), teamLogoWidth, teamLogoHeight);
		

			//fill in score UI here, but for now it will always start at zero
			for (int j = 0; j < teamUI.Key.Contestants.Count; j++) {

				GameObject bObj = Instantiate (playerNamePrefab, Vector3.zero, Quaternion.identity, teamUI.Value.transform.Find ("Contestant Buttons"));
				//bObj is scaled bc playerNamePrefab is scaled
				Rect textRect = playerNamePrefab.GetComponent<RectTransform> ().rect;

				Vector2 pos = new Vector2 (0, -(j * textRect.height * 2));

				bObj.transform.localPosition = pos;

				bObj.transform.GetChild(0).GetComponent<Text> ().text = teamUI.Key.Contestants [j].Name;

				Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();
				bcevent.AddListener (CreateSelectionListenerForPlayerNameButton (teamUI.Key, j));

				bObj.GetComponent<Button> ().onClick = bcevent;

				contestantButtons [teamUI.Key.Contestants [j]] = bObj;
			}
		}

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
		Canvas mainCanvas = GameManager.Instance.mainCanvas;

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

		if (prevIcons.Count < con.ActionsRemaining) {

			float y = -actionIconPrefab.GetComponent<RectTransform> ().rect.height;
			float iconWidth = actionIconPrefab.GetComponent<RectTransform> ().rect.width;

			for (int i = prevIcons.Count; i < con.ActionsRemaining; i++) {
				GameObject actionIcon = Instantiate (actionIconPrefab, contestantButtons [con.Data].transform);
				//will need to set this to left or right
				actionIcon.transform.localPosition = new Vector3 (iconWidth * (i + 0.5f), y, 0f);
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
