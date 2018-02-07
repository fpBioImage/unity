using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class qualityUpdate : MonoBehaviour {

	public GameObject qualityBox;
	public Camera fpCamera;
	public Slider resXY;
	public Slider resZ;
	public Dropdown interp;
	public Dropdown preset;
	public GameObject cube;
	//public RenderTexture fullScreenRT;

	private Material cubeMaterial;

	private int _interpID;
	private int _stepsID;

	void Awake() {
		setQuadSize ();
	}

	public void setXYFromBrowser(string jsonString){
		resXY.value = float.Parse (jsonString);
	}
	public void setZFromBrowser(string jsonString){
		resZ.value = float.Parse (jsonString);
	}
	public void setInterpFromBrowser(string jsonString){
		interp.value = int.Parse (jsonString);
	}
	

	// Use this for initialization
	void Start () {
		cubeMaterial = cube.GetComponent<Renderer> ().material;
		_stepsID = Shader.PropertyToID ("_Steps");
		_interpID = Shader.PropertyToID ("_Interp");
		Application.ExternalEval ("var qualityExists = localStorage.getItem('fpb-quality');" + 
			"if(qualityExists!=null) {" +
			"fpcanvas.SendMessage('Full Screen Quad', 'setXYFromBrowser', localStorage.getItem('fpb-quality-resXY'));" +
			"fpcanvas.SendMessage('Full Screen Quad', 'setZFromBrowser', localStorage.getItem('fpb-quality-resZ'));" +
			"fpcanvas.SendMessage('Full Screen Quad', 'setInterpFromBrowser', localStorage.getItem('fpb-quality-interp'))};");
		updateQuality ();
	}

	// Update is called once per frame
	void Update () {
	}

	public void openQualityBox(){
		qualityBox.SetActive (true);
	}
	public void closeQualityBox(){
		saveQualitySettingsToBrowser ();
		qualityBox.SetActive (false);
	}

	public void saveQualitySettingsToBrowser(){
		string evalMe = "localStorage.setItem('fpb-quality', 'true');" +
			"localStorage.setItem('fpb-quality-resXY', '" + resXY.value + "');" + 
			"localStorage.setItem('fpb-quality-resZ', '" + resZ.value + "');" + 
			"localStorage.setItem('fpb-quality-interp', '" + interp.value + "');";
		#if UNITY_EDITOR
		// Should save quality settings to preferences file. 
		#else
		Application.ExternalEval (evalMe);
		#endif
		print ("Quality settings saved to browser for next visit.");
	}

	public void updateQuality(){
		cubeMaterial.SetFloat (_stepsID, resZ.value);
		cubeMaterial.SetFloat (_interpID, interp.value);

		RenderTexture fullScreenRT = new RenderTexture (1, 1, 16);

		if (Screen.width > Screen.height) {
			float xRes = Mathf.Clamp (resXY.value, 10.0f, Screen.width);
			float yRes = xRes * (float)Screen.height / (float)Screen.width;

			fullScreenRT.width = (int)xRes;
			fullScreenRT.height = (int)yRes;
		} else {
			float yRes = Mathf.Clamp (resXY.value, 10.0f, Screen.height);
			float xRes = yRes * (float)Screen.width / (float)Screen.height;

			fullScreenRT.height = (int)yRes;
			fullScreenRT.width = (int)xRes;
		}

		GetComponent<Renderer> ().material.mainTexture = fullScreenRT;
		fpCamera.targetTexture = fullScreenRT;

		// Set preset box
		if (resXY.value == 256.0f && resZ.value == 64.0f && interp.value == 0) {
			preset.value = 0;
		} else if (resXY.value == 768.0f && resZ.value == 150.0f && interp.value == 1) {
			preset.value = 1;
		} else if (resXY.value == 1024.0f && resZ.value == 256.0f && interp.value == 1) {
			preset.value = 2;
		} else if (resXY.value == 2048.0f && resZ.value == 768.0f && interp.value == 2) {
			preset.value = 3;
		} else {
			preset.value = 4;
		}
			
	}

	void LateUpdate(){
		setQuadSize ();
	}

	public void presetValueChanged(int newValue){
		switch (newValue) {
		case 0:
			// Low Quality
			resXY.value = 256.0f;
			resZ.value = 64.0f;
			interp.value = 0;
			break;
		case 1:
			// Medium Quality
			resXY.value = 768.0f;
			resZ.value = 150.0f;
			interp.value = 1;
			break;
		case 2:
			// High Quality
			resXY.value = 1024.0f;
			resZ.value = 256.0f;
			interp.value = 1;
			break;
		case 3:
			// Top Quality (silly)
			resXY.value = 2048.0f;
			resZ.value = 768.0f;
			interp.value = 2;
			break;
		case 4: default:
			// Custom
			// Don't change any settings
			break;
		}
		updateQuality ();
	}

	void setQuadSize(){
		float frustumHeight = 2.0f * Mathf.Tan (Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
		float frustumWidth = frustumHeight * Camera.main.aspect;
		transform.localScale = new Vector3 (frustumWidth, frustumHeight);
	}
}
