using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IOccupant {

	Hex currentHex;

	public Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			currentHex = value;
			transform.position = currentHex.Position;
		}
	}

	public Team Team {
		get {
			return null;
		}
	}
}
