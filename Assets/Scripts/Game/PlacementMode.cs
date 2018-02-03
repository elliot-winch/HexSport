using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementMode {

	List<Team> teams;
	Dictionary<Team, List<Hex>> placementHexes;
	int currentTeamIndex;
	int currentCDIndex;

	GameObject hexOutliner;

	public PlacementMode(List<Team> teams){

		this.teams = teams;
		this.placementHexes = new Dictionary<Team, List<Hex>> ();

		foreach (Team t in teams) {
			this.placementHexes [t] = new List<Hex> ();
		}

		List<Tile> tiles = GridManager.Instance.Grid.Tiles.Values.ToList ();
		//For now, just splits board in half. But different splitting modes will be available for different maps / board shapes / gamemodes / number of teams

		int middleOfBoard = (GridManager.Instance.Grid.mapHeight / 2); 

		foreach (Tile tile in tiles) {
			
			float middleZ = -middleOfBoard - (tile.index.x/2);

			if (tile.index.z > middleZ) {
				placementHexes [teams [0]].Add (tile.GetComponent<Hex> ());
			} else if (tile.index.z < middleZ) {
				placementHexes [teams [1]].Add (tile.GetComponent<Hex> ());
			}
		}

		this.currentTeamIndex = 0;
		this.currentCDIndex = 0;

		this.hexOutliner = UserControlManager.Instance.SpawnUIGameObject (null);
	}


	public void PlacementUI(Hex h){

		if (h != null) {
			this.hexOutliner.SetActive (true);

			this.hexOutliner.transform.position = h.Position +  new Vector3 (0f, 0.001f, 0f); 

			if(CanPlace(h)){
				this.hexOutliner.GetComponent<MeshRenderer> ().material.color = Color.green;
				//spawn ghostly figure here
			} else {
				this.hexOutliner.GetComponent<MeshRenderer> ().material.color = Color.red;
			}
		} else {
			DisablePlacementUI ();
		}
	}

	public void DisablePlacementUI(){
		this.hexOutliner.SetActive (false);
	}

	public void EraseUI(){
		MonoBehaviour.Destroy (this.hexOutliner);
	}

	public void PlaceContestant(Hex h){

		if (CanPlace (h)) {

			GameManager.Instance.SpawnContestant (teams [currentTeamIndex].Contestants [currentCDIndex++], h);
			Debug.Log ("Spawning");

			if (currentCDIndex >= teams [currentTeamIndex].Contestants.Count) {
				currentTeamIndex++;

				if (currentTeamIndex >= teams.Count) {
					UserControlManager.Instance.ControlModeType = ControlModeEnum.Move;
					GameManager.Instance.CheckStartOfTurn ();
				} else {
					currentCDIndex = 0;
				}
			}
		}
	}

	bool CanPlace(Hex h){

		//Hex not in spawning range
		if (placementHexes [teams [currentTeamIndex]].Contains (h) == false) {
			return false;
		}

		//Hex occupied (ball doesn't count)
		if (h.OccupantBlocksMovement) {
			return false;
		}

		return true;
		
	}
}
