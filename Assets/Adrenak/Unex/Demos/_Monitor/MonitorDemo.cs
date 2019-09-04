﻿using UnityEngine;
using Adrenak.Unex;

public class MonitorDemo : MonoBehaviour {
    public GameObject toMonitor;
	
	void Start () {
        toMonitor.GetMonitor().HandleCollisionEnter(collision => {
            Debug.Log("Object Collided with " + collision.collider.name);
        });
	}
}
