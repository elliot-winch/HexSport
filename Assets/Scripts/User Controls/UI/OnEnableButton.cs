using System;
using UnityEngine.UI;

public class OnEnableButton :  Button{

	Func<bool> onEnable;

	void OnEnable(){
		if (onEnable != null) {
			this.interactable = onEnable ();
		} else {
			this.interactable = true;
		}
	}

	public void RegisterOnEnabledCallback(Func<bool> callback){
		onEnable += callback;
	}
}
