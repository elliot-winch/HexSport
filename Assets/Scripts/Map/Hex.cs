using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {

	float moveCost = 1f;
	IOccupant occupant;
	Vector3 cubeIndex;
	HexType type;

	public Vector3 Position {
		get {
			return transform.position;
		}
	}

	public Vector3 CubeIndex {
		get {
			return cubeIndex;
		}
		set {
			cubeIndex = value;
		}
	}

	public float MoveCost{
		get {
			return moveCost;
		}
	}

	public HexType Type {
		get {
			return type;
		}
		set {
			type = value;

			switch (type) {
			case HexType.Reg:
				moveCost = 1f;
				break;
			case HexType.Speed:
				moveCost = 0.5f;
				GetComponent<MeshRenderer> ().material.color = Color.blue;
				break;
			case HexType.Wall:
				moveCost = Mathf.Infinity;
				GameObject wallObj = Instantiate (GridManager.Instance.wallPrefab, Position, Quaternion.identity, transform);
				Wall wallComp = wallObj.GetComponent<Wall> ();

				wallComp.CurrentHex = this;
				occupant = wallComp;
				break;
			case HexType.Goal:
				moveCost = Mathf.Infinity;
				GameObject goalObj = Instantiate (GridManager.Instance.goalPrefab, Position, Quaternion.identity, transform);
				Goal goalComp = goalObj.GetComponent<Goal> ();

				goalComp.CurrentHex = this;
				occupant = goalComp;
				GridManager.Instance.Goals.Add (goalComp);
				break;
			}
		}
	}

	public IOccupant Occupant {
		get {
			return occupant;
		}
		set {
			occupant = value;
			Debug.Log (Position + " " + occupant);
		}
	}

	public bool OccupantBlocksMovement {
		get {
			return Occupant != null && (Occupant is Contestant || Occupant is Wall);
		}
	}

	public bool OccupantBlocksLineOfSight {
		get {
			return type == HexType.Wall;
		}
	}
}

public enum HexType {
	Reg,
	Wall,
	Speed,
	Goal
}
