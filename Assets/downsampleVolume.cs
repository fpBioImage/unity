using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class downsampleVolume : MonoBehaviour {

	public int downsample = 2;
	public LayerMask volumeLayer;
	public Shader compositeShader;
	public Shader rayMarchShader;

	private Camera ppCamera;
	private Material compositeMaterial;
	private Material rayMarchMaterial;

	void Awake () {
		rayMarchMaterial = new Material (rayMarchShader);
		compositeMaterial = new Material (compositeShader);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {}
		
	

	/*
	void OnRenderImage(RenderTexture source, RenderTexture destination){
		var width = source.width / downsample;
		var height = source.height / downsample;

		if (ppCamera == null) {
			var go = new GameObject ("PPCamera");
			ppCamera = go.AddComponent<Camera> ();
			ppCamera.enabled = false;
		}
			
		ppCamera.CopyFrom (GetComponent<Camera> ());
		ppCamera.clearFlags = CameraClearFlags.Nothing;
		ppCamera.backgroundColor = Color.black;
		ppCamera.cullingMask = volumeLayer;

		var volumeTarget = RenderTexture.GetTemporary (width, height, 0);

		Graphics.Blit (null, volumeTarget, rayMarchMaterial);

		//Composite
		compositeMaterial.SetTexture ("_BlendTex", volumeTarget);
		Graphics.Blit (source, destination, compositeMaterial);

		RenderTexture.ReleaseTemporary (volumeTarget);
	}*/



}
