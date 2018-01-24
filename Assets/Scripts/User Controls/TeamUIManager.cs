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


	public Text[] scoreTexts;
	public Image[] teamNames;
	public GameObject playerNamePrefab;


	void Start () {
		if (instance != null) {
			Debug.LogError ("There should not be more than one team UI manager");
		}
			
		List<Team> tim = TeamManager.Instance.TeamsInMatch;

		if (scoreTexts.Length != tim.Count) {
			Debug.LogWarning ("Not enough / too many text fields to display teams' scores");
		}

		if (teamNames.Length != tim.Count) {
			Debug.LogWarning ("Not enough / too many text fields to display teams' names");
		}

		for(int i = 0; i < tim.Count; i++) {

			teamNames [i].color = new Color (tim [i].Color.r, tim [i].Color.g, tim [i].Color.b, 45f / 256); 
			teamNames [i].transform.GetChild (0).GetComponent<Text> ().text = tim [i].Name;

			//fill in score UI here, but for now it will always start at zero
			for (int j = 0; j < tim[i].Contestants.Count; j++) {

				GameObject bObj = Instantiate(playerNamePrefab, Vector3.zero, Quaternion.identity, UIManager.Instance.mainCanvas.transform);

				Rect textRect = playerNamePrefab.GetComponent<RectTransform> ().rect;

				Vector2 pos = new Vector2 ((-(Screen.width / 2) + (textRect.width / 2)) + (i * (Screen.width - textRect.width )) , 130 - (j * textRect.height));

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
		scoreTexts [teamIndex].text = value.ToString();
	}
}
