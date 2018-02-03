using System.Linq;
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

	void Start(){
		if (instance != null) {
			Debug.LogError ("There should not be more than one team manager");
		}

		instance = this;

		teamsInMatch = new Dictionary<Team, int> ();

		Team one = new Team ("Team 1", Color.red);
		Team two = new Team ("Team 2", Color.blue);

		teamsInMatch [one] = 0;
		teamsInMatch [two] = 0;

		one.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
		one.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
		one.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
		two.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
		two.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));
		two.AddContestant (new ContestantData ("R", 8f, 1f, 1f, 1));

		/*
		List<Team> createdTeams = GameObject.Find ("Team Selection Data").GetComponent<TeamTransfer>().Teams;

		foreach (Team t in createdTeams) {
			Debug.Log (t.Contestants.Count);
			teamsInMatch [t] = 0;

		}
		* this is the real shit */
	}


	public void Score(Team t){
		if (teamsInMatch.ContainsKey (t)) {
			teamsInMatch [t]++;

			TeamUIManager.Instance.UpdateScoreUI (TeamsInMatch.IndexOf (t), teamsInMatch[t]);
		}
	}

	public void IncTeam(){
		currentTeamIndex++;
	}
}
