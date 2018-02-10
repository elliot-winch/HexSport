using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPath {

	Stack<Hex> validPath;

	public AStarPath(Grid grid, Hex startHex, Hex endHex, bool hexValuesAffectPath){

		List<Hex> closedSet = new List<Hex> ();

		PriorityQueue<float, Hex> openSet = new PriorityQueue<float, Hex> ();
		openSet.Enqueue (0, startHex);

		Dictionary<Hex, Hex> path = new Dictionary<Hex, Hex> ();

		Dictionary<Hex, float> g_score = new Dictionary<Hex, float> ();

		foreach (Hex h in grid.Hexes.Values) {
			g_score [h] = Mathf.Infinity;
		}

		g_score [startHex] = 0;

		Dictionary<Hex, float> f_score = new Dictionary<Hex, float> ();

		foreach (Hex h in grid.Hexes.Values) {
			f_score [h] = Mathf.Infinity;
		}

		f_score [startHex] = heuristicCostEstimate (startHex, endHex);

		while (!openSet.IsEmpty) {
			Hex current = openSet.Dequeue ().Value;

			if (current == endHex) {
				RecontructPath (path, current);
				return;
			}

			closedSet.Add (current);

			List<Hex> neighbours = grid.HexNeighbours (current.GetComponent<Tile>());

			foreach (Hex neighbour in neighbours) {

				if (neighbour.MoveCost == Mathf.Infinity || neighbour.OccupantBlocksMovement) {
					continue;
				}

				if(closedSet.Contains(neighbour)){
					continue;
				}
					

				float tentative_g_score = g_score [current] + (hexValuesAffectPath ? current.MoveCost : 1f);

				if (openSet.Contains (neighbour) && tentative_g_score >= g_score [neighbour]) {
					continue;
				}

				path [neighbour] = current;
				g_score [neighbour] = tentative_g_score;
				f_score[neighbour] = g_score [neighbour] + heuristicCostEstimate (neighbour, endHex);

				if (openSet.Contains (neighbour) == false) {
					openSet.Enqueue (f_score [neighbour], neighbour);
				}
			}
		}
	}

	float heuristicCostEstimate(Hex a, Hex b){
		return Grid.Distance (a.GetComponent<Tile>(), b.GetComponent<Tile>());
	}

	void RecontructPath(Dictionary<Hex, Hex> path, Hex current){

		validPath = new Stack<Hex> ();
		Queue<Hex> pathQueue = new Queue<Hex> ();

		pathQueue.Enqueue (current);

		while(path.ContainsKey(current)) {
			current = path [current];
			pathQueue.Enqueue (current);
		}

		while (pathQueue.Count > 1) {
			validPath.Push (pathQueue.Dequeue ());
		}
	}

	public Hex GetNextHex(){
		if (validPath.Count > 0) {
			return validPath.Pop ();
		} else {
			return null;
		}
	}

	public bool IsNextHex(){
		return validPath != null && validPath.Count > 0;
	}

	public int Length(){
		return validPath.Count;
	}
}
