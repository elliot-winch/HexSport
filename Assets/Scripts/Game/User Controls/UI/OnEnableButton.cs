using System;
using UnityEngine;
using UnityEngine.UI;

public class OnEnableButton :  Button{

	Func<bool> onEnable;

	void OnEnable(){
		if (onEnable != null) {
			gameObject.SetActive( onEnable ());
		} else {
			gameObject.SetActive( true);
		}
	}

	public void RegisterOnEnabledCallback(Func<bool> callback){
		onEnable += callback;
	}
}
