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


	public TargetSelectorMode(Contestant selected, Action action, int range, bool friendlyTeam, Func<T, bool> additionalChecks = null) 
		: base(
			onMouseOver: (hex) => { },
			onMouseNotOverMap: () => { },
			onLeftClick: (hex) => { }, //to be filled
			onRightClick: (hex) => {
				UserControlManager.Instance.ModeType = ControlModeEnum.Move;
			},
			onTabPressed: () => { }, // to be filled
			onModeChanged: () => { })
	{


		onModeChanged = () => {
			ClearLineUI ();
			camCont.enabled = true;
		};

		camCont.enabled = false;

		targets = new TargetFinder<T> (selected, range, friendlyTeam, additionalChecks).Targets;

		if (targets.Count <= 0) {
			//no targets UI
		} else {

			ChangeTarget (selected);

			onLeftClick = (hex) => {
				action();
			};

			onTabPressed = () => {
				ChangeTarget (selected);
			};
		}
	}

	public void ChangeTarget (Contestant selected)
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

			LineMouseUI (selected, currentTarget.CurrentHex);
		} else {
			Debug.Log ("No valid targets");
			camAuto.MoveCameraParallelToZeroPlane (selected.CurrentHex.Position, 0.2f);
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
}
