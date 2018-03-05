using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team  {

	string name;
	List<ContestantData> contestants;
	Color[] colors;
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

	//This is the main color
	public Color Color {
		get {
			return colors[0];
		}
	}

	public Color[] Colors {
		get {
			return colors;
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
		//calculate split complemenary colors
		float hue, sat, val;
		Color.RGBToHSV(color, out hue, out sat, out val);
		Debug.Log (hue + " " + sat + " " + val);
		Debug.Log( (hue + 150f/360)  + " " +  sat  + " " +  val);

		float hue1 = (hue + 210f / 360);
		float hue2 = (hue + 150f / 360);

		hue1 -= Mathf.Floor (hue1);
		hue2 -= Mathf.Floor (hue2);

		this.colors = new Color[3] { color, Color.HSVToRGB( hue2, sat, val), Color.HSVToRGB( hue1, sat, val)};
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
