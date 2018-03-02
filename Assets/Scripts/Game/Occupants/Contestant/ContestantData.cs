using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContestantData : IStats {

	Contestant contestant;

	public Contestant Contestant {
		get {
			return contestant;
		}
		set {
			contestant = value;
		}
	}

	Team team;

	public Team Team {
		get {
			return team;
		}
		set {
			team = value;
		}
	}

	string name;

	public string Name {
		get {
			return name;
		}
	}

	#region IStats implementation
	Dictionary<string,string> stats; 

	public Dictionary<string, string> Stats {
		get {
			return stats;
		}
	}
	#endregion

	//Stats
	float dexerity;
	float agility;
	float strength;

	public float Dexerity {
		get {
			return dexerity;
		}
	}

	public float Agility {
		get {
			return agility;
		}
	}

	public float Strength {
		get {
			return strength;
		}
	}

	public ContestantData(string name, float dex, float agility, float strength, int startingHealth){
		this.name = name;
		this.dexerity = dex;
		this.agility = agility;
		this.strength = strength;

		stats = new Dictionary<string, string> ();

		stats ["Title"] = name;
		stats ["Dexerity"] = dexerity.ToString();
		stats ["Agility"] = agility.ToString();
		stats ["Strength"] = strength.ToString();
	}

	/*
	public void EquipArmour(){

	}

	public void EquipWeapon(){

	}
*/

	//test!
	public bool CanShoot{
		get {
			return true;
		}
	}
}
