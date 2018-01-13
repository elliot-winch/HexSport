using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFinder<T> where T : IOccupant {

	public Dictionary<T, float> targets;

	public Dictionary<T, float> Targets {
		get {
			return targets;
		}
	}

	public TargetFinder(Contestant con, int range, bool friendlyTeam, Func<T, bool> additionalChecks = null){
		this.targets = GetValidTargets (con, range, friendlyTeam, additionalChecks);
	}

	Dictionary<T, float> GetValidTargets(Contestant con, int range, bool friendlyTeam, Func<T, bool> additionalChecks = null){

		Dictionary<T, float> targets = new Dictionary<T, float> ();

		List<Tile> tilesInRange = GridManager.Instance.Grid.TilesInRange (con.CurrentHex.GetComponent<Tile> ().index, range);

		foreach (Tile t in tilesInRange) {
			Hex h = t.GetComponent<Hex> ();

			if (h.Occupant != null && h.Occupant is T && (h.Occupant.Team == GameManager.Instance.CurrentTeam) == friendlyTeam) {

				T posTarg = (T)h.Occupant;

				int cubeDist = Grid.Distance (con.CurrentHex.GetComponent<Tile> (), posTarg.CurrentHex.GetComponent<Tile> ());

				if(GridManager.Instance.DrawLineOnGrid(con.CurrentHex, posTarg.CurrentHex, cubeDist)){

					if (additionalChecks == null || (additionalChecks != null && additionalChecks (posTarg))) {

						targets [posTarg] = 1f - cubeDist * (0.5f / con.Data.Dexerity);
					}
				}
			}
		}

		return targets;
	}
}
