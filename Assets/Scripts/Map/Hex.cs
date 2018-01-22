using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {

	float moveCost = 1f;
	IOccupant occupant;
	HexType type;
	GameObject spawnedObject;

	public Vector3 Position {
		get {
			return transform.position;
		}
	}

	public CubeIndex CubeIndex {
		get {
			return GetComponent<Tile>().index;
		}
	}

	public float MoveCost{
		get {
			return moveCost;
		}
	}

	public GameObject SpawnedObject {
		get {
			return spawnedObject;
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
				GetComponent<MeshRenderer> ().material = GridManager.Instance.speedHexMat;
				break;
			case HexType.Wall:
				moveCost = Mathf.Infinity;
				spawnedObject = Instantiate (GridManager.Instance.wallPrefab, Position, Quaternion.identity, transform);
				Wall wallComp = spawnedObject.GetComponent<Wall> ();

				wallComp.CurrentHex = this;
				occupant = wallComp;
				break;
			case HexType.Goal:
				moveCost = Mathf.Infinity;
				spawnedObject = Instantiate (GridManager.Instance.goalPrefab, Position, Quaternion.identity, transform);
				Goal goalComp = spawnedObject.GetComponent<Goal> ();

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
