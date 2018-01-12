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
}
