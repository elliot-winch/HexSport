using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamTransfer : MonoBehaviour {

	List<Team> teams;

	public List<Team> Teams {
		get {
			return teams;
		}
		set {
			teams = value;
		}
	}
}
