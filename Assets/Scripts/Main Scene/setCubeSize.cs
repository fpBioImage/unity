using UnityEngine;
using System.Collections;

public class setCubeSize : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.localScale = variables.cubeSize.normalized * variables.cubeScale;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
