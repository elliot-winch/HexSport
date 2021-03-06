﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UserControlManager : MonoBehaviour {

	static UserControlManager instance;

	public static UserControlManager Instance {
		get {
			return instance;
		}
	}

	public KeyCode selectNextKey = KeyCode.Tab;

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
			if (currentControlMode != null){
				if (currentControlMode.OnLeavingMode != null) {
					currentControlMode.OnLeavingMode ();
				}
			}

			currentControlMode = value;
			modeType = currentControlMode.ModeType;

			camCont.enabled = (currentControlMode.AutoOnly == false);

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

			camCont.enabled = (currentControlMode.AutoOnly == false);

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

		hexOutliner = Instantiate (UIHexBuilder.Instance.flatHexPrefab);
		hexOutliner.name = "Cursor";
		hexOutliner.SetActive (false);

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

		//Placement
		PlacementMode pm = new PlacementMode(TeamManager.Instance.TeamsInMatch);

		controlModes [(int)ControlModeEnum.Placement] = new ControlMode (
			type: ControlModeEnum.Placement,

			onMouseOver: (hex) => {
				//if over valid hex, green & show ghost of model, else red
				pm.PlacementUI(hex);
			},

			onMouseNotOverMap: () => {
				pm.DisablePlacementUI();
			},

			onLeftClick: (hex) => {
				pm.PlaceContestant(hex); //or pick up previously placed contestant
			},

			onRightClick : (hex) => {
				//perhaps remove contestant?
			},

			onTabPressed: () => {
				//cycle through contestants
			},

			onEnteringMode: () => {
				//select first contestant - which is done by constructor
			},

			onLeavingMode: () => {
				
				pm.EraseUI();
			},

			autoOnly: false
		);
	}

	void Update () {

		RaycastHit hitInfo;

		Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (EventSystem.current.IsPointerOverGameObject ()) {
			Debug.Log ("over UI");
		}

		if (Physics.Raycast (r, out hitInfo, 1<<LayerMask.NameToLayer("Hex")) && EventSystem.current.IsPointerOverGameObject() == false) {
			Hex mouseHex = null;

			if (hitInfo.collider.tag == "Hex") {
				mouseHex = hitInfo.collider.GetComponent<Hex> ();
			} else {
				currentControlMode.OnMouseNotOverMap ();
				Debug.Log ("on mouse NOT over");

				return;
			}

			if (mouseHex != null && prevMouseHex != mouseHex) {
				currentControlMode.OnMouseOver (mouseHex);
				Debug.Log ("on mouse over");
				prevMouseHex = mouseHex;
			}

		} else {
			currentControlMode.OnMouseNotOverMap ();
			Debug.Log ("on mouse NOT over");
		}

		//Left Click
		if (((Input.GetMouseButtonDown (0) && EventSystem.current.IsPointerOverGameObject() == false) || Input.GetKeyDown(KeyCode.Return)) && hitInfo.collider != null) {
			if (hitInfo.collider.tag == "Hex") {
				currentControlMode.OnLeftClick (hitInfo.collider.GetComponent<Hex> ());
			}
		}

		//Right Click
		if (((Input.GetMouseButtonDown (1) && EventSystem.current.IsPointerOverGameObject() == false) /*|| Input.GetKeyDown(KeyCode.Escape)*/) && hitInfo.collider != null) {
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

	//Run an action
	public void RunAction(Action<float> a, float time){
		StartCoroutine (Action(a, time));
	}

	IEnumerator Action(Action<float> a, float time){
		a (time);

		//disable buttons

		//here is where you might move the camera, start an animation etc.

		yield return new WaitForSeconds (time);

		ControlModeType = ControlModeEnum.Move;
		SelectNext (this.Selected);
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

		camAuto.MoveCameraParallelToZeroPlane (selected.CurrentHex.Position, 0.2f);

		while (selected.Moving) {
			yield return null;
		}

		selected.ShowMovementHexes ();

		if (onSelected != null) {
			onSelected (c);
		}
	}


	public void SelectNext(Contestant c){
		List<ContestantData> cons = TeamManager.Instance.CurrentTeam.Contestants;

		if (c != null) {
			int index = cons.IndexOf (c.Data) + 1;

			ContestantData posNext = cons [index % cons.Count];

			while (c != posNext.Contestant) {
				posNext = cons [index % cons.Count];

				if (posNext.Contestant.ActionsRemaining > 0) {
					Debug.Log ("different contestnat");
					StartCoroutine (SelectContestant (posNext.Contestant));
					return;
				} else {
					index++;
				}
			} 

			//if we've reached here, all teammates have run out of actions

			/*
			if (c.ActionsRemaining <= 0) {
				GameManager.Instance.NextTurn ();
			}*/ // we dont do this bc it is instant, and dont wait for any movement to happen
		} 
	}

	public void SelectFirst(){
		List<ContestantData> cons = TeamManager.Instance.CurrentTeam.Contestants;
		StartCoroutine (SelectContestant (cons [0].Contestant));
	}

	void MoveSelectedToHex(Hex h){
		if (selected != null) {
			DisableMoveUI ();
			selected.Move (h);
		}
	}

	void MovementMouseUI(Hex mouseHex){
		//button needs to be disabled if the player is moving or has the ball
		//need to be able to free throw - different UI

		hexOutliner.SetActive (true);
		hexOutliner.transform.position = mouseHex.Position + outlinerOffset;

		if (selected != null) {

			if (selected.Moving == false && selected.MoveHexesInRange.Contains (mouseHex)) {
				AStarPath p = new AStarPath (GridManager.Instance.Grid, selected.CurrentHex, mouseHex, true);

				List<Vector3> hexPositions = new List<Vector3> ();

				LineRenderer lr = selected.GetComponent<LineRenderer> ();
				lr.enabled = true;

				hexPositions.Add (selected.transform.position + outlinerOffset - selected.HexOffset);
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
	public void ThrowToTarget(float time){
		TargetSelectorMode<ICatcher> tsm = ((TargetSelectorMode<ICatcher>)currentControlMode);

		ICatcher currentTarget = tsm.CurrentTarget;
		List<ICatcher> targets = tsm.Targets;

		if(currentTarget != null){
			ICatcher catcher = (ICatcher)currentTarget;

			if (targets.Contains (catcher)) {
				selected.Ball.RegisterOnFinishedLaunch (() => {
					ControlModeType = ControlModeEnum.Move;
				});

				selected.Ball.ThrowToCatcher (selected, catcher, tsm.TargetProbabilities[catcher], time);

			}
		}
	}
	#endregion

	#region Swipe - temp probs
	public void SwipeBall(float time){
		//time is currently unused by will be used in the future 
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
}

public enum ControlModeEnum {
	Observe,
	Move,
	DirectTarget,
	Placement
}
