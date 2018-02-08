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

	void Start () {
		if (instance != null) {
			Debug.LogError ("There should not be more than one team UI manager");
		}

		instance = this;

		Canvas mainCanvas = GameManager.Instance.mainCanvas;
		Image[] teamBackground = mainCanvas.GetComponentsInChildren<Image> ();
			
		List<Team> tim = TeamManager.Instance.TeamsInMatch;

		for(int i = 0; i < tim.Count; i++) {

			teamBackground [i].color = new Color (tim [i].Color.r, tim [i].Color.g, tim [i].Color.b, 45f / 256); 
			teamBackground [i].transform.Find ("Text - Name").GetComponent<Text> ().text = tim [i].Name;

			//fill in score UI here, but for now it will always start at zero
			for (int j = 0; j < tim[i].Contestants.Count; j++) {

				GameObject bObj = Instantiate(playerNamePrefab, Vector3.zero, Quaternion.identity, teamBackground[i].transform.Find("Contestant Buttons"));

				Rect textRect = playerNamePrefab.GetComponent<RectTransform> ().rect;

				Vector2 pos = new Vector2 (0, -(j * textRect.height));

				bObj.transform.localPosition = pos;

				bObj.transform.GetChild(0).GetComponent<Text> ().text = tim [i].Contestants [j].Name;

				Button.ButtonClickedEvent bcevent = new Button.ButtonClickedEvent ();
				bcevent.AddListener (CreateSelectionListenerForPlayerNameButton (tim, i, j));

				bObj.GetComponent<Button> ().onClick = bcevent;
			}
		}
	}

	//Helper for start
	UnityAction CreateSelectionListenerForPlayerNameButton(List<Team> teams, int i, int j){
		UnityAction ua = () => {

			if(teams[i] == TeamManager.Instance.CurrentTeam){		
				UserControlManager.Instance.ControlModeType = ControlModeEnum.Move;
				StartCoroutine(UserControlManager.Instance.SelectContestant(teams[i].Contestants[j].Contestant));
			}
		};

		return ua;
	}
	
	public void UpdateScoreUI (int teamIndex, int value) {
		Canvas mainCanvas = GameManager.Instance.mainCanvas;
		Image[] teamBackground = mainCanvas.GetComponentsInChildren<Image> ();	

		//FIXME: scoring gives this error. also ball offset on goal is wrong
		teamBackground [teamIndex].transform.Find ("Text - Score").GetComponent<Text> ().text = value.ToString();
	}
}
