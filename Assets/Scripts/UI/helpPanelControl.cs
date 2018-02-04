using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class helpPanelControl : MonoBehaviour {

	private bool isHidden = true;
	public GameObject helpPanel;
	public Button helpButton;

	private Color normalColor = Color.white;
	private Color pressedColor = new Color(200.0f / 255.0f, 200.0f / 255.0f, 200.0f / 255.0f, 1.0f);

	// Use this for initialization
	void Start () {
		helpPanel.SetActive (false);
		isHidden = true;
	}
	
	// Update is called once per frame
	void Update () {
		if ( Input.GetKeyDown(KeyCode.M) && !variables.freezeAll ){
			showHideHelp();
		}
	}

	public void showHideHelp() {
		if (isHidden) {
			helpPanel.SetActive (true);
			helpButton.image.color = pressedColor;
			isHidden = false;
		} else {
			helpPanel.SetActive (false);
			helpButton.image.color = normalColor;
			isHidden = true;
		}
	}

}
