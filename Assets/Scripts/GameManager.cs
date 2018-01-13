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

	public GameObject testContestant;
	public GameObject ballPrefab;

	Dictionary<Team, int> teamsInMatch;
	int currentTeamIndex;

	public Team CurrentTeam {
		get {
			return teamsInMatch.Keys.ToArray() [(currentTeamIndex % teamsInMatch.Keys.Count)];
		} 
	}

	// Use this for initialization
	void Start () {
		if(instance != null){
			Debug.LogError ("Can only have one game manager");
		}

		instance = this;

		teamsInMatch = new Dictionary<Team, int> ();

		teamsInMatch[new Team ("Team 1", new List<ContestantData>() { new ContestantData(), new ContestantData(), new ContestantData()}, Color.red)] = 0;
		teamsInMatch[new Team ("Team 2", new List<ContestantData>() { new ContestantData(), new ContestantData(), new ContestantData()}, Color.blue)] = 0;

		int i = 0;
		foreach (Team t in teamsInMatch.Keys.ToList()) {
			foreach (ContestantData d in t.Contestants) {
				d.Team = t;
				SpawnContestant (d, GridManager.Instance.Grid.TileAt (2 * i, - 2 * i).GetComponent<Hex> ());
				i++;
			}
		}

		SpawnBall ();

		CheckStartOfTurn ();

		//temp!
		GridManager.Instance.Grid.TileAt (15, -16).GetComponent<Hex> ().Type = HexType.Goal;
		((Goal)GridManager.Instance.Grid.TileAt (15, -16).GetComponent<Hex> ().Occupant).Team = CurrentTeam;
	}
	
	void SpawnContestant (ContestantData d, Hex startingHex) {
		//prefab is temp, will load prefab of correct type
		//positions will load from map data

		GameObject go = Instantiate (testContestant, startingHex.Position , Quaternion.identity, transform);
		go.GetComponent<MeshRenderer> ().material.color = d.Team.Color;	

		Contestant c = go.GetComponent<Contestant> ();
		//Here set contestant values in line with data
		c.CurrentHex =  startingHex;
		c.Data = d;

		d.Contestant = c;
	}

	public void Score(Team t){
		if (teamsInMatch.ContainsKey (t)) {
			teamsInMatch [t]++;
		}
	}

	public void SpawnBall(){
		Hex ballStartHex = GridManager.Instance.Grid.TileAt (5, -5).GetComponent<Hex>();
		GameObject ball = Instantiate (ballPrefab, Vector3.zero, Quaternion.identity);

		ball.GetComponent<Ball> ().CurrentHex = ballStartHex;
	}

	public bool CheckStartOfTurn(){

		foreach (ContestantData c in CurrentTeam.Contestants) {
			//Make sure zero is high enough. Perhaps check all possible actions instead?
			if (c.Contestant.MovesRemaining > 0) {
				return false;
			}
		}

		currentTeamIndex++;

		UserControlManager.Instance.ModeType = ControlModeEnum.Move;

		foreach (ContestantData c in CurrentTeam.Contestants) {
			c.Contestant.OnTurnBegin (c.Contestant);
		}

		return true;
	}



}
