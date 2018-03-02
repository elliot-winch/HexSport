using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagableObject : MonoBehaviour, IOccupant {

	public int startingHealth;
	int health;



	public virtual void Hit(int amount){

		health = Mathf.Max (0, health - amount);

		if (health <= 0f) {
			OnDestroy ();
		} else {
			OnDamaged(amount);
		}
	}

	protected virtual void OnDamaged(int amount){
		Debug.Log ("I'm hit for: " + amount);
	}

	protected virtual void OnDestroy(){
		Destroy (gameObject);
	}

	#region IOccupant implementation

	protected Hex currentHex;

	public virtual Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			if (value == null) {
				Debug.LogError ("Cannot set T.CurrentHex to null");
			}

			if (currentHex != null) {
				currentHex.Occupant = null;
			}

			currentHex = value;

			currentHex.Occupant = this;
		}
	}

	public virtual Team Team {
		get {
			return null;
		}
	}

	public virtual Vector3 HexOffset {
		get {
			return Vector3.zero;
		}
	}

	#endregion
}
