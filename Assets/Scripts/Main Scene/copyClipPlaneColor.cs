using UnityEngine;
using System.Collections;

/*
 * The clip plane has a front and a back. However quads
 * in unity are only rendered on one side. This function
 * simply copies the color fading effect from the main
 * clipping plane to the helping back plane. 
 * Note that the back plane is a child of the front. 
 * 
*/

public class copyClipPlaneColor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Renderer> ().material.color = Color.clear;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate() {
		GetComponent<Renderer>().material.color = transform.parent.GetComponent<Renderer> ().material.color;
	}
}
