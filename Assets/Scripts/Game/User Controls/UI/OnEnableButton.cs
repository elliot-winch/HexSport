using System;
using UnityEngine;
using UnityEngine.UI;

public class OnEnableButton :  Button{

	Func<bool> onEnable;

	void OnEnable(){
		if (onEnable != null) {
			Debug.Log ("on enabled is not null");
		
			gameObject.SetActive( onEnable ());
		} else {
			Debug.Log ("on enabled is null");
			gameObject.SetActive( true);
		}
	}

	public void RegisterOnEnabledCallback(Func<bool> callback){
		onEnable += callback;
	}
}
