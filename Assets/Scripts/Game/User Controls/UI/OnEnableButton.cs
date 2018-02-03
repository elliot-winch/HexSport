using System;
using UnityEngine;
using UnityEngine.UI;

public class OnEnableButton :  Button{

	Func<bool> onEnable;

	void OnEnable(){
		if (onEnable != null) {
			base.interactable = onEnable ();
		} else {
			base.interactable = true;
		}
	}

	public void RegisterOnEnabledCallback(Func<bool> callback){
		onEnable += callback;
	}
}
