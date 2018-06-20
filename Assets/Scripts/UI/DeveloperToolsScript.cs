using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperToolsScript : MonoBehaviour {

	public GameObject renderCube;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ToggleFPSCount(bool toggle){
		transform.GetComponentInParent<FPSDisplay> ().enabled = toggle;
	}

	public void ToggleRainbowCube(bool toggle){
		renderCube.GetComponent<Renderer> ().material.SetInt ("_RainbowCube", toggle ? 1 : 0);
	}

	public void CloseDevPanel(){
		gameObject.SetActive (false);
	}
}
