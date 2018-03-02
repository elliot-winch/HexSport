using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	static GameManager instance;

	public static GameManager Instance {
		get {
			return instance;
		}
	}

	Action gameStart;

	public void RegisterOnGameStart(Action callback){
		gameStart += callback;
	}

	void Start(){
		if(instance != null){
			Debug.LogError ("Can only have one game manager");
		}

		instance = this;
	}

	//
	public void StartGame(){

		if (gameStart != null) {
			gameStart ();
			Debug.Log ("Starting game");
		}

		Debug.Log ("Next turn");
		NextTurn ();	
	}

	public bool CheckStartOfTurn(){
		foreach (ContestantData c in TeamManager.Instance.CurrentTeam.Contestants) {
			//Make sure zero is high enough. Perhaps check all possible actions instead?
			if (c.Contestant.ActionsRemaining > 0f) {
				return false;
			}
		}

		NextTurn ();

		return true;
	}

	public void NextTurn(){
		TeamUIManager.Instance.ClearActionsRemainingUI(TeamManager.Instance.CurrentTeam);

		TeamManager.Instance.IncTeam ();

		UserControlManager.Instance.ControlModeType = ControlModeEnum.Move;

		foreach (ContestantData c in TeamManager.Instance.CurrentTeam.Contestants) {
			c.Contestant.OnTurnBegin (c.Contestant);
		}
	
		//put a callback here

		UserControlManager.Instance.SelectFirst ();
	}


}
