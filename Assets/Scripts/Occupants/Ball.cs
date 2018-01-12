using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour, IOccupant {

	public float throwSpeed = 1f;
	public float throwHeight = 4f;
	public Vector3 ballOffset = new Vector3(0.5f, 0.5f, 0.5f);
	Hex currentHex;

	public Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			if (currentHex != null) {
				currentHex.Occupant = null;
			}

			currentHex = value;

			if (currentHex != null) {
				if (currentHex.Occupant != null) {
					if (currentHex.Occupant is Contestant) {
						this.PickUp ((Contestant)currentHex.Occupant);
					}
				} else {
					currentHex.Occupant = this;

					transform.parent = null;
					transform.position = currentHex.Position;
				}
			}
		}
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void PickUp(Contestant con){
		CurrentHex = null;

		transform.parent = con.transform;
		transform.localPosition = ballOffset;
	}

	public void Drop(){
		
	}

	public void Throw(Contestant a, Contestant b){
		StartCoroutine (Launch (a, b));
	}

	IEnumerator Launch(Contestant a, Contestant b){
		if (a.Ball == null) {
			Debug.LogError ("Trying to throw a ball you don't have");
		}

		a.Ball = null;

		Vector3 startPos = a.transform.position + ballOffset;
		Vector3 endPos = b.transform.position + ballOffset;

		float distance = Vector3.Distance(startPos, endPos);
		float movePercentage = 0f;
		Vector3 framePosition;

		while (movePercentage < 1f) {
			movePercentage += (throwSpeed * Time.deltaTime) / distance;

			framePosition = Vector3.Lerp(startPos, endPos, movePercentage);
			framePosition += new Vector3(0f, throwHeight * (-((movePercentage - 0.5f) * (movePercentage - 0.5f)) + 0.25f) , 0f);

			transform.position = framePosition;

			yield return null;
		}

		PickUp (b);
	}
}
