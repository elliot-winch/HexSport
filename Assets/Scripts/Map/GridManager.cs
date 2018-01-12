using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour {

	public GameObject wallPrefab;
	public int gridLength = 30;
	public int gridWidth = 20;

	static GridManager instance;

	Grid grid;

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

	void Start(){
		instance = this;

		GameObject board = new GameObject ();

		board.name = "Board";

		grid = board.AddComponent<Grid> ();

		grid.mapShape = MapShape.Rectangle;
		grid.mapWidth = gridWidth;
		grid.mapHeight = gridLength;

		grid.GenerateGrid ();

		/*
		for(int i = 0; i < gridWidth; i+=5) {
			for (int j = 0; j < 5; j++) {
				grid.TileAt(i,-j).GetComponent<Hex>().Type = HexType.Wall;
			}

			for (int j = gridLength - 5; j < gridLength; j++) {
				grid.TileAt(i,-j).GetComponent<Hex>().Type = HexType.Wall;
			}
		}*/
	}
}
