using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxTexture : MonoBehaviour {

	public Texture grid;
	public Texture black;
	public Renderer[] rend;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp (KeyCode.H) && !variables.freezeAll) {
			variables.showBindingBox = !variables.showBindingBox;

			for (int r = 0; r < rend.Length; r++) {
				rend[r].material.mainTexture = (variables.showBindingBox) ? grid : black;
			}
		}

	}

}
