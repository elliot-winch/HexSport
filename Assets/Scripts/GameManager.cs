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

	List<Hex> ballStartHexes;

	public Team CurrentTeam {
		get {
			return TeamsInMatch [(currentTeamIndex % teamsInMatch.Keys.Count)];
		} 
	}

	public Team[] TeamsInMatch {
		get {
			return teamsInMatch.Keys.ToArray ();
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

		ballStartHexes = new List<Hex> ();
		for(int k = 1; k <= 9; k++){
			ballStartHexes.Add(GridManager.Instance.Grid.TileAt(k * 2, -15 - k).GetComponent<Hex>());
		}

		//Spawn Pitch which reads from file when i have more than one pitch
		Grid currentGrid = GridManager.Instance.Grid;

		Hex[] goalHexes = new Hex[] { 
			currentGrid.TileAt(3, -5).GetComponent<Hex>(), 
			currentGrid.TileAt(10, -6).GetComponent<Hex>(), 
			currentGrid.TileAt(17, -12).GetComponent<Hex>(), 
			currentGrid.TileAt(3, -28).GetComponent<Hex>(), 
			currentGrid.TileAt(10, -34).GetComponent<Hex>(), 
			currentGrid.TileAt(17, -35).GetComponent<Hex>(), 
		};

		for (int i = 0; i < goalHexes.Length; i++) {
			goalHexes [i].Type = HexType.Goal;

			if (i < goalHexes.Length / 2) {
				((Goal)goalHexes [i].Occupant).Team = TeamsInMatch [0];
				goalHexes [i].transform.Rotate (new Vector3 (0f, 180f, 0f));
			} else {
				((Goal)goalHexes [i].Occupant).Team = TeamsInMatch [1];
			}
		}

		SpawnLineOf(HexType.Wall, currentGrid.TileAt(3, -14).GetComponent<Hex>(), currentGrid.TileAt(8, -14).GetComponent<Hex>());
		SpawnLineOf(HexType.Wall, currentGrid.TileAt(17, -21).GetComponent<Hex>(), currentGrid.TileAt(12, -16).GetComponent<Hex>());
		SpawnLineOf(HexType.Wall, currentGrid.TileAt(3, -19).GetComponent<Hex>(), currentGrid.TileAt(8, -24).GetComponent<Hex>());
		SpawnLineOf(HexType.Wall, currentGrid.TileAt(17, -26).GetComponent<Hex>(), currentGrid.TileAt(12, -26).GetComponent<Hex>());

		SpawnLineOf(HexType.Speed, currentGrid.TileAt(1, -20).GetComponent<Hex>(), currentGrid.TileAt(1, -11).GetComponent<Hex>());
		SpawnLineOf(HexType.Speed, currentGrid.TileAt(19, -29).GetComponent<Hex>(), currentGrid.TileAt(19, -20).GetComponent<Hex>());
		SpawnLineOf(HexType.Speed, currentGrid.TileAt(10, -23).GetComponent<Hex>(), currentGrid.TileAt(10, -27).GetComponent<Hex>());
		SpawnLineOf(HexType.Speed, currentGrid.TileAt(10, -13).GetComponent<Hex>(), currentGrid.TileAt(10, -17).GetComponent<Hex>());

		TeamsInMatch [0].Contestants [0].Team = TeamsInMatch [0];
		SpawnContestant (TeamsInMatch[0].Contestants[0], GridManager.Instance.Grid.TileAt (2, -7).GetComponent<Hex> ());
		TeamsInMatch [0].Contestants [1].Team = TeamsInMatch [0];
		SpawnContestant (TeamsInMatch[0].Contestants[1], GridManager.Instance.Grid.TileAt (10, -11).GetComponent<Hex> ());
		TeamsInMatch [0].Contestants [2].Team = TeamsInMatch [0];
		SpawnContestant (TeamsInMatch[0].Contestants[2], GridManager.Instance.Grid.TileAt (18, -15).GetComponent<Hex> ());

		TeamsInMatch [1].Contestants [0].Team = TeamsInMatch [1];
		SpawnContestant (TeamsInMatch[1].Contestants[0], GridManager.Instance.Grid.TileAt (2, -25).GetComponent<Hex> ());
		TeamsInMatch [1].Contestants [1].Team = TeamsInMatch [1];
		SpawnContestant (TeamsInMatch[1].Contestants[1], GridManager.Instance.Grid.TileAt (10, -29).GetComponent<Hex> ());
		TeamsInMatch [1].Contestants [2].Team = TeamsInMatch [1];
		SpawnContestant (TeamsInMatch[1].Contestants[2], GridManager.Instance.Grid.TileAt (18, -33).GetComponent<Hex> ());

		SpawnBall ();

		CheckStartOfTurn ();

	}
	
	void SpawnContestant (ContestantData d, Hex startingHex) {
		//prefab is temp, will load prefab of correct type
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

		Hex ballStartHex = ballStartHexes [UnityEngine.Random.Range (0, ballStartHexes.Count)];
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

	void SpawnLineOf(HexType type, Hex start, Hex end){

		List<Hex> hexes;
		GridManager.Instance.DrawLineOnGrid (start, end, out hexes);

		foreach (Hex h in hexes) {
			h.Type = type;
		}
	}

}
