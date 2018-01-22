using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setTexture : MonoBehaviour {

	public Shader rayMarchShader;
	private Material _rayMarchMaterial;

	void Awake() {
		_rayMarchMaterial = new Material (rayMarchShader);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void GenerateVolumeTexture()
	{
		_rayMarchMaterial.SetFloat ("_numSlices", variables.numSlices);
		_rayMarchMaterial.SetFloat ("_maxX", variables.slicesPerRow);
		_rayMarchMaterial.SetFloat ("_texturesPerSlice", variables.texturesPerSlice);
		_rayMarchMaterial.SetFloat ("_tPixelWidth", variables.tPixelWidth);
		_rayMarchMaterial.SetFloat ("_tPixelHeight", variables.tPixelHeight);
		_rayMarchMaterial.SetFloat ("_packedWidth", variables.atlasWidth);
		_rayMarchMaterial.SetFloat ("_packedHeight", variables.atlasHeight);
		_rayMarchMaterial.SetFloat ("_zScale", variables.cubeSize.normalized.z);

		_rayMarchMaterial.SetTexture ("_VolumeTex", variables.atlasArray[0]);
		_rayMarchMaterial.SetTexture ("_VolumeTex2", variables.atlasArray[1]);
		_rayMarchMaterial.SetTexture ("_VolumeTex3", variables.atlasArray[2]);
		_rayMarchMaterial.SetTexture ("_VolumeTex4", variables.atlasArray[3]);
		_rayMarchMaterial.SetTexture ("_VolumeTex5", variables.atlasArray[4]);
		_rayMarchMaterial.SetTexture ("_VolumeTex6", variables.atlasArray[5]);
		_rayMarchMaterial.SetTexture ("_VolumeTex7", variables.atlasArray[6]);
		_rayMarchMaterial.SetTexture ("_VolumeTex8", variables.atlasArray[7]);
	}
}
