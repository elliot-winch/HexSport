using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

	public GameObject wallPrefab;
	public GameObject goalPrefab;
	public int gridLength = 30;
	public int gridWidth = 20;

	static GridManager instance;

	Grid grid;

	List<IOccupant> pitchObjects;

	public static GridManager Instance {
		get {
			return instance;
		}
	}

	public Grid Grid {
		get {
			return grid;
		}
	}

	public List<IOccupant> Goals {
		get {
			return pitchObjects;
		}
	}

	void Start(){
		instance = this;

		GameObject board = new GameObject ();

		board.name = "Board";

		grid = board.AddComponent<Grid> ();

		grid.mapShape = MapShape.Rectangle;
		grid.mapWidth = gridWidth;
		grid.mapHeight = gridLength;

		grid.GenerateGrid ();

		pitchObjects = new List<IOccupant> ();

		List<Hex> wall;
		DrawLineOnGrid (Grid.TileAt(8, -10).GetComponent<Hex>(), Grid.TileAt(10, -5).GetComponent<Hex>(), out wall);

		foreach (Hex h in wall) {
			h.Type = HexType.Wall;
		}
	}
		
	//Takes blocked line of sight into account
	public bool DrawLineOnGrid(Hex start, Hex end,int cubeDist = -1){
		List<Hex> unused;
		return DrawLineOnGrid (start, end, out unused, cubeDist);
	}

	public bool DrawLineOnGrid(Hex start, Hex end, out List<Hex> hexes, int cubeDist = -1){
		if (cubeDist < 0) {
			cubeDist = Grid.Distance (start.GetComponent<Tile> (), end.GetComponent<Tile> ());
		}

		hexes = new List<Hex>();

		RaycastHit hitInfo;
		for (int i = 0; i <= cubeDist; i++) {
			Vector3 pos = Vector3.Lerp(start.Position, end.Position, (1f/cubeDist) * i) + new Vector3 (0, 0.1f, 0);

			if (Physics.Raycast (pos, -Vector3.up, out hitInfo, 1<<LayerMask.NameToLayer("Hex")) && hitInfo.collider.tag == "Hex") {
				Hex h = hitInfo.collider.GetComponent<Hex> ();

				if (h.OccupantBlocksLineOfSight == false) {
					hexes.Add (h);
				} else {
					hexes = null;
					return false;
				}
			} else {
				hexes = null;
				return false;
			}
		}

		return true;
	}
}
