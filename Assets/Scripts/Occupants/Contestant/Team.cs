using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team  {

	string name;
	List<ContestantData> contestants;
	Color color;

	public string Name {
		get {
			return name;
		}
	}

	public List<ContestantData> Contestants {
		get {
			return contestants;
		}
	}

	public Color Color {
		get {
			return color;
		}
	}

	public Team(string name, List<ContestantData> contestants, Color color){
		this.name = name;
		this.contestants = contestants;
		this.color = color;
	}

	public Team(string name, Color color){
		this.name = name;
		this.contestants = new List<ContestantData>();
		this.color = color;
	}

	public void AddContestant(ContestantData cd){
		contestants.Add (cd);
		cd.Team = this;
	}
}
