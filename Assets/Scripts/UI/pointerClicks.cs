using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * This script controls the hovering and clicking of the mouse
 * on the home screen. 
 * 
 * It could do with a little bit of work regarding returning to 
 * the home screen from the viewer. The script should read what
 * the current quality mode is on start, and embolden accordingly.
 * 
*/

public class pointerClicks : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	public GameObject variableHolder;
	public Font regularFont;
	public Font boldFont;

	private Color white = new Color(255.0f, 255.0f, 255.0f);
	private Color offWhite = new Color(0.914f, 0.914f, 0.914f);
	private Color grey = new Color(0.69f, 0.69f, 0.69f);

	private string textName;

	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		textName = transform.name;

		if ((textName.Equals ("Low") || textName.Equals ("Medium") || textName.Equals ("High") || textName.Equals ("Top")) && !textName.Equals (variables.getQuality ())) {
			GetComponent<Text> ().font = regularFont;
			GetComponent<Text> ().color = grey;
		} else if (!textName.Equals("Start")){
			GetComponent<Text> ().font = boldFont;
			GetComponent<Text> ().color = offWhite;
		}

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonUp(0)) {
			if ((textName.Equals ("Low") || textName.Equals ("Medium") || textName.Equals ("High") || textName.Equals ("Top")) && !textName.Equals (variables.getQuality ())) {
				GetComponent<Text> ().font = regularFont;
				GetComponent<Text> ().color = grey;
			}
		}

	}


	public void OnPointerEnter(PointerEventData eventData){
		GetComponent<Text> ().color = white;
	}

	public void OnPointerExit(PointerEventData eventData){
		if (textName.Equals ("Start") || textName.Equals(variables.getQuality()) || textName.Equals("Choose") ) {
			GetComponent<Text> ().color = offWhite;
		}else{
			GetComponent<Text> ().color = grey;
		}
	}

	public void OnPointerClick(PointerEventData eventData){
		if (textName.Equals ("Low") || textName.Equals ("Medium") || textName.Equals ("High") || textName.Equals ("Top")) {
			variables.setQuality (textName);
			GetComponent<Text> ().font = boldFont;
		} else if (textName.Equals ("Start")) {
			DontDestroyOnLoad (variableHolder);
			UnityEngine.SceneManagement.SceneManager.LoadScene ("loading");
		} else if (textName.Equals ("Info")) {
			Application.ExternalEval ("window.open(\"http://fpb.ceb.cam.ac.uk/\")");
		}
			
	}

}