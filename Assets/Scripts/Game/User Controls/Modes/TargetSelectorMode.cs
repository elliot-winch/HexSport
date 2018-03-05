using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO separate functionality and UI

public class TargetSelectorMode<T> : ControlMode where T : IOccupant {

	protected CameraAutoMove camAuto = Camera.main.GetComponent<CameraAutoMove> ();
	protected CameraControls camCont = Camera.main.GetComponent<CameraControls> ();

	protected Dictionary<T, float> targets;
	protected T currentTarget;

	public List<T> Targets { get { return targets.Keys.ToList(); } }
	public Dictionary<T, float> TargetProbabilities { get { return targets; } }
	public T CurrentTarget { get { return currentTarget; } }

	Contestant contestant;
	ContestantAction<T> conAction;
	int range;
	bool friendlyTeam;
	Func<T, bool> additionalChecks;


	public TargetSelectorMode(ContestantAction<T> conAction, Contestant contestant, int range, float timeCost, bool friendlyTeam, Func<T, bool> additionalChecks = null) 
		: base(
			type: ControlModeEnum.DirectTarget,
			onMouseOver: (hex) => { },
			onMouseNotOverMap: () => { },
			onLeftClick: (hex) => { }, 
			onRightClick: (hex) => {
				UserControlManager.Instance.ControlModeType = ControlModeEnum.Move;
			},
			onTabPressed: () => { }, 
			onEnteringMode: () => { },
			onLeavingMode: () => { },
			autoOnly: true)
	{

		onLeavingMode = () => {
			ClearLineUI ();
			CloseProbabilityPopUp();
			camCont.enabled = true;
		};

		onEnteringMode = () => {
			//CalculateTargets();
			//targets are calculated and stored on move ending now, rather than switching to specific slector mode

			DisplayProbabilityPopUp();

			ChangeTarget();
		};
			
		onLeftClick = (hex) => {
			//temp number, but might actually be were we calculate time / or get the time from a TimeManager
			contestant.ActionsRemaining -- ;
			UserControlManager.Instance.RunAction(conAction.CompleteAction, 1f);
		};

		onTabPressed = () => {
			ChangeTarget ();
		};

		CheckValidity = () => {
			return this.contestant.ActionsRemaining > 0 && CalculateTargets() > 0;
		};

		this.AutoOnly = true;

		this.conAction = conAction;
		this.contestant = contestant;
		this.range = range;
		this.friendlyTeam = friendlyTeam;
		this.additionalChecks = additionalChecks;

		bool willShowStats = false;

		Type[] interfaces = typeof(T).GetInterfaces ();

		foreach (Type t in interfaces) {
			if (t == typeof(IStats)) {
				willShowStats = true;
			}
		}

			
		if (willShowStats){
			onEnteringMode += () => {
				StatUIManager.Instance.TargetStatParent.gameObject.SetActive (true);
				StatUIManager.Instance.ShowTargetUI((IStats)currentTarget);
			};

			onLeavingMode += () => {
				StatUIManager.Instance.TargetStatParent.gameObject.SetActive (false);
			};

			onTabPressed += () => {
				StatUIManager.Instance.ShowTargetUI((IStats)currentTarget);
			};
		}

	}

	public int CalculateTargets(){
		targets = new TargetFinder<T> (contestant, range, friendlyTeam, additionalChecks).Targets;

		return targets.Count;
	}

	void ChangeTarget ()
	{
		List<T> targetList = Targets;

		if (targets.Count > 0) {
			if (currentTarget != null) {
				int index = targetList.IndexOf (currentTarget);

				currentTarget = targetList [(index + 1) % targets.Count];
				SwitchHighlightedProbability ((index + 1) % targets.Count);

			} else {
				currentTarget = targetList [0];
				SwitchHighlightedProbability (0);

			}
			camAuto.MoveCameraParallelToZeroPlane (currentTarget.CurrentHex.Position, 0.2f);

			LineMouseUI (contestant, currentTarget.CurrentHex);


		} else {
			Debug.Log ("No valid targets");
			camAuto.MoveCameraParallelToZeroPlane (contestant.CurrentHex.Position, 0.2f);
		}
	}

	#region UI
	Dictionary<Hex, GameObject> hexLine;

	internal void LineMouseUI(Contestant selected, Hex h){ //this should really have the relevant hexes passed to it rather than recalc them
		ClearLineUI ();

		List<Hex> hexesInLine;
		GridManager.Instance.DrawLineOnGrid (selected.CurrentHex, h.Occupant.CurrentHex, out hexesInLine);

		hexLine = new Dictionary<Hex, GameObject> ();

		foreach (Hex j in hexesInLine) {
			GameObject go = MonoBehaviour.Instantiate (UIHexBuilder.Instance.flatHexPrefab, selected.transform);
			go.transform.position = j.Position + new Vector3 (0, 0.001f, 0);
			go.GetComponent<MeshRenderer>().material.color = new Color(2/3f, 0, 2/3f);

			go.name = "TargetSelectorMode UI Hex";
			hexLine [j] = go;
		}
	}

	internal void ClearLineUI(){
		if (hexLine != null) {
			foreach (Hex j in hexLine.Keys) {
				MonoBehaviour.Destroy (hexLine [j]);
			}

			hexLine = null;
		}
	}

	//Probabilities UI
	GameObject probabilitiesBackground;
	GameObject[] targetIcons;
	int currentHighlightIcon;
	internal void DisplayProbabilityPopUp(){

		probabilitiesBackground = MonoBehaviour.Instantiate (ActionUIManager.Instance.probabilitiesPopUp, ScreenManager.Instance.mainCanvas.transform);
		probabilitiesBackground.transform.SetAsFirstSibling ();

		probabilitiesBackground.GetComponentInChildren<Text> ().text = conAction.Name;

		targetIcons = new GameObject[Targets.Count];
		Rect probabilityRect = probabilitiesBackground.GetComponent<RectTransform> ().rect;

		for(int i = 0; i < Targets.Count; i++) {
			GameObject targetIcon = MonoBehaviour.Instantiate(ActionUIManager.Instance.probabilityIcon, probabilitiesBackground.transform);
			Rect textRect = targetIcon.transform.Find ("Text").GetComponent<RectTransform> ().rect;
			float iconHeight = probabilityRect.height + textRect.height + targetIcon.GetComponent<RectTransform>().rect.height;

			targetIcon.GetComponent<Image> ().sprite = this.conAction.UI.ButtonSprite;

			targetIcon.GetComponentInChildren<Text> ().text = String.Format ("{0}%", (Math.Ceiling(targets [Targets[i]] * 100f)).ToString());

			targetIcon.transform.localPosition = new Vector2 ((textRect.width * 2f) * (i - ((Targets.Count - 1) / 2f)), iconHeight);
		
			targetIcons [i] = targetIcon;
		}
			
		currentHighlightIcon = -1;
	}

	internal void CloseProbabilityPopUp(){
		MonoBehaviour.Destroy (probabilitiesBackground);
	}

	internal void SwitchHighlightedProbability(int index){
		if (currentHighlightIcon >= 0) {
			targetIcons [currentHighlightIcon].GetComponent<RectTransform> ().sizeDelta = ActionUIManager.Instance.probabilityIcon.GetComponent<RectTransform> ().sizeDelta;

		}

		currentHighlightIcon = index;

		targetIcons [index].GetComponent<RectTransform> ().sizeDelta = targetIcons [index].GetComponent<RectTransform> ().sizeDelta * 1.5f;
	}


	#endregion //UI
	public override string ToString ()
	{
		return string.Format ("[TargetSelectorMode: Current Target In CurrentTarget={0}]", (CurrentTarget != null) ? CurrentTarget.CurrentHex.ToString() : "null");
	}
}
