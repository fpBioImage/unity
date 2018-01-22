using UnityEngine;
using System.Collections;

/*
 * This script contains global variables that are shared
 * between all scenes. It is never destroyed as long as
 * the program runs - that is, until the user closes the
 * webpage FP Bioimage is running in, or refreshes the 
 * page. 
 * 
 * I haven't been consistent with private and public 
 * variables. I began making all variables private, and
 * using get and set functions. However it became more
 * convenient to just declare variables as public and change
 * them directly, hence the disparity. 
 * 
 * 
*/

public class variables : MonoBehaviour {

	private static string quality = "Medium";
	private static string menuCameraTarget = "Home Page";
	private static bool freezeMouse = true;
	private static bool freezeAll = false;
	private static bool jamesMode = false;

	public static bool showBindingBox = true;
	public static bool sectionMode = false;

	public static bool loadBookmarkFromURL = false;
	public static bool bookmarkHover = true;

	public static bool offlineMode = false;
	public static bool loadAtlasDirectly = false;
	public static bool pngMode = true;

	// config strings
	public static string pathToImages;
	public static string imagePrefix;
	public static string numberingFormat;
	public static int numberOfImages;
	public static string uniqueName;
	public static float[] voxelSize = new float[3] {1.0f,1.0f,1.0f};

	// optional rendering settings
	public static float defaultOpacity = 0.95f;
	public static float defaultThreshold = 0.05f;
	public static float defaultIntensity = 0.75f;

	// floats for ray shader
	public static float numSlices;
	public static float sliceWidth;
	public static float sliceHeight;
	public static float slicesPerRow;
	public static float texturesPerSlice;
	public static float tPixelWidth;
	public static float tPixelHeight;
	public static float atlasHeight;
	public static float atlasWidth;
	public static bool imageAlpha = false;
	public static Vector3 cubeSize;

	public static float cubeScale;

	public static Texture2D[] atlasArray = new Texture2D[8]; // preallocating this size is crucial!

	private static string viewMemory = "";

	public static bool leftPanelOpen = false;
	public static bool rightPanelOpen = false;
	public static bool hidePanels = false;

	void Awake() {
		//public void Awake()
		//{
			DontDestroyOnLoad(this);
			
			if (FindObjectsOfType(GetType()).Length > 1)
			{
				Destroy(gameObject);
			}
		//}
	}

	// Get and set functions
	public static void setQuality(string newQuality){
		quality = newQuality;
	}

	public static string getQuality() {
		return quality;
	}

	public static void setViewMemory(string newString){
		viewMemory = newString;
	}

	public static string getViewMemory(){
		return viewMemory;
	}

	public static void setMenuCameraTarget(string newTarget){
		menuCameraTarget = newTarget;
	}

	public static string getMenuCameraTarget(){
		return menuCameraTarget;
	}

	public static void toggleFreezeMouse(){
		freezeMouse = !freezeMouse;
	}

	public static void setFreezeMouse(bool freeze){
		if (freeze) {
			freezeMouse = true;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		} else if (!freeze) {
			freezeMouse = false;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		//freezeMouse = freeze;
	}

	public static bool getFreezeMouse(){
		return freezeMouse;
	}

	public static bool getFreezeAll(){
		return freezeAll;
	}

	public static void setFreezeAll(bool freeze){
		freezeAll = freeze;
	}

	public static void enableJamesMode(){
		jamesMode = true;
	}

	public static bool getJamesMode(){
		return jamesMode;
	}

	
	// Start and update functions do nothing for this script. 
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
