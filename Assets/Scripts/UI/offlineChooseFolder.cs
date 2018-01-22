// TODO: sort out a Unity file browser

// Offline FP Bioimage is still under development. 

/*
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

public class offlineChooseFolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
	
	public Text folderText;

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
		string path = EditorUtility.OpenFolderPanel("Select a directory of png slices", "", "");
		// // This doesn't work except in the unity editor! :( 
		// // I guess I need to get a unity file browser
		// grim :(
		if (path != "") {

			variables.pathToImages = path;

			string[] splitString = path.Split ('/');
			folderText.text = ".../" + splitString [splitString.Length - 1];

			xText.text = "1";
			yText.text = "1";
			zText.text = "1";
		}

	}

	void Update () {

	}

}
*/