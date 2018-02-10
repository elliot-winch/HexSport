using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Do not call camera.transform.position! Move with this script to move UI
 * 
 */ 

public class CameraResponder : MonoBehaviour {

	Action<Vector3> onMoveCamera;

	public void RegisterOnCameraMove(Action<Vector3> callback){
		onMoveCamera += callback;
	}

	public Vector3 Position {
		get {
			return transform.position;
		}
		set {
			Vector3 moveDist = value - transform.position; 

			transform.position = value;

			onMoveCamera (moveDist);
		}
	}
}
