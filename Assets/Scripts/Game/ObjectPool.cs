using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {

	Queue<GameObject> objects;
	Func<GameObject> creation;
	int maxObjects;

	public ObjectPool(Func<GameObject> creation, int maxObjects = 100){
		this.creation = creation;
		this.maxObjects = maxObjects;
		this.objects = new Queue<GameObject> ();
	}

	public GameObject GetObject(){
		if (objects.Count > 0) {
			GameObject go = objects.Dequeue ();
			go.SetActive (true);
			return go;

		} else {
			return creation ();
		}
	}

	public void Dismiss(GameObject go){
		if (maxObjects > objects.Count) {
			objects.Enqueue (go);
			go.SetActive (false);
		}
		//else we don't care
	}
}