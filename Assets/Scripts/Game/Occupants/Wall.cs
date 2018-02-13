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
			transform.position = currentHex.Position + HexOffset;
		}
	}

	public Team Team {
		get {
			return null;
		}
	}

	public Vector3 HexOffset {
		get {
			return new Vector3 (0f, GetComponent<MeshRenderer> ().bounds.extents.y, 0f);
		}
	}

}
