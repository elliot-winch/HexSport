using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class Grid : MonoBehaviour {
	public static Grid inst;

	//Map settings
	public MapShape mapShape = MapShape.Rectangle;
	public int mapWidth;
	public int mapHeight;

	//Hex Settings
	public HexOrientation hexOrientation = HexOrientation.Flat;
	public float hexRadius = 1;
	public Material hexMaterial;
	
	//Generation Options
	public bool addColliders = true;
	public bool drawOutlines = true;
	public Material lineMaterial;

	//Internal variables
	internal Dictionary<string, Tile> grid = new Dictionary<string, Tile>();
	internal Dictionary<Tile, Hex> hexes = new Dictionary<Tile, Hex> ();
	internal Mesh hexMesh = null;
	internal CubeIndex[] directions = 
		new CubeIndex[] {
			new CubeIndex(1, -1, 0), 
			new CubeIndex(1, 0, -1), 
			new CubeIndex(0, 1, -1), 
			new CubeIndex(-1, 1, 0), 
			new CubeIndex(-1, 0, 1), 
			new CubeIndex(0, -1, 1)
		}; 

	#region Getters and Setters
	public Dictionary<string, Tile> Tiles {
		get {return grid;}
	}

	public Dictionary<Tile, Hex> Hexes {
		get {
			return hexes;
		}
	}
	#endregion

	#region Public Methods
	public void GenerateGrid() {
		//Generating a new grid, clear any remants and initialise values
		ClearGrid();
		GetMesh();

		//Generate the grid shape
		switch(mapShape) {
		case MapShape.Hexagon:
			GenHexShape();
			break;

		case MapShape.Rectangle:
			GenRectShape();
			break;

		case MapShape.Parrallelogram:
			GenParrallShape();
			break;

		case MapShape.Triangle:
			GenTriShape();
			break;

		default:
			break;
		}
	}

	public void ClearGrid() {
		Debug.Log ("Clearing grid...");
		foreach(var tile in grid)
			DestroyImmediate(tile.Value.gameObject, false);
	
		grid.Clear();
	}

	public Tile TileAt(CubeIndex index){
		if(grid.ContainsKey(index.ToString()))
		   return grid[index.ToString()];
		return null;
	}

	public Tile TileAt(int x, int y, int z){
		return TileAt(new CubeIndex(x,y,z));
	}

	public Tile TileAt(int x, int z){
		return TileAt(new CubeIndex(x,z));
	}

	public List<Tile> Neighbours(Tile tile) {
		List<Tile> ret = new List<Tile>();
		CubeIndex o;

		for(int i = 0; i < 6; i++) {
			o = tile.index + directions[i];
			if(grid.ContainsKey(o.ToString()))
				ret.Add(grid[o.ToString()]);
		}
		return ret;
	}

	public List<Tile> Neighbours(CubeIndex index){
		return Neighbours(TileAt(index));
	}

	public List<Tile> Neighbours(int x, int y, int z){
		return Neighbours(TileAt(x,y,z));
	}

	public List<Tile> Neighbours(int x, int z){
		return Neighbours(TileAt(x,z));
	}

	public List<Hex> HexNeighbours(Tile tile){
		List<Tile> tiles = Neighbours (tile);

		List<Hex> hexes = new List<Hex> ();

		foreach (Tile t in tiles) {
			hexes.Add (t.GetComponent<Hex> ());
		}

		return hexes;
	}

	//BreathFirstSearch includes obstaclues
	public List<Hex> HexesInRangeAccountingObstacles(Hex center, float range){
		List<Hex> inRange = new List<Hex> ();
		List<Hex> openSet = new List<Hex> ();
		Dictionary<Hex, float> distanceFromCenter = new Dictionary<Hex, float> ();
		//distanceFrom.Keys acts as closed set

		inRange.Add (center);
		openSet.Add (center);
		distanceFromCenter [center] = 0f;

		Hex current;
		List<Hex> neighbours;
		while (openSet.Count > 0) {
			current = openSet [0];
			openSet.RemoveAt (0);

			neighbours = HexNeighbours (current.GetComponent<Tile> ());

			foreach (Hex h in neighbours) {
				if (distanceFromCenter.ContainsKey (h)) {
					if (distanceFromCenter [current] + h.MoveCost <= distanceFromCenter [h]) {
						distanceFromCenter [h] = distanceFromCenter [current] + h.MoveCost;
					}
				} else if (distanceFromCenter [current] + h.MoveCost <= range && h.OccupantBlocksMovement == false) {
					inRange.Add (h);
					openSet.Add (h);
					distanceFromCenter [h] = distanceFromCenter [current] + h.MoveCost;
				}
			}
		}

		return inRange;
	}

	//BreathFirstSearch but stratifies the results
	//Potential fix, combine this and HexesInRangeAccountingObstacles, bc they basically do the same thing
	public List<List<Hex>> HexesInRangeSegmentedByActions(Hex center, int numAction, float rangePerAction){

		Queue<Hex> openSet = new Queue<Hex> ();
		Dictionary<Hex, float> hexDistanceFromCenter = new Dictionary<Hex, float> ();
		//distanceFrom.Keys acts as closed set

		openSet.Enqueue (center);
		hexDistanceFromCenter [center] = 0f;

		Hex current;
		List<Hex> neighbours;
		while (openSet.Count > 0) {
			current = openSet.Dequeue ();

			neighbours = HexNeighbours (current.GetComponent<Tile> ());

			foreach (Hex h in neighbours) {
				if (hexDistanceFromCenter.ContainsKey (h)) {
					if (hexDistanceFromCenter [current] + h.MoveCost <= hexDistanceFromCenter [h]) {
						hexDistanceFromCenter [h] = hexDistanceFromCenter [current] + h.MoveCost;
					}
				} else if (h.OccupantBlocksMovement == false) {
					if (hexDistanceFromCenter [current] + h.MoveCost <= rangePerAction * numAction) {
						openSet.Enqueue (h);
						hexDistanceFromCenter [h] = hexDistanceFromCenter [current] + h.MoveCost;
					}
				}
			}
		}

		List<List<Hex>> inRange = new List<List<Hex>> ();

		for (int i = 0; i < numAction; i++) {
			inRange.Add (new List<Hex> ());

			List<Hex> possibleHexes = hexDistanceFromCenter.Keys.ToList ();

			for(int j = possibleHexes.Count - 1; j >= 0; j--) {
				if (hexDistanceFromCenter [possibleHexes[j]] < rangePerAction * (i + 1)) {
					inRange [i].Add (possibleHexes[j]);
					hexDistanceFromCenter.Remove (possibleHexes[j]);
				}
			}
		}

		return inRange;
	}


	//Ignore obstacules
	public List<Tile> TilesInRange(Tile center, int range){
		//Return tiles rnage steps from center, http://www.redblobgames.com/grids/hexagons/#range
		List<Tile> ret = new List<Tile>();
		CubeIndex o;

		for(int dx = -range; dx <= range; dx++){
			for(int dy = Mathf.Max(-range, -dx-range); dy <= Mathf.Min(range, -dx+range); dy++){
				o = new CubeIndex(dx, dy, -dx-dy) + center.index;
				if(grid.ContainsKey(o.ToString()))
					ret.Add(grid[o.ToString()]);
			}
		}
		return ret;
	}

	public List<Tile> TilesInRange(CubeIndex index, int range){
		return TilesInRange(TileAt(index), range);
	}

	public List<Tile> TilesInRange(int x, int y, int z, int range){
		return TilesInRange(TileAt(x,y,z), range);
	}

	public List<Tile> TilesInRange(int x, int z, int range){
		return TilesInRange(TileAt(x,z), range);
	}

	public static int Distance(CubeIndex a, CubeIndex b){
		return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
	}

	public static int Distance(Tile a, Tile b){
		return Distance(a.index, b.index);
	}
	#endregion

	#region Private Methods
	internal void Awake() {
		if(!inst)
			inst = this;

		hexes = new Dictionary<Tile, Hex> ();
		GenerateGrid();
	}

	internal void GetMesh() {
		hexMesh = null;
		Tile.GetHexMesh(hexRadius, hexOrientation, ref hexMesh);
	}

	internal void GenHexShape() {
		Debug.Log ("Generating hexagonal shaped grid...");

		Tile tile;
		Vector3 pos = Vector3.zero;

		int mapSize = Mathf.Max(mapWidth, mapHeight);
		
		for (int q = -mapSize; q <= mapSize; q++){
			int r1 = Mathf.Max(-mapSize, -q-mapSize);
			int r2 = Mathf.Min(mapSize, -q+mapSize);
			for(int r = r1; r <= r2; r++){
				switch(hexOrientation){
				case HexOrientation.Flat:
					pos.x = hexRadius * 3.0f/2.0f * q;
					pos.z = hexRadius * Mathf.Sqrt(3.0f) * (r + q/2.0f);
					break;
					
				case HexOrientation.Pointy:
					pos.x = hexRadius * Mathf.Sqrt(3.0f) * (q + r/2.0f);
					pos.z = hexRadius * 3.0f/2.0f * r;
					break;
				}
				
				tile = CreateHexGO(pos,("Hex[" + q + "," + r + "," + (-q-r).ToString() + "]"));
				tile.index = new CubeIndex(q,r,-q-r);
				AddToGrid(tile.index.ToString(), tile);
			}
		}
	}

	internal void GenRectShape() {
		Debug.Log ("Generating rectangular shaped grid...");

		Tile tile;
		Vector3 pos = Vector3.zero;

		switch(hexOrientation){
		case HexOrientation.Flat:
			for(int q = 0; q < mapWidth; q++){
				int qOff = q >> 1;

				//for symmetry
				int tilesInRow = mapHeight - qOff - (q&1);
				for (int r = -qOff; r < tilesInRow; r++){
					pos.x = hexRadius * 3.0f/2.0f * q;
					pos.z = hexRadius * Mathf.Sqrt(3.0f) * (r + q/2.0f);
					
					tile = CreateHexGO(pos,("Hex[" + q + "," + r + "," + (-q-r).ToString() + "]"));
					tile.index = new CubeIndex(q,r,-q-r);
					AddToGrid(tile.index.ToString(), tile);
				}
			}
			break;
			
		case HexOrientation.Pointy:
			for(int r = 0; r < mapHeight; r++){
				int rOff = r>>1;
				for (int q = -rOff; q < mapWidth - rOff; q++){
					pos.x = hexRadius * Mathf.Sqrt(3.0f) * (q + r/2.0f);
					pos.z = hexRadius * 3.0f/2.0f * r;
					
					tile = CreateHexGO(pos,("Hex[" + q + "," + r + "," + (-q-r).ToString() + "]"));
					tile.index = new CubeIndex(q,r,-q-r);
					AddToGrid(tile.index.ToString(), tile);
				}
			}
			break;
		}
	}

	internal void GenParrallShape() {
		Debug.Log ("Generating parrellelogram shaped grid...");

		Tile tile;
		Vector3 pos = Vector3.zero;
		
		for (int q = 0; q <= mapWidth; q++){
			for(int r = 0; r <= mapHeight; r++){
				switch(hexOrientation){
				case HexOrientation.Flat:
					pos.x = hexRadius * 3.0f/2.0f * q;
					pos.z = hexRadius * Mathf.Sqrt(3.0f) * (r + q/2.0f);
					break;
					
				case HexOrientation.Pointy:
					pos.x = hexRadius * Mathf.Sqrt(3.0f) * (q + r/2.0f);
					pos.z = hexRadius * 3.0f/2.0f * r;
					break;
				}

				tile = CreateHexGO(pos,("Hex[" + q + "," + r + "," + (-q-r).ToString() + "]"));
				tile.index = new CubeIndex(q,r,-q-r);
				AddToGrid(tile.index.ToString(), tile);
			}
		}
	}

	internal void GenTriShape() {
		Debug.Log ("Generating triangular shaped grid...");
		
		Tile tile;
		Vector3 pos = Vector3.zero;

		int mapSize = Mathf.Max(mapWidth, mapHeight);

		for (int q = 0; q <= mapSize; q++){
			for(int r = 0; r <= mapSize - q; r++){
				switch(hexOrientation){
				case HexOrientation.Flat:
					pos.x = hexRadius * 3.0f/2.0f * q;
					pos.z = hexRadius * Mathf.Sqrt(3.0f) * (r + q/2.0f);
					break;
					
				case HexOrientation.Pointy:
					pos.x = hexRadius * Mathf.Sqrt(3.0f) * (q + r/2.0f);
					pos.z = hexRadius * 3.0f/2.0f * r;
					break;
				}
				
				tile = CreateHexGO(pos,("Hex[" + q + "," + r + "," + (-q-r).ToString() + "]"));
				tile.index = new CubeIndex(q,r,-q-r);
				AddToGrid(tile.index.ToString(), tile);
			}
		}
	}

	internal Tile CreateHexGO(Vector3 postion, string name) {
		GameObject go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(Tile));

		if(addColliders)
			go.AddComponent<MeshCollider>();

		if(drawOutlines)
			go.AddComponent<LineRenderer>();

		go.transform.position = postion;
		go.transform.parent = this.transform;

		Tile tile = go.GetComponent<Tile>();
		MeshFilter fil = go.GetComponent<MeshFilter>();
		MeshRenderer ren = go.GetComponent<MeshRenderer>();

		fil.sharedMesh = hexMesh;

		ren.material = (hexMaterial)? hexMaterial : UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

		if(addColliders){
			MeshCollider col = go.GetComponent<MeshCollider>();
			col.sharedMesh = hexMesh;
		}

		if(drawOutlines) {
			LineRenderer lines = go.GetComponent<LineRenderer>();
			lines.useLightProbes = false;
			lines.receiveShadows = false;

			lines.SetWidth(0.1f, 0.1f);
			lines.SetColors(Color.black, Color.black);
			lines.material = lineMaterial;

			lines.positionCount = 7;

			for(int vert = 0; vert <= 6; vert++)
				lines.SetPosition(vert, Tile.Corner(tile.transform.position, hexRadius, vert, hexOrientation));
		}

		go.AddComponent<Hex> ();
		go.tag = "Hex";

		go.layer = LayerMask.NameToLayer ("Hex");

		return tile;
	}

	internal void AddToGrid(string s, Tile t){
		grid [s] = t;
		hexes[t] = t.GetComponent<Hex> ();
	}
	#endregion
}

[System.Serializable]
public enum MapShape {
	Rectangle,
	Hexagon,
	Parrallelogram,
	Triangle
}

[System.Serializable]
public enum HexOrientation {
	Pointy,
	Flat
}