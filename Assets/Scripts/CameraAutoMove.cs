using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAutoMove : MonoBehaviour {

	CameraControls cc;

	Plane zeroPlane = new Plane(Vector3.up, Vector3.zero);

	void Start(){
		cc = GetComponent<CameraControls> ();
	}

	public void MoveCameraParallelToZeroPlane(Vector3 lookPoint, float speed){
		StartCoroutine (LerpCamera (lookPoint, speed));
	}

	IEnumerator LerpCamera(Vector3 lookPoint, float speed){
		cc.enabled = false;

		Ray r = Camera.main.ViewportPointToRay (new Vector2(0.5f, 0.5f));
		float distance = 0;
		zeroPlane.Raycast (r, out distance);
		Vector3 currentLookPoint = r.GetPoint (distance);

		Vector3 endPoint = transform.position + new Vector3 (lookPoint.x - currentLookPoint.x, 0, lookPoint.z - currentLookPoint.z);

		while (Vector3.Distance(transform.position, endPoint) > 0.1f) {
			transform.position = Vector3.Lerp (transform.position, endPoint, speed);
			yield return null;
		}

		if (UserControlManager.Instance.ModeType == ControlModeEnum.Move) {
			cc.enabled = true;
		}
	}
}
