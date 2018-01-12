using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour {

	static MouseManager instance;

	public static MouseManager Instance {
		get {
			return instance;
		}
	}

	public Button throwButton;
	public KeyCode selectNextKey = KeyCode.Tab;

	ControlMode mode = ControlMode.Observe;
	Action<Hex> onMouseOver;
	Action onMouseNotOnMap;
	Action<Hex> onLeftClick;
	Action<Hex> onRightClick;
	Action onTabPressed;
	Action onModeChanged;

	Action<Contestant> onSelected;
		
	Contestant selected;
	GameObject hexOutliner;
	Hex prevMouseHex;
	Vector3 outlinerOffset = new Vector3(0f, 0.01f, 0.0f);

	List<Contestant> targets;

	public ControlMode Mode {
		get {
			return mode;
		}
		set {
			if (mode == value) {
				return;
			}

			if (onModeChanged != null) {
				onModeChanged ();
			}

			mode = value;

			switch (mode) {

			case ControlMode.Move:
				onMouseOver = (mouseHex) => {
					//Mouse UI: Path Line and Cursor
					MovementMouseUI (mouseHex);
				};

				onMouseNotOnMap = () => {
					hexOutliner.SetActive (false);
					if (selected != null) {
						selected.GetComponent<LineRenderer> ().enabled = false;
					}
				};

				onLeftClick = (hex) => {
					SelectHex (hex);
				};

				onRightClick = (hex) => {
					MoveSelectedToHex (hex);
				};

				onTabPressed = () => {
					if (selected != null) {
						SelectNext (selected);
					} else {
						StartCoroutine (SelectContestant (GameManager.Instance.CurrentTeam.Contestants [0].Contestant));
					}
				};

				onModeChanged = () => {
					if(selected != null){
						selected.HideMovementHexes();
						selected.GetComponent<LineRenderer>().enabled = false;
					}
				};
				break;
			case ControlMode.Throw:

				targets = GameManager.Instance.GetValidTargets (selected, 4 /*selected.range*/, true);

				onMouseOver = (mouseHex) => {
					//Mouse UI: Path Line and Cursor
					hexOutliner.SetActive (true);
					ThrowMouseUI (mouseHex);
				};

				onMouseNotOnMap = () => {
					hexOutliner.SetActive (false);
					ClearThrowLineUI ();
				};

				onLeftClick = (hex) => {
					ThrowTo (hex);
				};

				onRightClick = (hex) => {
					Mode = ControlMode.Move;
				};

				onTabPressed = () => {
					//will cycle through targets
				};

				onModeChanged = () => {
					ClearThrowLineUI();
				}; 
				break;
			}
		}
	}

	public Action<Contestant> OnSelected {
		get {
			return onSelected;
		}
	}

	void Start(){
		if (instance != null) {
			Debug.LogError ("There should not be two mouse managers");
		}

		instance = this;

		Mode = ControlMode.Move;
	
		hexOutliner = new GameObject ();
		Vector3[] verts = new Vector3[6];

		for(int k = 0; k < verts.Length; k++){
			float angleRad = Mathf.PI / 3 * k;
			verts [k] = new Vector3 (Mathf.Cos (angleRad), Mathf.Sin (angleRad));
		}

		int[] tris = new int[12];

		tris [0] = 0;
		tris [1] = 1;
		tris [2] = 5;

		tris [3] = 1;
		tris [4] = 4;
		tris [5] = 5;

		tris [6] = 2;
		tris [7] = 4;
		tris [8] = 1;

		tris [9] = 3;
		tris [10] = 4;
		tris [11] = 2;

		Vector3[] normals = new Vector3[6];

		for (int k = 0; k < normals.Length; k++) {
			normals [k] = Vector3.up;
		}

		Mesh m = new Mesh ();
		m.vertices = verts;
		m.triangles = tris;
		m.normals = normals;

		hexOutliner.AddComponent<MeshFilter> ().sharedMesh = m;
		hexOutliner.AddComponent<MeshRenderer> ().material.color = Color.green;

		hexOutliner.transform.Rotate (new Vector3 (270, 0, 0));
	}

	void Update () {
		
		RaycastHit hitInfo;

		Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (r, out hitInfo, 1<<LayerMask.NameToLayer("Hex"))) {
			Hex mouseHex = null;

			if (hitInfo.collider.tag == "Hex") {
				mouseHex = hitInfo.collider.GetComponent<Hex> ();
			} else {
				onMouseNotOnMap ();
				return;
			}

			if (mouseHex != null && prevMouseHex != mouseHex) {
				onMouseOver (mouseHex);
				prevMouseHex = mouseHex;
			}

		} else {
			onMouseNotOnMap ();
		}

		//Left Click
		if (Input.GetMouseButtonDown (0) && hitInfo.collider != null) {
			if (hitInfo.collider.tag == "Hex") {
				onLeftClick (hitInfo.collider.GetComponent<Hex> ());
			}
		}

		//Right Click
		if (Input.GetMouseButtonDown (1) && hitInfo.collider != null) {
			if (hitInfo.collider.tag == "Hex") {
				onRightClick(hitInfo.collider.GetComponent<Hex>());
			}
		}

		//Select Next
		if(Input.GetKeyDown(selectNextKey)){
			onTabPressed ();
		}
	}

	#region private methods - move mode
	void SelectHex(Hex h){
		//Temp - non team members cannot be selected
		//Create two levels of selection - view and control

		if (h.Occupant != null && h.Occupant is Contestant) {
			Contestant c = (Contestant)h.Occupant;

			if (c.Data.Team == GameManager.Instance.CurrentTeam) {
				StartCoroutine (SelectContestant (c));
			}
		}
	}
		
	IEnumerator SelectContestant(Contestant c){

		if (selected != null) {
			selected.HideMovementHexes ();
		}

		selected = c;

		onSelected (selected);

		while (selected.Moving) {
			yield return null;
		}

		selected.ShowMovementHexes ();
	}

	void MoveSelectedToHex(Hex h){
		if (selected != null) {
			StartCoroutine (selected.Move (h));

			if (selected.Moving) {
				selected.HideMovementHexes ();
			}

			selected.RegisterOnMoveCompleteCallback ((Contestant c) => {
				if (c == selected) {
					c.ShowMovementHexes ();
				}
			});
		}
	}

	void MovementMouseUI(Hex mouseHex){
		//button needs to be disabled if the player is moving or has the ball
		//need to be able to free throw - different UI

		hexOutliner.SetActive (true);
		hexOutliner.transform.position = mouseHex.Position + outlinerOffset;

		if (selected != null) {
			if (selected.Moving == false && selected.MoveHexesInRange.Contains (mouseHex)) {
				Path p = new Path (GridManager.Instance.Grid, selected.CurrentHex, mouseHex);

				List<Vector3> hexPositions = new List<Vector3> ();

				LineRenderer lr = selected.GetComponent<LineRenderer> ();
				lr.enabled = true;

				hexPositions.Add (selected.transform.position + outlinerOffset);
				while (p.IsNextHex ()) {
					hexPositions.Add (p.GetNextHex ().Position + outlinerOffset);
				}

				lr.positionCount = hexPositions.Count;

				lr.SetPositions (hexPositions.ToArray ());
			} else {
				selected.GetComponent<LineRenderer> ().enabled = false;
			}
		}
	}
	#endregion

	#region private methods - throw mode
	void ThrowTo(Hex h){
		if(h.Occupant != null && h.Occupant is Contestant){
			Contestant target = (Contestant)h.Occupant;

			if (targets.Contains (target)) {
				selected.Ball.Throw (selected, (Contestant)h.Occupant);
			}
		}
	}

	List<Hex> prevHexLine;
	void ThrowMouseUI(Hex h){
		if (h.Occupant != null 
			&& h.Occupant is Contestant
			&& targets.Contains((Contestant)h.Occupant)) {

			hexOutliner.GetComponent<MeshRenderer> ().material.color = Color.green;

			List<Hex> hexesInLine;
			GameManager.Instance.DrawLineOnGrid (selected.CurrentHex, h.Occupant.CurrentHex, out hexesInLine);

			ClearThrowLineUI ();

			foreach (Hex j in hexesInLine) {
				j.GetComponent<MeshRenderer> ().material.color = new Color (1 / 3f, 1 / 3f, 0);
			}

			prevHexLine = hexesInLine;
		} else {
			hexOutliner.GetComponent<MeshRenderer> ().material.color = Color.red;

			ClearThrowLineUI ();
		}
		hexOutliner.transform.position = h.Position + outlinerOffset;
	}

	void ClearThrowLineUI(){
		if (prevHexLine != null) {
			foreach (Hex j in prevHexLine) {
				j.GetComponent<MeshRenderer> ().material.color = new Color (1f, 1f, 1f);
			}

			prevHexLine = null;
		}
	}
	#endregion


	public void SwitchModes(int modeNum){
		Mode = (ControlMode)modeNum;
	}

	public void SelectNext(Contestant c){
		List<ContestantData> cons = GameManager.Instance.CurrentTeam.Contestants;

		int index = cons.IndexOf (c.Data);
		StartCoroutine(SelectContestant(cons[(index + 1) % cons.Count].Contestant));
	}

	public void RegisterOnSelectedCallback(Action<Contestant> callback){
		onSelected += callback;
	}
}

public enum ControlMode {
	Observe,
	Move,
	Throw
}
