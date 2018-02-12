using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * This class is temporary. It makes a hexagon, which will eventually be a 3D model rather than
 * programmatically generated.
 * 
 */ 

public static class UIHexBuilder {

	static GameObject flatHexPrefab = null;

	//Allows for lazy instantiation
	public static GameObject FlatHexPrefab {
		get {
			if (flatHexPrefab == null){
				flatHexPrefab = new GameObject ();
				Vector3[] verts = new Vector3[6];

				for(int k = 0; k < verts.Length; k++){
					float angleRad = Mathf.PI / 3 * k;
					verts [k] = new Vector3 (Mathf.Cos (angleRad), Mathf.Sin (angleRad));
				}

				int[] tris = new int[12];

				tris [0] = 0;
				tris [1] = 1;
				tris [2] = 5;

				tris [3] = 1;
				tris [4] = 4;
				tris [5] = 5;

				tris [6] = 2;
				tris [7] = 4;
				tris [8] = 1;

				tris [9] = 3;
				tris [10] = 4;
				tris [11] = 2;

				Vector3[] normals = new Vector3[6];

				for (int k = 0; k < normals.Length; k++) {
					normals [k] = Vector3.up;
				}

				Mesh m = new Mesh ();
				m.vertices = verts;
				m.triangles = tris;
				m.normals = normals;

				flatHexPrefab.AddComponent<MeshFilter> ().sharedMesh = m;
				flatHexPrefab.transform.Rotate (new Vector3 (270, 0, 0));
				flatHexPrefab.AddComponent<MeshRenderer> ();

			}
			return flatHexPrefab;
		}
	}
}
