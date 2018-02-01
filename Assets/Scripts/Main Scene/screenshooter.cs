using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

/*
 * This script does not work! 
 * 
 * The aim of this script is to take a high-resolution
 * screenshot of the current view. However, a grey
 * plane is visible over the scene. This needs debugging
 * to get screenshooting working.
 * 
*/

public class screenshooter : MonoBehaviour {

	public InputField wRes;
	public InputField hRes;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		// Screen shots
		if (Input.GetKeyUp (KeyCode.V)) {
			takeSreenshot (false);
		}

		if (Input.GetKeyUp (KeyCode.C)) {
			takeSreenshot (true);
		}
		
	}

	public void takeSreenshot(bool hiRes){
			int camWidth;
			int camHeight;

			if (hiRes) {
				bool wOK = int.TryParse (wRes.text, out camWidth);
				bool hOK = int.TryParse (hRes.text, out camHeight);

				if (!wOK || camWidth < 1) {
					camWidth = 1920;
				} 
				if (!hOK || camHeight < 1) {
					camHeight = 1080;
				}
			} else {
				camWidth = Camera.main.pixelWidth;
				camHeight = Camera.main.pixelHeight;
			}

			Camera mainCamera = Camera.main;

			RenderTexture rt = new RenderTexture(camWidth, camHeight, 24);
			mainCamera.targetTexture = rt;
			Texture2D snapShot = new Texture2D(camWidth, camHeight, TextureFormat.RGB24, false);
			mainCamera.Render();
			RenderTexture.active = rt;
			snapShot.ReadPixels(new Rect(0, 0, camWidth, camHeight), 0, 0);
			mainCamera.targetTexture = null;
			RenderTexture.active = null;
			Destroy(rt);

			byte[] bytes = snapShot.EncodeToPNG ();

			#if UNITY_EDITOR
			File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bytes);
			#else 
			Application.ExternalCall ("download", "data:image/png;base64," + System.Convert.ToBase64String (bytes), "Screenshot " + System.DateTime.Now.ToString ("yyyy-MM-dd") + " at " + System.DateTime.Now.ToString ("HH.mm.ss") + ".png");
			#endif

	}

}
