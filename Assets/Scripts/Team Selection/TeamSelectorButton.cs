using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectorButton<T> : Button {

	T data;
	Team team;

	public T Data {
		get {
			return data;
		}
		set {
			if (data == null) {
				data = value;
			} else {
				Debug.LogError ("Attempting to overwrite data stored in a TeamSelectorButton");
			}
		}
	}

	public Team Team {
		get {
			return team;
		} 
		set {
			team = value;
		}
	}
}
