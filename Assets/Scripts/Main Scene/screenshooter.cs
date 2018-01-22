using UnityEngine;
using System.Collections;

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

	private bool takeHiResShot = false;

	private int resWidth = 1000;
	private int resHeight = 500;

	public GameObject cuttingQuad;
	private Camera renderCamera;

	// Use this for initialization
	void Start () {
		renderCamera = new Camera();
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.V) && !variables.getFreezeAll ()) {
			//cuttingQuad.SetActive (false);
			takeHiResShot = true;
			//StartCoroutine(takeHiResShot());
			//string filepath = System.IO.Path.Combine(Application.dataPath, "screenshot.png");
			//Debug.Log (filepath);
			//Application.CaptureScreenshot(filepath);
		}
	}




	void OnPreRender(){
		if (takeHiResShot) {
			renderCamera.CopyFrom (Camera.main);

			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			renderCamera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
			//Camera.main.renderingPath = RenderingPath.Forward;
			renderCamera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			renderCamera.targetTexture = null;
			RenderTexture.active = null; 
			Destroy (rt);
			Destroy (renderCamera);
			byte[] bytes = screenShot.EncodeToPNG();

			Application.ExternalCall("download", "data:image/png;base64," + System.Convert.ToBase64String(bytes), "Screenshot " + System.DateTime.Now.ToString ("yyyy-MM-dd") + " at " + System.DateTime.Now.ToString("HH.mm.ss") + ".png");
			takeHiResShot = false;
			//cuttingQuad.SetActive (true);
		}
	}

	/*IEnumerator takeHiResShot()
	{
		yield return new WaitForEndOfFrame ();

		Texture2D rt = new Texture2D (resWidth, resHeight);

		rt.ReadPixels (new Rect (0, 0, resWidth, resHeight), 0, 0);
		rt.Apply ();

		yield return 0;

		byte[] bytes = rt.EncodeToPNG ();
		Application.ExternalCall("download", "data:image/png;base64," + System.Convert.ToBase64String(bytes), "Screenshot " + System.DateTime.Now.ToString ("yyyy-MM-dd") + " at " + System.DateTime.Now.ToString("HH.mm.ss") + ".png");

		Destroy (rt);
		cuttingQuad.SetActive (true);
	}*/

}
