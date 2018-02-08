using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IContestantAction { 

	string Name { get; }
	ContestantActionsEnum ActionType {get;}
	ControlMode ControlMode {get;}
} 

public class ContestantAction<T> : IContestantAction where T : IOccupant{


	string name;
	ContestantActionsEnum actionType;

	Action<float> action;
	float time;
	//will be a superclass but for now is just targeted view 
	TargetSelectorMode<T> controlMode;

	public string Name {
		get {
			return name;
		}
	}

	public ContestantActionsEnum ActionType {
		get {
			return actionType;
		}
	}

	public Action<float> CompleteAction {
		get {
			return this.action;
		}
	}

	public ControlMode ControlMode {
		get {
			return controlMode;
		}
	}

	public ContestantAction(string name, ContestantActionsEnum actionType, Action<float> action, float time, Contestant con, int range, float timeCost, bool friendlyTeam, Func<T, bool> additionalChecks){
		this.name = name;
		this.actionType = actionType;
		this.action = action;
		this.time = time;

		this.controlMode = new TargetSelectorMode<T> (this, con, range, timeCost, friendlyTeam, additionalChecks);
	}
}


public enum ContestantActionsEnum {
	Shoot,
	Throw,
	Swipe
}

public static class ContestantActionsFactory {
	
	public static ContestantAction<T> CreateAction<T>(string name, ContestantActionsEnum actionType, float time, Contestant con, int range, float timeCost, bool friendlyTeam, Func<T, bool> additionalChecks = null) where T : IOccupant{

		Action<float> action = (t) => { };

		switch (actionType) {
			
		case ContestantActionsEnum.Throw:
			action = UserControlManager.Instance.ThrowToTarget;

			break;
		
			//i dont want the usercontrolmanager to have to know individual method
			//also, the usercontrolmanager gets the current control mode form storage,
			//but the controlmode is the one calling the action, so why not just
			//pass the mode forward?

		case ContestantActionsEnum.Swipe: 
			action = UserControlManager.Instance.SwipeBall;
			break;
		case ContestantActionsEnum.Shoot: 
			action = (t) => {
				Debug.Log ("Phew");
			};
			break;
		}
			
		return new ContestantAction<T> (name, actionType, action, time, con, range, timeCost, friendlyTeam, additionalChecks);
	}

			
}