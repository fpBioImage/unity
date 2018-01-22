using UnityEngine;
using System.Collections;

// Offline FPBioimage is still under development.

/*
 * This script sets the global variables required
 * in the renderer.
 * It should probably be merged neatly with the 
 * 'configLoader' script, with use of the global 
 * variables offlineMode, set by the 'offlineHome' 
 * scene. 
*/

public class offlineConfig : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		variables.offlineMode = true;

		setLoadFromBookmark ("false");
		setNumberOfImages (336);
		setImagePrefix ("mouse_z");
		setNumberingFormat ("0000");
		setPathToImages ("/Users/carcu/Pictures/mouse/");

		setVoxelSizeX (1.0f);
		setVoxelSizeY (1.0f);
		setVoxelSizeZ (1.0f); 

		variables.sliceWidth = 255;
		#if UNITY_EDITOR
			variables.sliceWidth *= 2;
			// This is a weird thing I don't understand! confuse. 
		#endif

		variables.sliceHeight = 255;
		variables.imageAlpha = true;
		variables.loadAtlasDirectly = true;

		variables.defaultIntensity = 1.0f;
		variables.defaultOpacity = 0.4f;
		variables.defaultThreshold = 0.2f;
	}

	// Update is called once per frame
	void Update () {
		// Nothing to do. 
	}
		

	public void setLoadFromBookmark(string boolean){
		variables.loadBookmarkFromURL = (boolean=="true") ? true : false;
	}

	public void setUniqueName(string javascriptString)
	{
		javascriptString = System.Text.RegularExpressions.Regex.Replace(javascriptString, @"\s+", "");
		variables.uniqueName = javascriptString;
	}

	public void setNumberOfImages(int javascriptInt)
	{	variables.numberOfImages = javascriptInt;	}

	public void setImagePrefix(string javascriptString)
	{	variables.imagePrefix = javascriptString;	}

	public void setNumberingFormat(string javascriptString)
	{	variables.numberingFormat = javascriptString;	}

	public void setPathToImages(string javascriptString)
	{	
		if (javascriptString.Substring (javascriptString.Length - 1) != "/") {
			javascriptString = javascriptString + "/";
		}
		//variables.pathToImages = "../" + javascriptString;
		variables.pathToImages = javascriptString;
	}

	public void setVoxelSizeX(float javascriptFloat){
		variables.voxelSize[0] = javascriptFloat;
	}

	public void setVoxelSizeY(float javascriptFloat){
		variables.voxelSize[1] = javascriptFloat;
	}

	public void setVoxelSizeZ(float javascriptFloat){
		variables.voxelSize[2] = javascriptFloat;
	}

}
