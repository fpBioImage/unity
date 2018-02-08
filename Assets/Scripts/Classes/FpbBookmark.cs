using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FpbBookmark{
	// Camera properties
	public Vector3 cameraPosition;
	public Quaternion cameraRotation;

	// Cube properties
	public Quaternion cubeRotation;

	// Cutting plane properties
	public string cuttingParent;
	public Vector3 cuttingPlanePosition;
	public Quaternion cuttingPlaneRotation;

	// Rendering properties
	public float opacity;
	public float intensity;
	public float threshold;
	public int projectionType;

	public string sectionMode; // Note that bools are stored as strings
	public string bindingBox; 

	public string annotation;

	private bool nullBookmark = false;
	private int browserBookmarkNumber; 

	public bool getSectionMode(){
		return (sectionMode == "true") ? true : false;
	}

	public bool getBindingBox(){
		return (bindingBox == "true") ? true : false;
	}

	public void setSectionMode(bool inputBool){
		sectionMode = inputBool ? "true" : "false";
	}

	public void setBindingBox(bool inputBool){
		bindingBox = inputBool ? "true" : "false";
	}

	public FpbBookmark(){
	}

	public FpbBookmark(int nullInt){
		nullBookmark = true;
		browserBookmarkNumber = nullInt;
	}

	public bool isNullBookmark(){
		return nullBookmark;
	}

	public int getBrowserBookmarkNumber(){
		return browserBookmarkNumber;
	}

	/*
	// Check annotation (1)
	annotation = annotation.Replace("\\", "\\\\");
	annotation = annotation.Replace("'","\\'");
	annotation = annotation.Replace (" ", "_");*/
}
