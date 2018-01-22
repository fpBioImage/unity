using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * This script deals with the bookmarking feature of FPBioimage.  
 * It has been known to be buggy in the past; if you find a bug,
 * please submit an issue on the github page. 
 * 
 * The script uses a state machine to deal with the various
 * stages of the bookmarking process. Google 'state machine' to
 * find out more. 
 * 
 * It used to save bookmarks as cookies in the browser; however
 * due to the 4kB size limit of cookies, it now uses the HTML5
 * localStorage API. Since other elements of this program require
 * HTML5 anyway this shouldn't be a problem. 
 * 
*/
public class bookmarker : MonoBehaviour {

	// Set up variables
	private int state = 0;
	private string savingBookmarkNumber;
	private string restoringBookmarkNumber;

	[Header("Elements to bookmark")]
	public GameObject cubeObject;
	public GameObject cuttingPlaneObject;

	[Header("UI Elements")]
	public GameObject annotationBox;
	public GameObject dummySelectable;
	public GameObject backgroundPanel;

	public Text annotationInputText;
	public Text annotationPlaceholder;

	public Text annotationInputTitleText;
	public Text confirmationText;
	public Text restoredAnnotationText;

	public GameObject shareBookmarkText;

	private InputField annotationInput;

	private bool mouseState;

	public void setURLbookmarkString(string javascriptString){
		variables.setViewMemory(javascriptString);
	}

	// Use this for initialization
	void Start () {
		state = 0;
		annotationBox.SetActive (false);
		annotationInputTitleText.enabled = false;
		confirmationText.enabled = false;
		restoredAnnotationText.enabled = false;
		backgroundPanel.SetActive(false);
		shareBookmarkText.SetActive (false);

		annotationInput = annotationBox.GetComponent<InputField> ();

		variables.bookmarkHover = false;

	}

	public void startBookmarkCreation(){
		mouseState = variables.getFreezeMouse ();
		state = 1;
	}

	public void startBookmarkRestoration(){
		state = 6;
	}

	public void setDefaultView(){
		if (variables.loadBookmarkFromURL) {
			Application.ExternalEval("function getParameterByName(name, url) {" +
				"if (!url) url = window.location.href;" +
				"name = name.replace(/[\\[\\]]/g, '\\\\$&');" +
				"var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)')," +
				"results = regex.exec(url);" +
				"if (!results) return null;" +
				"if (!results[2]) return '';" +
				"return decodeURIComponent(results[2].replace(/\\+/g, ' ')); }" +
				"SendMessage('Main Camera', 'setURLbookmarkString', getParameterByName('b'));" );

			loadBookmarkFromString (variables.getViewMemory());
		} else {
			restoredCameraPosition ("0", false);
		}
	}

	// Update is called once per frame
	void Update () {
		// Default state: all states should eventually return to here
		if (state == 0) {
			//variables.setFreezeAll (false);

			if (!variables.getFreezeAll ()) {
				if (Input.GetKeyUp (KeyCode.B)) {
					// Start creating bookmark
					startBookmarkCreation ();
				}
				for (int i = 0; i < 10; i++) {
					// Start restoring bookmark
					string iStr = i.ToString ();
					if (Input.GetKeyUp (iStr)) {
						restoringBookmarkNumber = iStr;
						state = 3;
					}
				}
			}
		}

		// other states
		if (state == 1) {
			variables.setFreezeAll (true);
			initialisingBookmark ();
		} else if (state == 2) {
			variables.setFreezeAll (true);
			makingBookmark ();
		} else if (state == 3) {
			variables.setFreezeAll (true);
			restoringBookmark ();
		} else if (state == 4) {
			variables.setFreezeAll (true);
			confirmBookmarkSaved ();
		} else if (state == 5) {
			variables.setFreezeAll (true);
			displayingBookmark ();
		} else if (state == 6) {
			variables.setFreezeAll (true);
			restoreInfo ();

			for (int i = 0; i < 10; i++) {
				// Start restoring bookmark
				string iStr = i.ToString ();
				if (Input.GetKeyUp(iStr)) {
					confirmationText.text = "";
					restoringBookmarkNumber = iStr;
					state = 3;
				}
			}

		}

	}

	private void restoreInfo(){
		//variables.setFreezeMouse (true);
		confirmationText.text = "Press a number button to restore that bookmark. Esc to cancel";
		backgroundPanel.SetActive (true);
		confirmationText.enabled = true;

		if (Input.GetKeyUp (KeyCode.Escape) ) {
			// Cancel bookmark restoration
			confirmationText.enabled = false;
			confirmationText.text = "";
			backgroundPanel.SetActive (false);
			variables.setFreezeAll (false);
			state = 0;
			//variables.setFreezeMouse (mouseState);
		}
	}

	private void initialisingBookmark(){
		// runs in state 1
		variables.setFreezeMouse(true);
		confirmationText.text = "Press a number button to create a bookmark. Esc to cancel.";
		backgroundPanel.SetActive (true);
		confirmationText.enabled = true;

		// Escape doesn't work in full screen. I can't think of a better key though. (Perhaps ctrl+B?)
		if (Input.GetKeyUp (KeyCode.Escape) ) {
			// Cancel bookmark creation
			confirmationText.enabled = false;
			backgroundPanel.SetActive (false);
			variables.setFreezeAll (false);
			state = 0;
			variables.setFreezeMouse (mouseState);
		}

		for (int i = 0; i < 10; i++) {
			string iStr = i.ToString ();
			if (Input.GetKeyUp (iStr)) {
				// Show bookmark creation box, and move to state 2 to wait for input
				confirmationText.enabled = false;
				annotationInputTitleText.text = "Type annotation for Bookmark " + iStr + " (Esc to cancel):";
				annotationInputTitleText.enabled = true;

				annotationBox.SetActive (true);
				annotationInput.Select ();

				savingBookmarkNumber = iStr;
				restoringBookmarkNumber = iStr;
				state = 2;
			}
		}
	}

	private void makingBookmark(){
		// runs in state 2
		if ( Input.GetKeyUp (KeyCode.Escape) ) {
			annotationInputTitleText.enabled = false;
			annotationBox.SetActive (false);
			backgroundPanel.SetActive (false);
			dummySelectable.GetComponent<InputField> ().Select ();
			variables.setFreezeAll (false);
			state = 0;
		}
		if (Input.GetKeyUp (KeyCode.Return)) {
			string annotation = annotationInput.text;

			saveCameraPosition (savingBookmarkNumber, annotation);
			annotationInputTitleText.enabled = false;
			annotationBox.SetActive (false);
			annotationInput.text = "";
			dummySelectable.GetComponent<InputField> ().Select ();

			confirmationText.text = "Saved bookmark " + savingBookmarkNumber + ". Press '" + savingBookmarkNumber + "' to restore it.";

			// This is an Easter egg implemented to keep James happy. Any complaints should be addressed to him.  
			if (annotation.Equals ("I am James Manton.")) {
				variables.enableJamesMode ();
				confirmationText.text = "James mode activated. Clipping plane rotation unlocked!";
			}

			confirmationText.enabled = true;
			shareBookmarkText.SetActive (true);

			variables.bookmarkHover = false;
			state = 4;
		}
	}

	private void confirmBookmarkSaved() {
		// runs in state 4
		if (Input.anyKeyDown && !variables.bookmarkHover) {
			// Stop showing confirmation text as soon as something is pressed. 
			confirmationText.enabled = false;
			backgroundPanel.SetActive (false);
			shareBookmarkText.SetActive (false);
			variables.setFreezeMouse (mouseState);
			variables.setFreezeAll (false);
			state = 0;
		}
	}

	private void restoringBookmark() {
		// runs in state 3
		mouseState = variables.getFreezeMouse();
		variables.setFreezeMouse(true);

		// restored camera positition should set string of annotation 
		restoredCameraPosition (restoringBookmarkNumber, false);

		// now show restored annotation
		backgroundPanel.SetActive(true);
		restoredAnnotationText.enabled = true;

		state = 5;
	}

	private void displayingBookmark(){
		// runs in state 5
		if (Input.anyKeyDown && !variables.bookmarkHover) {
			// Stop showing annotation text as soon as something is pressed. 
			restoredAnnotationText.enabled = false;
			backgroundPanel.SetActive (false);
			shareBookmarkText.SetActive (false);
			variables.setFreezeMouse (mouseState);
			variables.setFreezeAll (false);
			state = 0;
		}
	}

	// Helper function to create the bookmarking string
	public string getBookmarkString(string annotation) {
		// Get properties of camera (7)
		float xPos = transform.position.x;
		float yPos = transform.position.y;
		float zPos = transform.position.z;
		float wRot = transform.rotation.w;
		float xRot = transform.rotation.x;
		float yRot = transform.rotation.y;
		float zRot = transform.rotation.z;

		// Get properties of cutting plane (8)
		string cuttingParent = cuttingPlaneObject.transform.parent.name;
		float cutXPos = cuttingPlaneObject.transform.localPosition.x;
		float cutYPos = cuttingPlaneObject.transform.localPosition.y;
		float cutZPos = cuttingPlaneObject.transform.localPosition.z;
		float cutXRot = cuttingPlaneObject.transform.localRotation.x;
		float cutYRot = cuttingPlaneObject.transform.localRotation.y;
		float cutZRot = cuttingPlaneObject.transform.localRotation.z;
		float cutWRot = cuttingPlaneObject.transform.localRotation.w;

		// Get properties of cube (4)
		float cubeXRot = cubeObject.transform.rotation.x;
		float cubeYRot = cubeObject.transform.rotation.y;
		float cubeZRot = cubeObject.transform.rotation.z;
		float cubeWRot = cubeObject.transform.rotation.w;

		// Get color properties (3)
		float opacity = GetComponent<mfRayMarching> ().opacity;
		float threshold = GetComponent<mfRayMarching> ().threshold;
		float intensity = GetComponent<mfRayMarching> ().intensity;

		// Get rendering mode (2)
		string sectionMode = variables.sectionMode ? "true" : "false";
		string bindingBox = variables.showBindingBox ? "true" : "false";

		// Check annotation (1)
		annotation = annotation.Replace("\\", "\\\\");
		annotation = annotation.Replace("'","\\'");
		annotation = annotation.Replace (" ", "_");

		string bookmarkString = "xPosE" + xPos +
		                        "QyPosE" + yPos + "QzPosE" + zPos + "QwRotE" + wRot + "QxRotE" + xRot +
		                        "QyRotE" + yRot + "QzRotE" + zRot + "QcutPE" + cuttingParent + "QcXPoE" + cutXPos +
		                        "QcYPoE" + cutYPos + "QcZPoE" + cutZPos + "QcXRoE" + cutXRot + "QcYRoE" + cutYRot +
		                        "QcZRoE" + cutZRot + "QcWRoE" + cutWRot + "QcubXE" + cubeXRot + "QcubYE" + cubeYRot +
		                        "QcubZE" + cubeZRot + "QcubWE" + cubeWRot +
		                        "QopacE" + opacity + "QthreE" + threshold + "QsmooE" + intensity +
		                        "QbboxE" + bindingBox + "QsmodE" + sectionMode + "QannoE" + annotation;

		return bookmarkString;

	}

	private void saveCameraPosition (string bookmarkNumber, string annotation){
		// saves the bookmark as a 'cookie' in the browser
		string bookmarkID = variables.uniqueName + bookmarkNumber;
		string bookmarkString = getBookmarkString (annotation);

		// Set up cookie using localStorage API (HTML5)
		string evalMe = "localStorage.setItem('bookmark" + bookmarkID + "', '" + bookmarkString + "');"; 

		Application.ExternalEval (evalMe);
	}

	public void createBookmarkURL(){
		string evalMe = "var pageHref = window.location.href;" +
		                "var bLoc = pageHref.indexOf('?b='); " +
		                "var pageURL = ''; " +
		                "if (bLoc > -1){" +
						"pageURL = pageHref.substring(0, bLoc);}" +
		                "else {" +
		                "pageURL = pageHref;}" +
						"SendMessage('Main Camera', 'setPageURL', pageURL);";

		Application.ExternalEval (evalMe);

		restoredCameraPosition (restoringBookmarkNumber, true); // this will set returnedString correctly

		string urlExtension = returnedString.Replace("\\", "\\\\").Replace("\'", "\\\'");

		string urlToShare = urlRoot + "?b=" + urlExtension;

//  	// I can't get cross-domain requests working, otherwise it would be nice to shrink the URL when it's created: 
//		WWW request = new WWW("http://tinyurl.com/api-create.php?url=" + urlToShare);
//		yield return request;
//		string tinyURL = request.text;

//		if (tinyURL.Length != 0 ) {
//			Application.ExternalEval ("window.prompt('Copy this link to share: (Ctrl+C / Cmd+C)', '" + tinyURL + "');");
//		} else {
			Application.ExternalEval ("window.prompt('Copy this link to share: (Ctrl+C / Cmd+C)', '" + urlToShare + "');");
//		}
	}

	// Helper function to get the URL of the webpage (without any bookmark). 
	private string urlRoot;
	public void setPageURL(string javascriptString){
		urlRoot = javascriptString;
	}

	private string returnedString;
	private void restoredCameraPosition (string bookmarkNumber, bool noShow){
		string bookmarkID = variables.uniqueName + bookmarkNumber;

		// Read localStorage using javascript
		string evalMe = "var cookieString = localStorage.getItem('bookmark" + bookmarkID + "');" +
		                "if (cookieString == null){" +
						"cookieString = 'Could not find bookmark " + bookmarkNumber + "';}" +
		                "SendMessage('Main Camera', 'setCookieString', cookieString);";

		Application.ExternalEval (evalMe);

		if ( returnedString.Contains("Could not find bookmark") ) {
			if (bookmarkNumber.Equals("0")){
				saveCameraPosition ("0", "Default view"); // This will only run the very first time this data is viewed (or if cookies are cleared)
			} else {
			restoredAnnotationText.text = returnedString;
			shareBookmarkText.SetActive (false);
			}
		} else if (!noShow) {
			loadBookmarkFromString (returnedString);
			restoredAnnotationText.text = "Bookmark " + bookmarkNumber + ": " + restoredAnnotationText.text;
			shareBookmarkText.SetActive (true);
		}
	}

	public void loadBookmarkFromString( string bookmarkAsString){
		// Parse the cookie string
		int i=0;
		string[] split = bookmarkAsString.Split('Q');

		string[] cameraValues = new string[25];
		foreach (string item in split){
			cameraValues [i] = item.Substring(5);
			i++;
		}

		// Assign values to variables
		float xPos = float.Parse (cameraValues [0]);
		float yPos = float.Parse (cameraValues [1]);
		float zPos = float.Parse (cameraValues [2]);
		float wRot = float.Parse (cameraValues [3]);
		float xRot = float.Parse (cameraValues [4]);
		float yRot = float.Parse (cameraValues [5]);
		float zRot = float.Parse (cameraValues [6]);

		string parentString = cameraValues [7];
		float cutXPos = float.Parse (cameraValues [8]);
		float cutYPos = float.Parse (cameraValues [9]);
		float cutZPos = float.Parse (cameraValues [10]);
		float cutXRot = float.Parse (cameraValues [11]);
		float cutYRot = float.Parse (cameraValues [12]);
		float cutZRot = float.Parse (cameraValues [13]);
		float cutWRot = float.Parse (cameraValues [14]);

		float cubeXRot = float.Parse (cameraValues [15]);
		float cubeYRot = float.Parse (cameraValues [16]);
		float cubeZRot = float.Parse (cameraValues [17]);
		float cubeWRot = float.Parse (cameraValues [18]);

		float opacity = float.Parse (cameraValues [19]);
		float threshold = float.Parse (cameraValues [20]);
		float intensity = float.Parse (cameraValues [21]);

		string bindingBox = cameraValues [22];
		string sectionMode = cameraValues [23];

		// Set camera position
		transform.position = new Vector3 (xPos, yPos, zPos);
		transform.rotation = new Quaternion (xRot, yRot, zRot, wRot);

		// Set clipping plane
		GameObject parent = GameObject.Find (parentString);
		cuttingPlaneObject.transform.SetParent (parent.transform);

		cuttingPlaneObject.transform.localPosition = new Vector3 (cutXPos, cutYPos, cutZPos);
		cuttingPlaneObject.transform.localRotation = new Quaternion (cutXRot, cutYRot, cutZRot, cutWRot);

		// Set object rotation
		cubeObject.transform.rotation = new Quaternion (cubeXRot, cubeYRot, cubeZRot, cubeWRot);

		// Set object color
		GetComponent<mfRayMarching> ().opacity = opacity;
		GetComponent<mfRayMarching> ().intensity = intensity;
		GetComponent<mfRayMarching> ().threshold = threshold;

		// Set rendering modes
		bool newBindingBox = (bindingBox == "true") ? true : false;
		variables.showBindingBox = newBindingBox;

		bool newSectionMode = (sectionMode == "true") ? true : false;
		variables.sectionMode = newSectionMode;

		restoredAnnotationText.text = cameraValues[24].Replace("_"," ");

		GetComponent<CameraMovement>().resetRotation ();
	}

	public void setCookieString( string javascriptString){
		// This helper function lets javascript send its cookie to Unity.
		returnedString = javascriptString;
	}


}
