using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSelectorMode<T> : ControlMode where T : IOccupant {

	protected CameraAutoMove camAuto = Camera.main.GetComponent<CameraAutoMove> ();
	protected CameraControls camCont = Camera.main.GetComponent<CameraControls> ();

	protected Dictionary<T, float> targets;
	protected T currentTarget;

	public List<T> Targets { get { return targets.Keys.ToList(); } }
	public Dictionary<T, float> TargetProbabilities { get { return targets; } }
	public T CurrentTarget { get { return currentTarget; } }

	Contestant contestant;
	Action action;
	int range;
	bool friendlyTeam;
	Func<T, bool> additionalChecks;


	public TargetSelectorMode(Contestant contestant, Action action, int range, bool friendlyTeam, Func<T, bool> additionalChecks = null) 
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
			camCont.enabled = true;
		};

		onEnteringMode = () => {
			//CalculateTargets();
			//targets are calculated and stored on move ending now, rather than switching to specific slector mode
			ChangeTarget();
		};
			
		onLeftClick = (hex) => {
			action();

			//when the action is done it should switch back to move
		};

		onTabPressed = () => {
			ChangeTarget ();
		};

		CheckValidity = () => {
			return CalculateTargets () > 0;
		};

		this.AutoOnly = true;

		this.contestant = contestant;
		this.action = action;
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
				ContestantStatUIManager.Instance.TargetStatParent.gameObject.SetActive (true);
				ContestantStatUIManager.Instance.ShowTargetUI((IStats)currentTarget);
			};

			onLeavingMode += () => {
				ContestantStatUIManager.Instance.TargetStatParent.gameObject.SetActive (false);
			};

			onTabPressed += () => {
				ContestantStatUIManager.Instance.ShowTargetUI((IStats)currentTarget);
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

			} else {
				currentTarget = targetList [0];
			}
			camAuto.MoveCameraParallelToZeroPlane (currentTarget.CurrentHex.Position, 0.2f);

			LineMouseUI (contestant, currentTarget.CurrentHex);

		} else {
			Debug.Log ("No valid targets");
			camAuto.MoveCameraParallelToZeroPlane (contestant.CurrentHex.Position, 0.2f);
		}
	}


	//UI
	Dictionary<Hex, GameObject> hexLine;

	internal void LineMouseUI(Contestant selected, Hex h){
		ClearLineUI ();

		List<Hex> hexesInLine;
		GridManager.Instance.DrawLineOnGrid (selected.CurrentHex, h.Occupant.CurrentHex, out hexesInLine);

		hexLine = new Dictionary<Hex, GameObject> ();

		foreach (Hex j in hexesInLine) {
			hexLine[j] = UserControlManager.Instance.SpawnUIGameObject (h);
			hexLine[j].GetComponent<MeshRenderer>().material.color = new Color(2/3f, 0, 2/3f);
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

	public override string ToString ()
	{
		return string.Format ("[TargetSelectorMode: Current Target In CurrentTarget={0}]", (CurrentTarget != null) ? CurrentTarget.CurrentHex.ToString() : "null");
	}
}
