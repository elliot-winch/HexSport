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

		teamsInMatch[new Team ("Team 1", Color.red)] = 0;
		teamsInMatch[new Team ("Team 2", Color.blue)] = 0;

	

		for (int i = 0; i < 3; i++) {
			foreach (Team t in teamsInMatch.Keys) {
				t.AddContestant (new ContestantData ("Dave" + i.ToString(), i, i, i, i));
			}
		}

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
