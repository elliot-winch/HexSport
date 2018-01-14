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
	public Vector3 BallOffset {
		get {
			return new Vector3 (0f, 6f, 0f);
		}
	}

	public System.Action<Ball> OnCatch {
		get {
			return (ball) => {
				GameManager.Instance.Score(team);

				Destroy(ball.gameObject);

				GameManager.Instance.SpawnBall();
			};
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
