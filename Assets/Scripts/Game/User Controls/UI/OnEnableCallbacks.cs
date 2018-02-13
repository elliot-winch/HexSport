using System;
using UnityEngine;
using UnityEngine.UI;

public class OnEnableCallbacks : MonoBehaviour{

	Func<bool> onEnableCondition;
	Action onEnableTrue;
	Action onEnableFalse;

	void OnEnable(){
		if (onEnableCondition != null && onEnableCondition() == true) {
			if (onEnableTrue != null) {
				onEnableTrue ();
			}
		} else{
			if (onEnableFalse != null) {
				onEnableFalse ();
			}
		}
	}

	public void RegisterOnEnabledConditionCallback(Func<bool> callback){
		onEnableCondition += callback;
	}

	public void RegisterOnEnabledTrueCallback(Action callback){
		onEnableTrue += callback;
	}

	public void RegisterOnEnabledFalseCallback(Action callback){
		onEnableFalse += callback;
	}


}
