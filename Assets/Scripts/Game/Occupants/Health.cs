using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health {

	int startingHealth;
	int health;

	public int CurrentHealth {
		get {
			return health;
		}
		set {
			health = value;
		}
	}

	public Health(int startingHealth){
		this.startingHealth = startingHealth;
	}

}
