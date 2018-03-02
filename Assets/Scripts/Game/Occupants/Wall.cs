using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : DamagableObject {

	public override Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			currentHex = value;
			transform.position = currentHex.Position + HexOffset;
		}
	}

	public override Vector3 HexOffset {
		get {
			return new Vector3 (0f, GetComponent<MeshRenderer> ().bounds.extents.y, 0f);
		}
	}

}
