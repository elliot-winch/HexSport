using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour {

	public float cameraHeight = 10f;
	public KeyCode forwards;
	public KeyCode left;
	public KeyCode right;
	public KeyCode backwards;
	public float moveSpeed = 5f;
	public KeyCode rotateLeft;
	public KeyCode rotateRight;
	public float rotateSpeed = 0.2f;
	public float scrollSpeed = 5f;

	Plane zeroPlane = new Plane(Vector3.up, Vector3.zero);

	CameraAutoMove cAuto;
	CameraResponder cResp;

	public float CameraHeight {
		get {
			return cameraHeight;
		}
		set {
			cameraHeight = value;
			transform.position = new Vector3(transform.position.x, cameraHeight, transform.position.z);
		}
	}

	void Start () {
		cAuto = GetComponent<CameraAutoMove> ();
		cResp = GetComponent<CameraResponder> ();

		CameraHeight = cameraHeight;

		/*
		UserControlManager.Instance.RegisterOnSelectedCallback( (c) => {
			cAuto.MoveCameraParallelToZeroPlane(c.transform.position, 0.2f);

		});
		*/
	}
	
	void Update () {

		//Moving
		Vector3 toMove = new Vector3 ();

		if (Input.GetKey (forwards)) {
			toMove += new Vector3(transform.forward.x, 0, transform.forward.z) * moveSpeed;
		}

		if (Input.GetKey (backwards)) {
			toMove += new Vector3(-transform.forward.x, 0, -transform.forward.z) * moveSpeed;
		}

		if (Input.GetKey (left)) {
			toMove += new Vector3(-transform.right.x, 0, -transform.right.z) * moveSpeed;
		}

		if (Input.GetKey (right)) {
			toMove += new Vector3(transform.right.x, 0, transform.right.z) * moveSpeed;
		}

		transform.position += toMove;

		//Rotation
		if(Input.GetKey(rotateLeft) || Input.GetKey(rotateRight)){
			Ray r = Camera.main.ViewportPointToRay (new Vector2(0.5f, 0.5f));

			float distance = 0;

			zeroPlane.Raycast (r, out distance);

			Vector3 point = r.GetPoint (distance);

			if (Input.GetKey (rotateLeft)) {

				transform.RotateAround (point, Vector3.up, rotateSpeed);
				transform.LookAt (point);
			}

			if (Input.GetKey (rotateRight)) {
				transform.RotateAround (point, Vector3.up, -rotateSpeed);
			}
		}

		//Zoom - broken
		transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
	}

}
