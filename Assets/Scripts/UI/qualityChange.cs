using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class qualityChange : MonoBehaviour {

	public Dropdown qualityDropdown;
	public GameObject bookmarker;

	private bool justStarted;

	// Use this for initialization
	void Start () {
		justStarted = true;

		switch (variables.getQuality ()) {
		case "Low":
			qualityDropdown.value = 0;
			break;
		case "Medium":
			qualityDropdown.value = 1;
			break;
		case "High":
			qualityDropdown.value = 2;
			break;
		case "Top":
			qualityDropdown.value = 3;
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void updateQuality(int newValueEnum){
		if (!justStarted) {
			//TODO: should probably make some effort to restore the same view... 
			if (newValueEnum == 0) {
				variables.setQuality ("Low");
			} else if (newValueEnum == 1) {
				variables.setQuality ("Medium");
			} else if (newValueEnum == 2) {
				variables.setQuality ("High");
			} else if (newValueEnum == 3) {
				variables.setQuality ("Top");
			}

			variables.setViewMemory (bookmarker.GetComponent<bookmarker> ().getBookmarkString (""));
			UnityEngine.SceneManagement.SceneManager.LoadScene ("main");
		}
	}

	void LateUpdate(){
		justStarted = false;
	}
}
