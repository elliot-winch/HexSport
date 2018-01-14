using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * To implement!
 * 
 */ 

/*
public class Map {

	Dictionary<CubeIndex, HexType> mapInfo;

	public Map(int mapWidth, int mapHeight){
		//load from file

		mapInfo = new Dictionary<Vector3, HexType> ();

		for (int q = 0; q < mapWidth; q++) {
			int qOff = q >> 1;

			//for symmetry
			int tilesInRow = mapHeight - qOff - (q & 1);
			for (int r = -qOff; r < tilesInRow; r++) {
				mapInfo[new CubeIndex(q, r, -q-r)] = HexType.Reg;
			}
				
		}
			
	}

	public void GetHexType(Vector3 cubeCoords){
		return mapInfo [cubeCoords];
	}
}
*/