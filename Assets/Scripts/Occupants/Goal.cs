using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour, ICatcher {

	Hex currentHex;

	public Hex CurrentHex {
		get {
			return currentHex;
		}
		set {
			currentHex = value;
			transform.position = currentHex.Position;
		}
	}

	#region ICatcher implementation
	public Transform BallHolderObject {
		get {
			return transform;
		}
	}

	public System.Action<Ball> OnCatch {
		get {
			return (ball) => {
				TeamManager.Instance.Score(team);

				Destroy(ball.gameObject);

				GameManager.Instance.SpawnBall();
			};
		}
	}

	public Ball Ball {
		get {
			return null;
		}
	}

	Team team;
	public Team Team {
		get {
			return team;
		}
		set {
			if(team == null){
				team = value;

				MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer> ();

				foreach (MeshRenderer mr in mrs) {
					mr.material.color = team.Color;
				}
			}
		}
	}
	#endregion
}
