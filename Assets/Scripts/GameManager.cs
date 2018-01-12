using System;
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

	List<Team> teamsInMatch;
	int currentTeamIndex;

	public Team CurrentTeam {
		get {
			return teamsInMatch [(currentTeamIndex % teamsInMatch.Count)];
		} 
	}

	// Use this for initialization
	void Start () {
		if(instance != null){
			Debug.LogError ("Can only have one game manager");
		}

		instance = this;

		teamsInMatch = new List<Team> ();

		teamsInMatch.Add (new Team ("Team 1", new List<ContestantData>() { new ContestantData(), new ContestantData() }, Color.red));
		teamsInMatch.Add (new Team ("Team 2", new List<ContestantData>() { new ContestantData(), new ContestantData() }, Color.blue));

		int i = 0;
		foreach (Team t in teamsInMatch) {
			foreach (ContestantData d in t.Contestants) {
				d.Team = t;
				SpawnContestant (d, GridManager.Instance.Grid.TileAt (2 * i, - 2 * i).GetComponent<Hex> ());
				i++;
			}
		}

		Hex ballStartHex = GridManager.Instance.Grid.TileAt (5, -5).GetComponent<Hex>();
		GameObject ball = Instantiate (ballPrefab, Vector3.zero, Quaternion.identity);

		ball.GetComponent<Ball> ().CurrentHex = ballStartHex;

		CheckStartOfTurn ();


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

	public bool CheckStartOfTurn(){

		foreach (ContestantData c in CurrentTeam.Contestants) {
			//Make sure zero is high enough. Perhaps check all possible actions instead?
			if (c.Contestant.MovesRemaining > 0) {
				return false;
			}
		}

		currentTeamIndex++;

		foreach (ContestantData c in CurrentTeam.Contestants) {
			c.Contestant.OnTurnBegin (c.Contestant);
		}

		return true;
	}

	public List<Contestant> GetValidTargets(Contestant con, int range, bool friendlyTeam, Func<bool> additionalChecks = null){

		List<Contestant> possibleTargets = new List<Contestant> ();
		List<Contestant> targets = new List<Contestant> ();

		if (friendlyTeam) {
			foreach(ContestantData cd in con.Data.Team.Contestants){
				if (cd == con.Data) {
					continue;
				}

				possibleTargets.Add (cd.Contestant);
			}
		} else {
			foreach (Team t in teamsInMatch) {
				if (t == con.Data.Team) {
					continue;
				}

				foreach (ContestantData cd in t.Contestants) {
					possibleTargets.Add (cd.Contestant);
				}
			}
		}

		foreach (Contestant posTarg in possibleTargets) {

			//Range check
			int cubeDist = Grid.Distance (con.CurrentHex.GetComponent<Tile> (), posTarg.CurrentHex.GetComponent<Tile> ());
			Debug.Log ("Cube Dist: " + cubeDist);

			if (cubeDist > range) {
				continue;
			}
				
			if(DrawLineOnGrid(con.CurrentHex, posTarg.CurrentHex)){
				targets.Add (posTarg);
				Debug.Log (posTarg.name);

			}
		}

		return targets;
	}

	//Takes blocked line of sight into account
	public bool DrawLineOnGrid(Hex start, Hex end,int cubeDist = -1){
		List<Hex> unused;
		return DrawLineOnGrid (start, end, out unused, cubeDist);
	}

	public bool DrawLineOnGrid(Hex start, Hex end, out List<Hex> hexes, int cubeDist = -1){
		if (cubeDist < 0) {
			cubeDist = Grid.Distance (start.GetComponent<Tile> (), end.GetComponent<Tile> ());
		}

		hexes = new List<Hex>();

		RaycastHit hitInfo;
		for (int i = 0; i <= cubeDist; i++) {
			Vector3 pos = Vector3.Lerp(start.Position, end.Position, (1f/cubeDist) * i) + new Vector3 (0, 0.1f, 0);

			if (Physics.Raycast (pos, -Vector3.up, out hitInfo, 1<<LayerMask.NameToLayer("Hex")) && hitInfo.collider.tag == "Hex") {
				Hex h = hitInfo.collider.GetComponent<Hex> ();

				if (h.OccupantBlocksLineOfSight == false) {
					hexes.Add (h);
				}
			} else {
				hexes = null;
				return false;
			}
		}

		return true;
	}
}
