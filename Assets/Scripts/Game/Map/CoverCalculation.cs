using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is an experimental class. It is unused in the current build. 
 * 
 * It is attempting to more fairly and precisely calculate obstacles to line of sight
 * 
 */ 


public class CoverCalculation {


	public static void CalculateObstructions(Hex start, Hex end){
		List<Hex> l = GridBetweenPoints (GridManager.Instance.Grid, start, end);

		foreach (Hex h in l) {
			h.GetComponent<MeshRenderer> ().material.color = Color.blue;
		}


	}
		
	//Need a way to limit number of hexes searched to just small area
	static List<List<Hex>> GetAllPaths(Grid grid, Hex start, Hex end){

		Queue<List<Hex>> openSet = new Queue<List<Hex>> ();
		List<List<Hex>> closedSet = new List<List<Hex>> ();
		List<Hex> visited = new List<Hex> ();

		List<Hex> startList = new List<Hex> ();
		startList.Add (start);

		openSet.Enqueue (startList);

		while(openSet.Count > 0){

			List<Hex> p = openSet.Dequeue ();

			Hex lastInP = p.Last ();

			if (lastInP == end) {
				closedSet.Add (p);
				continue;
			}

			List<Hex> neighbours = grid.HexNeighbours (lastInP.GetComponent<Tile>());

			visited.Add (lastInP);

			foreach (Hex h in neighbours) {
				if (visited.Contains (h) == false) {

					List<Hex> newPath = new List<Hex> (p);
					newPath.Add (h);

					openSet.Enqueue (newPath);
				}
			}
		}

		return closedSet;
	}


	static List<Hex> GridBetweenPoints(Grid grid, Hex start, Hex end){

		List<Hex> hexes = new List<Hex> ();

		CubeIndex startCoord = start.CubeIndex;
		CubeIndex endCoord = end.CubeIndex;

		int startX = 0;
		int endX = 0;
		if (startCoord.x < endCoord.x) {
			startX = startCoord.x;
			endX = endCoord.x;
		} else {
			startX = endCoord.x;
			endX = startCoord.x;
		}

		int startY = 0;
		int endY = 0;
		if (startCoord.z < endCoord.z) {
			startY = startCoord.z;
			endY = endCoord.z;
		} else {
			startY = endCoord.z;
			endY = startCoord.z;
		}

		 
		for (int i = startX; i < endX; i++) {
			for (int j = startY; j < endY; j++) {
				hexes.Add (grid.TileAt (new CubeIndex (i, j)).GetComponent<Hex> ());
			}
		}

		return hexes;
	}
}


