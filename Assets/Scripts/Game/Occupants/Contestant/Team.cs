using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team  {

	string name;
	List<ContestantData> contestants;
	Color color;
	Sprite image;

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

	public Sprite Image {
		get {
			return image;
		}
	}

	public Team(string name, List<ContestantData> contestants, Color color, Sprite s){
		this.name = name;
		this.contestants = contestants;
		this.color = color;
		this.image = s;
	}

	public Team(string name, Color color, Sprite s) : this (name, new List<ContestantData> (), color, s)
	{
	}

	public void AddContestant(ContestantData cd){
		contestants.Add (cd);
		cd.Team = this;
	}
}
