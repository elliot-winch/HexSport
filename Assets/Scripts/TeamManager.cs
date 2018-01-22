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

	public Text[] scoreTexts;

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
		teamsInMatch[new Team ("Team 2", Color.blue)] = 3;

		if (scoreTexts.Length != teamsInMatch.Count) {
			Debug.LogWarning ("Not enough / too many text fields to display teams' scores");
		}

		for (int i = 0; i < 3; i++) {
			foreach (Team t in teamsInMatch.Keys) {
				t.AddContestant (new ContestantData ());

				int tIndex = TeamsInMatch.IndexOf (t);
				scoreTexts [tIndex].text = teamsInMatch[t].ToString();

			}
		}
	}


	public void Score(Team t){
		if (teamsInMatch.ContainsKey (t)) {
			teamsInMatch [t]++;

			int tIndex = TeamsInMatch.IndexOf (t);
			scoreTexts [tIndex].text = teamsInMatch[t].ToString();
		}
	}

	public void IncTeam(){
		currentTeamIndex++;
	}
}
