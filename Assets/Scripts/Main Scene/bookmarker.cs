using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
 * TODO: add handling for bookmarks that dont' exist yet
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
	public Slider opacity;
	public Slider intensity;
	public Slider threshold;
	public Dropdown projectionType;

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

	private bool bookmarkInURL = false;
	private bool mouseState;

	private FpbBookmark urlBookmark = null;
	private FpbBookmark browserBookmark = null;

	void Awake(){
		// Check for bookmark in url:
		Application.ExternalEval("if(window.location.href.indexOf('b=') > -1){fpcanvas.SendMessage('Cube', 'setBookmarkInURL', 'true');}");	
	}

	void Start(){
		if (bookmarkInURL) {
			// Try to load bookmark from URL
			string evalMe = "function getParameterByName(name, url) {" +
			                "if (!url) url = window.location.href;" +
			                "name = name.replace(/[\\[\\]]/g, '\\\\$&');" +
			                "var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)')," +
			                "results = regex.exec(url);" +
			                "if (!results) return null;" +
			                "if (!results[2]) return '';" +
			                "return decodeURIComponent(results[2].replace(/\\+/g, ' ')); }" +
			                "fpcanvas.SendMessage('Main Camera', 'getBookmarkFromURL', getParameterByName('b'));";
		
			Application.ExternalEval (evalMe);
			state = 2; 
			loadBookmark (urlBookmark);
		} else {
			// Load rendering paramters from webpage, or use default.
			opacity.value = (variables.fpbJSON.opacity != null) ? variables.fpbJSON.opacity : 5.0f;
			intensity.value = (variables.fpbJSON.intensity != null) ? variables.fpbJSON.intensity : 1.0f;
			threshold.value = (variables.fpbJSON.threshold != null) ? variables.fpbJSON.threshold : 0.2f;
			state = 0;
		}
	}

	void Update(){
		if (state == 0) {
			// Default state; wait for number to restore
			if (!variables.freezeAll) {
				if (Input.GetKeyUp (KeyCode.B)) {
					state = 1;
					//variables.freezeAll = true;
				} else {
					for (int i = 0; i < 10; i++) {
						// Start restoring bookmark
						string iStr = i.ToString ();
						if (Input.GetKeyUp (iStr)) {
							print ("Restoring bookmark " + i);
							restoreBookmarkFromBrowser (i);
						}
					}
				}
			}

		} else if (state == 1) {
			// Ready to restore a bookmark
			for (int i = 0; i < 10; i++) {
				string iStr = i.ToString ();
				if (Input.GetKeyUp (iStr)) {
					print("Saving bookmark " + i);
					addBookmarkToBrowser (i);
					state = 0;
				}
			}

		}
	}

	private void restoreBookmarkFromBrowser(int bookmarkNumber){
		// Read localStorage using javascript
		string uniqueName = variables.fpbJSON.uniqueName;
		#if UNITY_EDITOR
		if(EditorPrefs.HasKey("fpb-" + uniqueName + "-bookmark" + bookmarkNumber)){
			browserBookmark = decodeBookmark(EditorPrefs.GetString("fpb-" + uniqueName + "-bookmark" + bookmarkNumber));
		} else {
			browserBookmark = decodeBookmark("Could not find bookmark " + bookmarkNumber);
		}
		#else
		string evalMe = "var cookieString = localStorage.getItem('fpb-" + uniqueName + "-bookmark" + bookmarkNumber + "');" +
			"if (cookieString == null){" +
			"bookmark64String = 'Could not find bookmark " + bookmarkNumber + "';}" +
			"fpcanvas.SendMessage('Cube', 'sendBookmark', bookmark64String);";
		Application.ExternalEval (evalMe);
		#endif

		loadBookmark (browserBookmark);
	}

	private void loadBookmark(FpbBookmark bookmarkToLoad){
		Camera.main.transform.position = bookmarkToLoad.cameraPosition;
		Camera.main.transform.rotation = bookmarkToLoad.cameraRotation;
		cubeObject.transform.rotation  = bookmarkToLoad.cubeRotation;

		GameObject parent = GameObject.Find (bookmarkToLoad.cuttingParent);
		cuttingPlaneObject.transform.SetParent (parent.transform);
		cuttingPlaneObject.transform.localPosition = bookmarkToLoad.cuttingPlanePosition;
		cuttingPlaneObject.transform.localRotation = bookmarkToLoad.cuttingPlaneRotation;

		opacity.value = bookmarkToLoad.opacity;
		intensity.value = bookmarkToLoad.intensity;
		threshold.value = bookmarkToLoad.threshold;
		projectionType.value = bookmarkToLoad.projectionType;

		variables.sectionMode = bookmarkToLoad.getSectionMode();
		variables.hidePanels = bookmarkToLoad.getBindingBox();
	}

	private void addBookmarkToBrowser(int bookmarkNumber){
		FpbBookmark bookmarkToSave = saveBookmark ();
		string string64ToSave = encodeBookmark (bookmarkToSave);
		print (variables.fpbJSON.uniqueName); // default fpb json for in the editor?? 

		#if UNITY_EDITOR
		EditorPrefs.SetString("fpb-" + variables.fpbJSON.uniqueName + "-bookmark" + bookmarkNumber, string64ToSave);
		#else
		string evalMe = "localStorage.setItem('fpb-" + variables.fpbJSON.uniqueName + "-bookmark" + bookmarkNumber + "', '" + string64ToSave + "');";
		Application.ExternalEval (evalMe);
		#endif
	}

	private FpbBookmark saveBookmark(){
		FpbBookmark newBookmark = new FpbBookmark ();
		newBookmark.cameraPosition = Camera.main.transform.position;
		newBookmark.cameraRotation = Camera.main.transform.rotation;
		newBookmark.cubeRotation = cubeObject.transform.rotation;

		newBookmark.cuttingParent = cuttingPlaneObject.transform.parent.name;
		newBookmark.cuttingPlanePosition = cuttingPlaneObject.transform.localPosition;
		newBookmark.cuttingPlaneRotation = cuttingPlaneObject.transform.localRotation;

		newBookmark.opacity = opacity.value;
		newBookmark.intensity = intensity.value;
		newBookmark.threshold = threshold.value;
		newBookmark.projectionType = projectionType.value;

		newBookmark.setSectionMode (variables.sectionMode);
		newBookmark.setBindingBox (variables.hidePanels);
		return newBookmark;
	}

	public void setBookmarkInURL(string jsString){
		bookmarkInURL = jsString == "true" ? true : false;
	}

	public void getBookmarkFromURL(string urlBookmarkString64){
		// Decode base-64 string
		urlBookmark = decodeBookmark(urlBookmarkString64);
	}

	public void getBookmarkFromBrowser(string browserBookmarkString64){
		// Decode base-64 string
		browserBookmark = decodeBookmark(browserBookmarkString64);
	}

	private string encodeBookmark(FpbBookmark bookmarkToEncode){
		string jsonBookmarkString = JsonUtility.ToJson (bookmarkToEncode);
		byte[] data = System.Text.Encoding.UTF8.GetBytes (jsonBookmarkString);
		return System.Convert.ToBase64String (data);
	}

	private FpbBookmark decodeBookmark(string bookmarkString64){
		byte[] data = System.Convert.FromBase64String(bookmarkString64);
		string jsonBookmarkString = System.Text.Encoding.UTF8.GetString (data);
		return JsonUtility.FromJson<FpbBookmark> (jsonBookmarkString);
	}
}
