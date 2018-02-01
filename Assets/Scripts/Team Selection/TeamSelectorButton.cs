using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class  TeamSelectorButton : MonoBehaviour {

	public bool active = false;
	public int originalChildPosition;

	public void MoveToFront(){

		if (active) {
			Debug.Log (originalChildPosition);

			this.transform.SetAsLastSibling ();
		}

	}

	public void MoveBack(){
		if (active) {
			this.transform.SetSiblingIndex (originalChildPosition);
		}
	}
}

public interface IStats {

	Dictionary<string, string> Stats { get; }
}
