using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour, IOccupant {

	public float throwSpeed = 1f;
	public float throwHeight = 4f;
	public Vector3 ballOffset = new Vector3(0.5f, 0.5f, 0.5f);
	Hex currentHex;

	Action onFinishedLaunch;

	public Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			//the ball is abou to move

			//if the current hex contains only the ball, now nothing will occupy this space.
			if (currentHex != null && currentHex.Occupant is Ball && ((Ball)currentHex.Occupant) == this) {
				currentHex.Occupant = null;
			}

			//move the ball
			currentHex = value;

			//if the ball has moved to a hex
			if (currentHex != null) {
				//if that hex has an occupant...
				if (currentHex.Occupant != null) {
					//...which can receive the ball (eg a player)
					if (currentHex.Occupant is ICatcher) {
						this.Receive ((ICatcher)currentHex.Occupant);
					}
					//...which cant receive the ball (eg a wall)
				} else {
					//the hex doesnt have an occupant, so the ball occupies the hex
					currentHex.Occupant = this;

					transform.parent = null;
					transform.position = currentHex.Position;
				}
			}

			//the case where current Hex is set to null is handled by whatever picks up the ball
		}
	}

	public Team Team {
		get {
			return null;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Receive(ICatcher catcher){
		catcher.OnCatch (this);
	}

	public void Release(Contestant c){
		c.Ball = null;
		transform.parent = null;
	}

	public void ThrowToCatcher(Contestant a, ICatcher b, float chanceToThrow){
		if (a.Ball == null) {
			Debug.LogError ("Trying to throw a ball you don't have");
		}

		Release (a);

		StartCoroutine (Throwroutine (a, b, chanceToThrow));
	}

	IEnumerator Throwroutine(Contestant a, ICatcher b, float chanceToThrow){
		float randomVal = UnityEngine.Random.value;

		if(randomVal <= chanceToThrow){

			yield return StartCoroutine (Launch (a.CurrentHex.Position + a.BallOffset, b.CurrentHex.Position + b.BallOffset));
			Receive (b);
		} else {
			List<Hex> hexesAroundMissed = GridManager.Instance.Grid.HexesInRangeAccountingObstacles (b.CurrentHex, 1 + ((randomVal - chanceToThrow) * 10f));

			Debug.Log (hexesAroundMissed.Count + " collected from range: " + (1 + ((randomVal - chanceToThrow) * 10f)));

			Hex hex = hexesAroundMissed [UnityEngine.Random.Range (1, hexesAroundMissed.Count)];

			yield return StartCoroutine (Launch (a.transform.position + a.BallOffset, hex.Position));

			CurrentHex = hex;

		}

		onFinishedLaunch ();
	}

	IEnumerator Launch(Vector3 startPos, Vector3 endPos){

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

	}

	public void RegisterOnFinishedLaunch(Action callback){
		onFinishedLaunch += callback;
	}
}
