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


	List<Hex> ballStartHexes;


	// Use this for initialization
	void Start () {
		if(instance != null){
			Debug.LogError ("Can only have one game manager");
		}

		instance = this;

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
				((Goal)goalHexes [i].Occupant).Team = TeamManager.Instance.TeamsInMatch [0];
				goalHexes [i].transform.Rotate (new Vector3 (0f, 180f, 0f));
			} else {
				((Goal)goalHexes [i].Occupant).Team = TeamManager.Instance.TeamsInMatch [1];
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

		SpawnBall ();

/*		//counter is temp. in fact, this will all be temp as playes choose where to spawn
		int counter = 9;
		foreach (Team t in TeamManager.Instance.TeamsInMatch) {
			foreach (ContestantData d in t.Contestants) {
				SpawnContestant (d, GridManager.Instance.Grid.TileAt (2 + counter, -7 - counter).GetComponent<Hex> ());
				counter++;
			}
		}
		*/

		UserControlManager.Instance.ControlModeType = ControlModeEnum.Placement;
	}
	
	public void SpawnContestant (ContestantData d, Hex startingHex) {
		//prefab is temp, will load prefab of correct type
		GameObject go = Instantiate (testContestant, startingHex.Position, testContestant.transform.rotation, transform);
		go.transform.GetChild(0).GetComponent<MeshRenderer> ().material.color = d.Team.Color;	

		//temporary contestant factory
		Contestant c = go.GetComponent<Contestant> ();
		//Here set contestant values in line with data
		c.CurrentHex =  startingHex;
		c.Position = startingHex.Position;
		c.Data = d;

		d.Contestant = c;


		//Possible Actions
		if (d.CanShoot) {
			c.PossibleActions.Add (ContestantActionsFactory.CreateAction<IOccupant> ("Shoot", ContestantActionsEnum.Shoot, c, 4, true));
		}

		Func<ICatcher, bool> throwReqs = (con) => {

			return c.Ball != null && con.Ball == null;
		};

		c.PossibleActions.Add (ContestantActionsFactory.CreateAction<ICatcher> ("Throw", ContestantActionsEnum.Throw, c, (int)(d.Dexerity * 2), true, throwReqs));

		Func<Contestant, bool> checkForBall = (Contestant con) => {
			return con.Ball != null;
		};

		c.PossibleActions.Add ( ContestantActionsFactory.CreateAction<Contestant> ("Swipe", ContestantActionsEnum.Swipe, c, 1, false, checkForBall));

		UIManager.Instance.CreateButtonPool (c);
	}


	public void SpawnBall(){

		Hex ballStartHex = ballStartHexes [UnityEngine.Random.Range (0, ballStartHexes.Count)];
		GameObject ball = Instantiate (ballPrefab, Vector3.zero, Quaternion.identity);

		ball.GetComponent<Ball> ().CurrentHex = ballStartHex;
	}

	public bool CheckStartOfTurn(){

		foreach (ContestantData c in TeamManager.Instance.CurrentTeam.Contestants) {
			//Make sure zero is high enough. Perhaps check all possible actions instead?
			if (c.Contestant.MovesRemaining > 0f) {
				return false;
			}
		}

		NextTurn ();

		return true;
	}

	public void NextTurn(){
		TeamManager.Instance.IncTeam ();

		UserControlManager.Instance.ControlModeType = ControlModeEnum.Move;

		foreach (ContestantData c in TeamManager.Instance.CurrentTeam.Contestants) {
			c.Contestant.OnTurnBegin (c.Contestant);
		}
	
		UserControlManager.Instance.SelectFirst ();
	}

	void SpawnLineOf(HexType type, Hex start, Hex end){

		List<Hex> hexes;
		GridManager.Instance.DrawLineOnGrid (start, end, out hexes);

		foreach (Hex h in hexes) {
			h.Type = type;
		}
	}

}
