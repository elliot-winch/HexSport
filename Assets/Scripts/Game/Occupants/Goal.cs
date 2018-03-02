using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour, ICatcher, IStats {

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

	public Vector3 HexOffset {
		get {
			return Vector3.zero;
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

				SpawnManager.Instance.SpawnBall();
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

	#region IStats implementation

	public Dictionary<string, string> Stats {
		get {
			return new Dictionary<string, string> () {
				{ "Title",  "Goal" },
				{ "Value", "1" }
			};
		}
	}

	#endregion
}
