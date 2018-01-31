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

	//Stats
	float dexerity;
	float agility;
	float strength;
	Health health;

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
		this.health = new Health (startingHealth);
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
			return false;
		}
	}
}
