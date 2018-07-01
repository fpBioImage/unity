using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperToolsScript : MonoBehaviour {

	public GameObject devPanel;

	public GameObject renderCube;

	public Slider xySlider; 
	public Slider zSlider;
	public Slider oSlider;
	public Slider iSlider;
	public Slider cSlider;

	public Text xyText;
	public Text zText;
	public Text oText;
	public Text iText;
	public Text cText;

	private bool sliderValues = false;

	// Use this for initialization
	void Start () {
		xyText.gameObject.SetActive(false);
		zText.gameObject.SetActive(false);
		oText.gameObject.SetActive(false);
		iText.gameObject.SetActive(false);
		cText.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if (sliderValues) {
			xyText.text = string.Format("{0:0.}", xySlider.value);
			zText.text = string.Format("{0:0.}", zSlider.value);
			oText.text = string.Format("{0:0.00}", oSlider.value);
			iText.text = string.Format("{0:0.00}", iSlider.value);
			cText.text = string.Format("{0:0.00}", cSlider.value);
		}
	}

	public void ToggleFPSCount(bool toggle){
		transform.GetComponentInParent<FPSDisplay> ().enabled = toggle;
	}

	public void ToggleRainbowCube(bool toggle){
		renderCube.GetComponent<Renderer> ().material.SetInt ("_RainbowCube", toggle ? 1 : 0);
	}

	public void ToggleSliderValues(bool toggle){
		sliderValues = toggle;
		xyText.gameObject.SetActive(toggle);
		zText.gameObject.SetActive(toggle);
		oText.gameObject.SetActive(toggle);
		iText.gameObject.SetActive(toggle);
		cText.gameObject.SetActive(toggle);
	}

	public void CloseDevPanel(){
		devPanel.SetActive (false);
	}
}
