﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamManager : MonoBehaviour {

	static TeamManager instance;

	public static TeamManager Instance {
		get {
			return instance;
		}
	}

	Dictionary<Team, int> teamsInMatch;
	int currentTeamIndex;

	public Team CurrentTeam {
		get {
			return TeamsInMatch [(currentTeamIndex % teamsInMatch.Keys.Count)];
		} 
	}

	public List<Team> TeamsInMatch {
		get {
			return teamsInMatch.Keys.ToList ();
		}
	}

	//Good: independent of the UserControlMode
	//Bad: calculate numCons each time (which, with ContestantStatUIManager is every frame
	List<Contestant> all = null;
	public List<Contestant> AllContestants{
		get {
			int numCons = 0;

			foreach (Team t in TeamsInMatch) {
				numCons += t.Contestants.Count;
			}
				
			if (all != null && all.Count == numCons) {
				return all;
			} else {
				List<Contestant> cons = new List<Contestant> ();

				foreach (Team t in TeamsInMatch) {
					foreach (ContestantData c in t.Contestants) {
						if (c.Contestant != null) {
							cons.Add (c.Contestant);
						}
					}
				}

				all = cons;
				return cons;
			}
		}
	}

	void Start(){
		if (instance != null) {
			Debug.LogError ("There should not be more than one team manager");
		}

		instance = this;

		teamsInMatch = new Dictionary<Team, int> ();


		GameObject teamSelectionData = GameObject.Find ("Team Selection Data");

		if(teamSelectionData != null){
			List<Team> createdTeams = teamSelectionData.GetComponent<TeamTransfer>().Teams;

			foreach (Team t in createdTeams) {
				teamsInMatch [t] = 0;
			}
		} else {
			//this is the testing case for when there was no selection
			Debug.Log("testing case");

			Team one = new Team ("Team 1", Color.red, new Sprite());
			Team two = new Team ("Team 2", Color.blue, new Sprite());

			one.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
			one.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
			one.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
			two.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
			two.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
			two.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));

			teamsInMatch [one] = 0;
			teamsInMatch [two] = 0;

		}
	}


	public void Score(Team t){
		if (teamsInMatch.ContainsKey (t)) {
			teamsInMatch [t]++;

			TeamUIManager.Instance.UpdateScoreUI (t, teamsInMatch[t]);
		}
	}

	public void IncTeam(){
		currentTeamIndex++;
	}
}
