using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateLightIntensity(float newIntensity){
		GetComponent<Light> ().intensity = newIntensity * 2.0f;
	}
}
