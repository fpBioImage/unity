using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FpbJSON {
		public string uniqueName;
		public int numberOfImages;
		public int sliceWidth;
		public int sliceHeight;
		public string imagePrefix;
		public string numberingFormat;
		public string pathToImages;
		public Vector3 voxelSize;
		public float opacity = -1.0f;
		public float intensity = -1.0f;
		public float threshold = -1.0f;
		public int projection = -1;

		public string atlasMode; // note that bools are stored as strings...
		public string imageAlpha;

	public bool getAtlasMode(){
		return (atlasMode == "true" || atlasMode == "1") ? true : false;
	}

	public bool getImageAlpha(){
		return (imageAlpha == "true" || imageAlpha == "1") ? true : false;
	}

	public FpbJSON(){
	}

	public FpbJSON(bool offlineMode){
		if (offlineMode) {
			uniqueName = "mouse";
			numberOfImages = 255;
			sliceWidth = 336;
			sliceHeight = 255;
			imagePrefix = "mouse_z";
			numberingFormat = "0000";
			pathToImages = "C:\\Users\\carcu\\Documents\\Unity Projects\\FP Bioimage\\Builds\\mouse\\";
			voxelSize = new Vector3 (1.0f, 1.0f, 1.0f);
			opacity = 5.0f;
			intensity = 1.0f;
			threshold = 0.2f;
			projection = 1;
			atlasMode = "true";
			imageAlpha = "true";
		}
	}
}
