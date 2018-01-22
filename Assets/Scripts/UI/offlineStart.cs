using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

// Offline FPBioimage is still under development. 

/*
 * This script helps out the UI elements in the scene
 * 'offlineHome'.
*/

public class offlineStart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	public GameObject variableHolder;

	public InputField xText;
	public InputField yText;
	public InputField zText;

	private Color white = new Color(255.0f, 255.0f, 255.0f);
	private Color offWhite = new Color(0.914f, 0.914f, 0.914f);

	// Use this for initialization
	void Start () {
	
	}

	public void OnPointerEnter(PointerEventData eventData){
		GetComponent<Text> ().color = white;
	}

	public void OnPointerExit(PointerEventData eventData){
		GetComponent<Text> ().color = offWhite;
	}

	public void OnPointerClick(PointerEventData eventData){
		DontDestroyOnLoad (variableHolder);

		float.TryParse (xText.text, out variables.voxelSize[0]);
		float.TryParse (yText.text, out variables.voxelSize[1]);
		float.TryParse (zText.text, out variables.voxelSize[2]);

		UnityEngine.SceneManagement.SceneManager.LoadScene ("offlineLoader");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
