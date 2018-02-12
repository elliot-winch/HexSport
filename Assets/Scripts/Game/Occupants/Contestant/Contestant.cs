using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contestant : MonoBehaviour, ICatcher, IStats {

	public int actionsPerTurn = 2;
	public float movesPerAction = 4f; //this will be dependent on the con's pace
	public float moveSpeed = 3f;

	//Movement variables
	bool moving = false;
	int actionsRemaining;
	float movesInActionRemaining;
	List<Dictionary<Hex, GameObject>> moveHexesInRange;
	IOccupant destOccupant;

	//For movement UI
	ObjectPool uiHexPool;

	ContestantData data;
	List<IContestantAction> possibleActions;

	Vector3 positionOffset = new Vector3 (0, 0, 0);

	Ball ball;

	#region ICatcher implementation
	public Transform BallHolderObject {
		get {
			return transform.GetChild (0).GetChild (0);
		}
	}

	public Action<Ball> OnCatch {
		get {
			return (ball) => {
				ball.CurrentHex = null;

				this.ball = ball;
				//should be ballPosition object
				ball.transform.parent = BallHolderObject;
				ball.transform.localPosition = Vector3.zero;
			};
		}
	}
	#endregion

	#region IStats implementation

	public Dictionary<string, string> Stats {
		get {
			return data.Stats;
		}
	}
	#endregion

	public Vector3 Position {
		get {
			return transform.position;
		}
		set {
			transform.position = value + positionOffset;
		}
	}

	public Vector3 PositionOffset {
		get {
			return positionOffset;
		}
	}

	public int ActionsRemaining {
		get {
			return actionsRemaining;
		}
		set {
			actionsRemaining = value;

			//potentially have a callback here instead of explicitly stating
			TeamUIManager.Instance.UpdateActionsRemainingUI(this);

			GameManager.Instance.CheckStartOfTurn ();
		}
	}

	Hex currentHex;

	public Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			if (value == null) {
				Debug.LogError ("Cannot set T.CurrentHex to null");
			}

			if (currentHex != null) {
				currentHex.Occupant = null;
			}

			currentHex = value;

			currentHex.Occupant = this;
		}
	}

	public Team Team {
		get {
			return data.Team;
		}
	}

	Action<Contestant> onTurnBegin;

	public Action<Contestant> OnTurnBegin {
		get {
			return onTurnBegin;
		}
	}

	public bool Moving {
		get {
			return moving;
		}
	}

	Action<Contestant> onMoveBegan;
	Action<Contestant> onMoveComplete;

	public Action<Contestant> OnMoveBegan {
		get {
			return onMoveBegan;
		}
	}

	public Action<Contestant> OnMoveComplete {
		get {
			return onMoveComplete;
		}
	}


	public List<Hex> MoveHexesInRange {
		get {
			List<Hex> hexes = new List<Hex> ();

			foreach (Dictionary<Hex, GameObject> h in moveHexesInRange) {
				hexes.AddRange (h.Keys.ToList());
			}

			return hexes;
		}
	}

	public ContestantData Data {
		get {
			return data;
		}
		set {
			if (data == null) {
				data = value;
			}
		}
	}

	public List<IContestantAction> PossibleActions {
		get {
			return possibleActions;
		}
	}

	public Ball Ball {
		get {
			return ball;
		}
		set {
			ball = value;
		}
	}
		
	//
	void Awake(){
		possibleActions = new List<IContestantAction> ();

		LineRenderer lr = GetComponent<LineRenderer> ();
		lr.enabled = false;

		moveHexesInRange = new List<Dictionary<Hex, GameObject>> ();

		RegisterOnTurnStartCallback ( (c) => {
			c.ActionsRemaining = actionsPerTurn;
		} );

		RegisterOnMoveCompleteCallback ( (c) => {
			CheckHex(destOccupant);
		} );

		uiHexPool = new ObjectPool (() => {
			GameObject spawned;

			spawned = Instantiate (UIHexBuilder.FlatHexPrefab, Vector3.zero, Quaternion.identity, transform);

			spawned.transform.Rotate (new Vector3 (270, 0, 0));
			spawned.SetActive (true);

			spawned.name = "Movement UI Hex";

			return spawned;
		}, maxObjects: 200);


		//UIManager uses this in Start.
		onMoveComplete (this);
	}

	public void Move(Hex destHex){
		if (moving == false) {
			for (int i = 0; i < moveHexesInRange.Count; i++) {
				if (moveHexesInRange [i].Keys.Contains (destHex)) {
					ActionsRemaining -= (i + 1);
					movesInActionRemaining = movesPerAction * (i+1);

					StartCoroutine (MoveCoroutine(destHex));
				}
			}
		}
	}

	IEnumerator MoveCoroutine(Hex destHex){

		moving = true;

		if (onMoveBegan != null) {
			onMoveBegan (this);
		}

		HideMovementHexes ();

		AStarPath p = new AStarPath (GridManager.Instance.Grid, currentHex, destHex, true);

		Hex next;

		Hex current = CurrentHex;
		destOccupant = destHex.Occupant;
		CurrentHex = destHex;

		while (p.IsNextHex ()) {
			next = p.GetNextHex ();

			CheckHex (next.Occupant);

			if (movesInActionRemaining >= next.MoveCost) {
				yield return StartCoroutine (MoveToHex (current, next));
			} 

			current = next;
		}

		onMoveComplete (this);

		if (UserControlManager.Instance.Selected == this) {
			ShowMovementHexes ();
		}

		moving = false;
	}

	public void RegisterOnTurnStartCallback(Action<Contestant> callback){
		onTurnBegin += callback;
	}

	public void RegisterOnMoveBeganCallback(Action<Contestant> callback){
		onMoveBegan += callback;
	}

	public void RegisterOnMoveCompleteCallback(Action<Contestant> callback){
		onMoveComplete += callback;
	}

	public void ShowMovementHexes(){
		List<List<Hex>> hexes = GridManager.Instance.Grid.HexesInRangeSegmentedByActions (CurrentHex, ActionsRemaining, movesPerAction);

		for(int i = 0; i < hexes.Count; i++) {
			moveHexesInRange.Add(new Dictionary<Hex, GameObject> ());

			foreach (Hex h in hexes[i]) {
				GameObject uiHex = uiHexPool.GetObject ();
				uiHex.transform.position = h.Position + new Vector3 (0, 0.001f, 0);
				uiHex.GetComponent<MeshRenderer> ().material.color = new Color (1f / (i + 1), 0, 1f/ (i + 1));

				moveHexesInRange[i] [h] = uiHex;
			}
		}
	}

	public void HideMovementHexes(){
		foreach (Dictionary<Hex, GameObject> hexes in moveHexesInRange) {
			foreach (Hex h in hexes.Keys) {
				uiHexPool.Dismiss (hexes [h]);
			}
		}

		moveHexesInRange = new List<Dictionary<Hex, GameObject>> ();
	}

	public 

	#region private methods
	IEnumerator MoveToHex(Hex current, Hex neighbour){

		float distance = Vector3.Distance(current.Position, neighbour.Position);
		float movePercentage = 0f;

		while (movePercentage < 1f) {
			movePercentage += (moveSpeed * Time.deltaTime) / (distance * neighbour.MoveCost);

			Position = Vector3.Lerp(current.Position, neighbour.Position, movePercentage); 

			yield return null;
		}

		movesInActionRemaining -= neighbour.MoveCost;
	
	}

	void CheckHex(IOccupant o){
		if (o != null && o is Ball) {
			((Ball)o).Receive (this);
		}
	}
	#endregion

}
