using System;
using UnityEngine;
using UnityEngine.UI;

public class OnEnableButton :  Button{

	Func<bool> onEnable;

	void OnEnable(){
		if (onEnable != null && onEnable() == false) {
			GetComponentInChildren<Image> ().color = new Color (2 / 3f, 0, 0);

			base.interactable = false;
		} else {
			GetComponentInChildren<Image> ().color = new Color (1f, 1f, 1f);

			base.interactable = true;
		}
	}

	public void RegisterOnEnabledCallback(Func<bool> callback){
		onEnable += callback;
	}
}
