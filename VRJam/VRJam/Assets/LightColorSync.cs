using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightColorSync : MonoBehaviour {

	// Use this for initialization
	void Start () {

        GetComponent<Renderer>().material.SetColor("_EmissionColor", GetComponentInChildren<Light>().color);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
