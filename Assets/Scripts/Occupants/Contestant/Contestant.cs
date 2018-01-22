using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contestant : MonoBehaviour, ICatcher {

	public float movesPerTurn = 4f;
	public float moveSpeed = 3f;

	//Movement variables
	bool moving = false;
	float movesRemaining;
	Dictionary<Hex, GameObject> moveHexesInRange;
	IOccupant destOccupant;

	ContestantData data;
	List<IContestantAction> possibleActions;

	Vector3 positionOffset = new Vector3 (0, 1f, 0);

	Ball ball;

	#region ICatcher implementation
	public Vector3 BallOffset {
		get {
			return new Vector3 (0.5f, 0.5f, 0.5f);
		}
	}

	public Action<Ball> OnCatch {
		get {
			return (ball) => {
				ball.CurrentHex = null;

				this.ball = ball;
				ball.transform.parent = transform;
				ball.transform.localPosition = BallOffset;
			};
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

	public float MovesRemaining {
		get {
			return movesRemaining;
		}
		private set {
			movesRemaining = value;

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

	Action<Contestant> onMoveComplete;
	Action<Contestant> defaultOnMoveComplete;

	public Action<Contestant> OnMoveComplete {
		get {
			return onMoveComplete;
		}
	}

	public List<Hex> MoveHexesInRange {
		get {
			return moveHexesInRange.Keys.ToList();
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

		RegisterOnTurnStartCallback ( (c) => {
			c.MovesRemaining = c.movesPerTurn;
		} );

		RegisterOnMoveCompleteCallback ( (c) => {
			CheckHex(destOccupant);
		} );

		defaultOnMoveComplete = onMoveComplete;
	}

	public IEnumerator Move(Hex destHex){
		if (moving == false && moveHexesInRange.ContainsKey (destHex)) {

			moving = true;

			HideMovementHexes ();

			Path p = new Path (GridManager.Instance.Grid, currentHex, destHex);

			Hex next;

			Hex current = CurrentHex;
			destOccupant = destHex.Occupant;
			CurrentHex = destHex;

			while (p.IsNextHex ()) {
				next = p.GetNextHex ();

				CheckHex (next.Occupant);

				if (MovesRemaining >= next.MoveCost) {
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
	}

	public void RegisterOnTurnStartCallback(Action<Contestant> callback){
		onTurnBegin += callback;
	}

	public void RegisterOnMoveCompleteCallback(Action<Contestant> callback){
		onMoveComplete += callback;
	}

	public void ResetOnMoveCompleteCallback(){
		onMoveComplete = defaultOnMoveComplete;
	}

	public void ShowMovementHexes(){
		moveHexesInRange = new Dictionary<Hex, GameObject> ();

		List<Hex> hexes = GridManager.Instance.Grid.HexesInRangeAccountingObstacles (CurrentHex, MovesRemaining);

		foreach (Hex h in hexes) {
			moveHexesInRange [h] = UserControlManager.Instance.SpawnUIGameObject (h);
			moveHexesInRange [h].GetComponent<MeshRenderer> ().material.color = new Color (1 / 3f, 0, 1 / 3f);
		}
	}

	public void HideMovementHexes(){
		foreach (Hex h in moveHexesInRange.Keys) {
			Destroy (moveHexesInRange [h]);
		}
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

		MovesRemaining -= neighbour.MoveCost;
	
	}

	void CheckHex(IOccupant o){
		if (o != null && o is Ball) {
			((Ball)o).Receive (this);
		}
	}
	#endregion

}
