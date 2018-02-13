
using UnityEngine;

public interface IOccupant {

	Hex CurrentHex { get; set; }

	Team Team { get; }

	Vector3 HexOffset { get; }
}
