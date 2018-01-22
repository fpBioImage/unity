using UnityEngine;
using System.Collections;

/*
 * This script runs in the first (home) scene. It evaluates 
 * javascript from the webpage to load in various variables 
 * to tell set the global variables for details of the 
 * image stack, to set the global variables for rendering 
 * options, and to determine if the URl contains a bookmark. 
*/

public class configLoader : MonoBehaviour {

	void Start () {

		// These are required inputs, telling FPBioimage where to find the images:
		string evalInputs = "if(fpb.uniqueName==undefined){fpb.uniqueName='defaultName'};SendMessage('Main Camera', 'setUniqueName', fpb.uniqueName);" +
			"if(fpb.numberOfImages==undefined){fpb.numberOfImages=0};SendMessage('Main Camera', 'setNumberOfImages', fpb.numberOfImages);" +
			"if(fpb.imagePrefix==undefined){fpb.imagePrefix='not-found'};SendMessage('Main Camera', 'setImagePrefix', fpb.imagePrefix);" +
			"if(fpb.numberingFormat==undefined){fpb.numberingFormat='0000'};SendMessage('Main Camera', 'setNumberingFormat', fpb.numberingFormat);" +
			"if(fpb.pathToImages==undefined){fpb.pathToImages='not-found'};SendMessage('Main Camera', 'setPathToImages', fpb.pathToImages);" +
			"if(fpb.voxelSize.x==undefined){fpb.voxelSize.x=1};SendMessage('Main Camera', 'setVoxelSizeX', fpb.voxelSize.x);" +
			"if(fpb.voxelSize.y==undefined){fpb.voxelSize.y=1};SendMessage('Main Camera', 'setVoxelSizeY', fpb.voxelSize.y);" +
			"if(fpb.voxelSize.z==undefined){fpb.voxelSize.z=1};SendMessage('Main Camera', 'setVoxelSizeZ', fpb.voxelSize.z);" + 
			"if(fpb.atlasMode==undefined){fpb.atlasMode='false'};SendMessage('Main Camera', 'setAtlasMode', fpb.atlasMode);" + 
			"if(fpb.sliceWidth==undefined){fpb.sliceWidth=100};SendMessage('Main Camera', 'setSliceWidth', fpb.sliceWidth);" +
			"if(fpb.sliceHeight==undefined){fpb.sliceHeight=100};SendMessage('Main Camera', 'setSliceHeight', fpb.sliceHeight);" + 
			"if(fpb.imageAlpha==undefined){fpb.imageAlpha='false'};SendMessage('Main Camera', 'setImageAlpha', fpb.imageAlpha);";

		Application.ExternalEval (evalInputs);

		// This works out if we need to load a bookmark from the URL:
		string evalURL = "if(window.location.href.indexOf('b=') > -1){" +
		                   "SendMessage('Main Camera', 'setLoadFromBookmark', 'true');}";

		Application.ExternalEval (evalURL);

		if (variables.loadBookmarkFromURL) {
			Application.ExternalEval("function getParameterByName(name, url) {" +
				"if (!url) url = window.location.href;" +
				"name = name.replace(/[\\[\\]]/g, '\\\\$&');" +
				"var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)')," +
				"results = regex.exec(url);" +
				"if (!results) return null;" +
				"if (!results[2]) return '';" +
				"return decodeURIComponent(results[2].replace(/\\+/g, ' ')); }" +
				"SendMessage('Main Camera', 'setURLbookmarkString', getParameterByName('b'));" );
		}

		// The following variables are optional inputs
		string evalRendering = "if(fpb.opacity){SendMessage('Main Camera', 'setOpacity', fpb.opacity)};" +
							   "if(fpb.threshold){SendMessage('Main Camera', 'setThreshold', fpb.threshold);}" +
							   "if(fpb.intensity){SendMessage('Main Camera', 'setIntensity', fpb.intensity)};";

		Application.ExternalEval (evalRendering);

	}
	
	// Update is called once per frame
	void Update () {
		// Nothing to do. 
	}

	// The following C# functions can be called from Javascript: 
	//  Configuration options
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
		variables.pathToImages = javascriptString; // is this ../ problematic? Now got an absolute path to images, should be ok..
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

	public void setAtlasMode(string boolean){
		variables.loadAtlasDirectly = (boolean=="true") ? true : false;
	}

	public void setSliceWidth(float javascriptFloat){
		variables.sliceWidth = javascriptFloat;
	}

	public void setSliceHeight(float javascriptFloat){
		variables.sliceHeight = javascriptFloat;
	}

	//  Check if there is a bookmark in the URL
	public void setLoadFromBookmark(string boolean){
		variables.loadBookmarkFromURL = (boolean=="true") ? true : false;
	}

	//  Optional rendering input variables
	public void setOpacity(float javascriptFloat)
	{	variables.defaultOpacity = javascriptFloat;	}

	public void setThreshold(float javascriptFloat)
	{	variables.defaultThreshold = javascriptFloat;	}

	public void setIntensity(float javascriptFloat)
	{	variables.defaultIntensity = javascriptFloat;	}

	public void setURLbookmarkString(string javascriptString){
		variables.setViewMemory(javascriptString);
	}
	public void setImageAlpha(string boolean){
		variables.imageAlpha = (boolean=="true") ? true : false;
	}

}
