using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserControlManager : MonoBehaviour {

	static UserControlManager instance;

	public static UserControlManager Instance {
		get {
			return instance;
		}
	}

	public Button throwButton;
	public KeyCode selectNextKey = KeyCode.Tab;

	GameObject flatHexPrefab;

	//References
	CameraControls camCont;
	CameraAutoMove camAuto;

	//Mode and input callbacks
	Hex prevMouseHex;
	ControlMode[] controlModes;
	ControlMode currentControlMode;
	ControlModeEnum modeType;

	//Movement vairables
	Action<Contestant> onSelected;
	Action<Contestant> onDeselected;
	Contestant selected;
	GameObject hexOutliner;
	Vector3 outlinerOffset = new Vector3(0f, 0.01f, 0.0f);

	public Contestant Selected {
		get {
			return selected;
		}
	}

	public ControlMode ControlMode {
		get {
			return currentControlMode;
		}
		set {
			if (currentControlMode != null && currentControlMode.OnLeavingMode != null) {
				currentControlMode.OnLeavingMode ();
			}

			currentControlMode = value;
			modeType = currentControlMode.ModeType;

			currentControlMode.OnEnteringMode ();

		}
	}

	public ControlModeEnum ControlModeType {
		get {
			return modeType;
		}

		set {
			if (currentControlMode != null && currentControlMode.OnLeavingMode != null) {
				currentControlMode.OnLeavingMode ();
			}

			currentControlMode = controlModes [(int)value];
			modeType = value;

			currentControlMode.OnEnteringMode ();

		}
	}

	void Start(){
		if (instance != null) {
			Debug.LogError ("There should not be two mouse managers");
		}

		instance = this;

		camCont = Camera.main.GetComponent<CameraControls> ();
		camAuto = Camera.main.GetComponent<CameraAutoMove> ();

	
		flatHexPrefab = new GameObject ();
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

		flatHexPrefab.AddComponent<MeshFilter> ().sharedMesh = m;
		flatHexPrefab.transform.Rotate (new Vector3 (270, 0, 0));
		flatHexPrefab.AddComponent<MeshRenderer> ();

		flatHexPrefab.SetActive (false);

		hexOutliner = Instantiate (flatHexPrefab);
		hexOutliner.name = "Cursor";

		hexOutliner.GetComponent<MeshRenderer> ().material.color = Color.green;

		//Control Modes
		controlModes = new ControlMode[6];

		controlModes [(int)ControlModeEnum.Observe] = new ControlMode (
			type: ControlModeEnum.Observe,

			onMouseOver: (hex) => {
			},

			onMouseNotOverMap: () => {
			},

			onLeftClick: (hex) => {
			},

			onRightClick : (hex) => {
			},

			onTabPressed: () => {
			},

			onEnteringMode: () => {
			},

			onLeavingMode: () => {
			},

			autoOnly: false
		);

		controlModes [(int)ControlModeEnum.Move] = new ControlMode (
			type: ControlModeEnum.Move,

			onMouseOver: (hex) => {
				MovementMouseUI (hex);
			},

			onMouseNotOverMap: () => {
				DisableMoveUI();
			},

			onLeftClick: (hex) => {
				SelectHex (hex);
			},

			onRightClick : (hex) => {
				MoveSelectedToHex (hex);
			},

			onTabPressed: () => {
				if (selected != null) {
					SelectNext (selected);
				}
			},

			onEnteringMode: () => {
				SelectFirst();
			},

			onLeavingMode: () => {
				DisableMoveUI();
				if(selected != null){
					selected.HideMovementHexes();
				}
			},

			autoOnly: false
		);

		ControlModeType = ControlModeEnum.Observe;

		Debug.Log (modeType);
		Debug.Log (currentControlMode.ToString());
	}

	void Update () {
		
		RaycastHit hitInfo;

		Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (r, out hitInfo, 1<<LayerMask.NameToLayer("Hex"))) {
			Hex mouseHex = null;

			if (hitInfo.collider.tag == "Hex") {
				mouseHex = hitInfo.collider.GetComponent<Hex> ();
			} else {
				currentControlMode.OnMouseNotOverMap ();
				return;
			}

			if (mouseHex != null && prevMouseHex != mouseHex) {
				currentControlMode.OnMouseOver (mouseHex);
				prevMouseHex = mouseHex;
			}

		} else {
			currentControlMode.OnMouseNotOverMap ();
		}

		//Left Click
		if ((Input.GetMouseButtonDown (0) || Input.GetKeyDown(KeyCode.Return)) && hitInfo.collider != null) {
			if (hitInfo.collider.tag == "Hex") {
				currentControlMode.OnLeftClick (hitInfo.collider.GetComponent<Hex> ());
			}
		}

		//Right Click
		if ((Input.GetMouseButtonDown (1) || Input.GetKeyDown(KeyCode.Escape)) && hitInfo.collider != null) {
			if (hitInfo.collider.tag == "Hex") {
				currentControlMode.OnRightClick(hitInfo.collider.GetComponent<Hex>());
			}
		}

		//Tab Handler
		if(Input.GetKeyDown(selectNextKey)){
			currentControlMode.OnTabPressed ();
		}

		if(Input.GetKeyDown(KeyCode.Space)){
			GameManager.Instance.NextTurn();
		}
	}

	#region internal methods - move mode
	void SelectHex(Hex h){
		//Temp - non team members cannot be selected
		//Create two levels of selection - view and control

		if (h.Occupant != null && h.Occupant is Contestant) {
			Contestant c = (Contestant)h.Occupant;

			if (c.Data.Team == TeamManager.Instance.CurrentTeam) {
				StartCoroutine (SelectContestant (c));
			}
		}
	}
		
	public IEnumerator SelectContestant(Contestant c){

		if (selected != null) {
			selected.HideMovementHexes ();
			DisableMoveUI ();

			if (onDeselected != null) {
				onDeselected (selected);
			}
		}

		selected = c;
	
		if (onSelected != null) {
			onSelected (c);
		}

		camAuto.MoveCameraParallelToZeroPlane (selected.CurrentHex.Position, 0.2f);

		while (selected.Moving) {
			yield return null;
		}

		selected.ShowMovementHexes ();
	}


	public void SelectNext(Contestant c){
		List<ContestantData> cons = TeamManager.Instance.CurrentTeam.Contestants;

		if (c != null) {
			int index = cons.IndexOf (c.Data);
			StartCoroutine (SelectContestant (cons [(index + 1) % cons.Count].Contestant));
		} 
	}

	public void SelectFirst(){
		List<ContestantData> cons = TeamManager.Instance.CurrentTeam.Contestants;
		StartCoroutine (SelectContestant (cons [0].Contestant));
	}

	void MoveSelectedToHex(Hex h){
		if (selected != null) {
			DisableMoveUI ();
			StartCoroutine (selected.Move (h));
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

				hexPositions.Add (selected.transform.position + outlinerOffset - selected.PositionOffset);
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

	void DisableMoveUI(){
		hexOutliner.SetActive (false);
		if (selected != null) {
			selected.GetComponent<LineRenderer> ().enabled = false;
		}
	}
	#endregion

	#region internal methods - throw mode - plz move this somewhere else
	public void ThrowToTarget(){
		TargetSelectorMode<ICatcher> tsm = ((TargetSelectorMode<ICatcher>)currentControlMode);

		ICatcher currentTarget = tsm.CurrentTarget;
		List<ICatcher> targets = tsm.Targets;

		if(currentTarget != null){
			ICatcher catcher = (ICatcher)currentTarget;

			if (targets.Contains (catcher)) {
				selected.Ball.RegisterOnFinishedLaunch (() => {
					ControlModeType = ControlModeEnum.Move;
				});

				selected.Ball.ThrowToCatcher (selected, catcher, tsm.TargetProbabilities[catcher]);

			}
		}
	}
	#endregion

	#region Swipe - temp probs
	public void SwipeBall(){
		Contestant target = ((TargetSelectorMode<Contestant>)currentControlMode).CurrentTarget;

		if (target.Ball == null) {
			Debug.LogError ("Trying to swipe ball that isn't held");
		}

		selected.Ball = target.Ball;

		selected.Ball.Release (target);
		selected.Ball.Receive (selected);

	}
	#endregion
		
	public void RegisterOnSelectedCallback(Action<Contestant> callback){
		onSelected += callback;
	}
		
	public void RegisterOnDeselectedCallback(Action<Contestant> callback){
		onDeselected += callback;
	}

	public GameObject SpawnUIGameObject(Hex h){
		GameObject spawned = Instantiate (flatHexPrefab, h.Position + new Vector3 (0, 0.001f, 0), Quaternion.identity, transform);

		spawned.transform.Rotate (new Vector3 (270, 0, 0));
		spawned.SetActive (true);

		spawned.transform.parent = selected.transform;
		spawned.name = "Move UI Hex";

		return spawned;
	}
}

public enum ControlModeEnum {
	Observe,
	Move,
	DirectTarget
}
