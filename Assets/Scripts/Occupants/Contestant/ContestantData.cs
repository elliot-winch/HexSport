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

	string name;

	public string Name {
		get {
			return name;
		}
	}

	//Stats
	float dexerity;
	float agility;
	float strength;
	float weaponsHandling;
	float health;

	public float Dexerity {
		get {
			return dexerity;
		}
	}

	public ContestantData(string name){
		this.name = name;
		this.dexerity = 6f;
		this.agility = 3f;
	}

	public ContestantData(string name, float dex, float agility){
		this.name = name;
		this.dexerity = dex;
		this.agility = agility;
	}


	//test!
	public bool CanShoot{
		get {
			return true;
		}
	}
}
