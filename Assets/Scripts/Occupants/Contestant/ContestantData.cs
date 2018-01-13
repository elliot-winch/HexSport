using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContestantData {

	Contestant contestant;

	public Contestant Contestant {
		get {
			return contestant;
		}
		set {
			if (contestant == null) {
				contestant = value;
			}
		}
	}

	Team team;

	public Team Team {
		get {
			return team;
		}
		set {
			if (team == null) {
				team = value;
			}
		}
	}

	//Stats
	float dexerity;
	float agility;
	float strength;
	float firearms;
	float health;

	public float Dexerity {
		get {
			return dexerity;
		}
	}

	public ContestantData(){
		this.dexerity = 6f;
		this.agility = 3f;
	}

	public ContestantData(Contestant con, Team team, float dex, float agility){
		this.contestant = con;
		this.team = team;
		this.dexerity = dex;
		this.agility = agility;
	}

}
