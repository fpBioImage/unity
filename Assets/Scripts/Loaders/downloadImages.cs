using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class downloadImages : MonoBehaviour {

	public bool changingQuality = false;

	public GameObject infoBox;
	public Text infoText;
	public GameObject qualityButton;
	public GameObject qualityBox;
	public GameObject fullScreenQuad;

	public bool offlineMode = false;
	public bool atlasMode = true;
	public string pathToImages = "";
	public string imagePrefix = "";
	public string numberingFormat = "0000";
	public bool objMode = false;

	public int imageWidth = 1;
	public int imageHeight = 1;
	public int imageDepth = 1;
	public Vector3 voxelSize = new Vector3(1,1,1);

	private int sizeLimit = 500;
	private float numAtlases = 8.0f;

	public GameObject cube;
	private Material rayMarchMaterial;


	void Awake() {
		#if UNITY_EDITOR
			offlineMode = true;
			variables.offlineMode = true;
			variables.fpbJSON = new FpbJSON(offlineMode);
		#else
			offlineMode = false; // Just in case!
			print("Setting image variables...");
			setVariables();
			print("Finished setting variables method.");
		#endif
	}

	void Start () {
		qualityButton.SetActive (false);
		rayMarchMaterial = cube.GetComponent<Renderer> ().material;

		if (!objMode) {
			if (!atlasMode) {
				print ("Loading by image slices");
				infoText.text = "Downloading image slices...";
				StartCoroutine (loadBySlices ());
			} else {
				print ("Loading atlases directly");
				infoText.text = "Downloading texture maps...";
				for (int i = 0; i < numAtlases; i++) {
					StartCoroutine (loadByAtlas (i));
				}
			}
		} else {
			print ("Loading surface model");
			infoText.text = "Downloading surface model...";
			StartCoroutine (loadSurface ());
		}
	}

	IEnumerator loadSurface(){
		string surfacePath = pathToImages + imagePrefix + ".obj";
		WWW wwS = new WWW (surfacePath);

		while (!wwS.isDone) {
			float downloadProgress = wwS.progress;
			infoText.text = "Downloaded " + (downloadProgress).ToString ("P1"); 
			yield return null;
		}

		yield return wwS;
		GameObject surfaceModel = null;

		if (string.IsNullOrEmpty (wwS.error)) {
			//surfaceModel = OBJLoader.LoadOBJFile("Surface model", wwS.text);
		} else {
			FastObjImporter.Instance.ImportFromString (File.ReadAllText(surfacePath));
		}
	}

	IEnumerator loadBySlices(){
		//// THIS ALL NEEDS UPDATING. UNSUPPORTED FOR NOW. 
		// First, check that variables.slices is null. If not, we don't have
		// to download the images again, and can skip straight to loading the scene. 
			// load the first image, to determine sizes. 
		bool pngMode = true;

		// First, calculate image size from first texture
		Texture2D texture0 = new Texture2D (4, 4);
		int ii = 0;
		string loadImage0 = pathToImages + imagePrefix + ii.ToString (numberingFormat) + ".png";
		if (!offlineMode) { 
			WWW ww0 = new WWW (loadImage0);
			yield return ww0;

			if (!string.IsNullOrEmpty(ww0.error)){
				pngMode = false;
				loadImage0 = pathToImages + imagePrefix + ii.ToString (numberingFormat) + ".jpg";
				ww0 = new WWW (loadImage0);
				yield return ww0;
			}

			ww0.LoadImageIntoTexture (texture0);

		} else {
			byte[] fileData = File.ReadAllBytes (loadImage0);
			texture0.LoadImage (fileData);
		}

		int texWidth = texture0.width;
		int texHeight = texture0.height;

		// Make sure we're within size limits
		imageWidth = texWidth > sizeLimit ? sizeLimit : texWidth;
		imageHeight = texHeight > sizeLimit ? sizeLimit : texHeight;

		// Calcualte atlas size
		float paddedImageWidth = (float)ceil2 ((uint)imageWidth);
		float paddedImageHeight = (float)ceil2 ((uint)imageHeight);
		float slicesPerAtlas = Mathf.Ceil (imageDepth / numAtlases);

		int atlasWidth = (int) ceil2 ((uint)paddedImageWidth);
		int atlasHeight = (int) ceil2 ((uint)(paddedImageHeight * slicesPerAtlas));

		while ((atlasHeight > 2*atlasWidth) && (atlasHeight > paddedImageHeight)){
			atlasHeight /= 2;
			atlasWidth *= 2;
		}

		// Create array of atlas textures
		Color32 black = new Color32 (0, 0, 0, 255);

		Texture2D[] atlasArray = new Texture2D[(int)numAtlases];
		for (int i = 0; i < (int)numAtlases; i++) {
			atlasArray[i] = new Texture2D (atlasWidth, atlasHeight, TextureFormat.ARGB32, false);
		}

		// Set all pixels in the atlases to be clear
		Color32[] bigClearArray = atlasArray[0].GetPixels32 ();
		for (int i = 0; i < bigClearArray.Length; i++)
			bigClearArray [i] = black;

		for (int i = 0; i < (int)numAtlases; i++) {
			atlasArray[i].SetPixels32(bigClearArray);
		}


		// Set up some variables for atlas filling
		int xOffset = Mathf.FloorToInt(((float)paddedImageWidth-(float)texWidth)/2.0f);
		int yOffset = Mathf.FloorToInt(((float)paddedImageHeight-(float)texHeight)/2.0f);

		float slicesPerRow = Mathf.Floor((float)atlasWidth / (float)paddedImageWidth);

		// This loop does the actual downloading and filling of the atlases
		for (int i = 0; i < imageDepth; i++) {

			int atlasNumber = (int) (((float)i) % numAtlases);
			float locationIndex = Mathf.Floor(((float)i)/numAtlases);

			Texture2D downloadedImage = new Texture2D (imageWidth, imageHeight);
			infoText.text = "Loading slice " + (i+1).ToString () + " of " + imageDepth + ".";

			string imageToLoad = pathToImages + imagePrefix + i.ToString (numberingFormat) + ".png";
			if (!offlineMode) {
				if (!pngMode)
					imageToLoad = pathToImages + imagePrefix + i.ToString (numberingFormat) + ".jpg";
				WWW www = new WWW (imageToLoad);
				yield return www;
				www.LoadImageIntoTexture (downloadedImage);
				//yield return new WaitForSeconds (0.025f);
			} else {
				byte[] fileData = File.ReadAllBytes (imageToLoad);
				downloadedImage.LoadImage (fileData);
				yield return null;
			}

			if (texWidth > sizeLimit || texHeight > sizeLimit) {
				TextureScaler.scale (downloadedImage, imageWidth, imageHeight);
			}

			float xCoord = (locationIndex % slicesPerRow) * paddedImageWidth;
			xCoord += xOffset;
			float yCoord = Mathf.Floor(locationIndex / slicesPerRow) * paddedImageHeight;
			yCoord += yOffset;

			atlasArray[atlasNumber].SetPixels ((int)xCoord, (int)yCoord, texWidth, texHeight, downloadedImage.GetPixels (0, 0, texWidth, texHeight));
		}

		// Apply pixels and send to GPU
		infoText.text = "Preparing volumetric renderer...";
		for (int i = 0; i < (int)numAtlases; i++) {
			atlasArray [i].Apply ();
			rayMarchMaterial.SetTexture ("_Atlas" + i, atlasArray [i]);
		}

		// Now set the material and rendering properties
		rayMarchMaterial.SetFloat("_atlasWidth", atlasWidth);
		rayMarchMaterial.SetFloat ("_atlasHeight", atlasHeight);
		rayMarchMaterial.SetFloat ("_imageDepth", imageDepth);
		rayMarchMaterial.SetFloat ("_imageWidth", paddedImageWidth);
		rayMarchMaterial.SetFloat ("_imageHeight", paddedImageHeight);
		rayMarchMaterial.SetFloat ("_slicesPerAtlas", slicesPerAtlas);
		rayMarchMaterial.SetFloat ("_slicesPerRow", slicesPerRow);

		Vector3 cubeSize = new Vector3 (imageWidth * voxelSize.x, imageHeight * voxelSize.y, imageDepth * voxelSize.z).normalized;
		cubeSize *= 3.5f * Mathf.Min (1.0f / cubeSize.x, Mathf.Min (1.0f / cubeSize.y, 1.0f / cubeSize.z));
		cube.transform.localScale = cubeSize;

		// Load the scene
		infoText.text = "Click to start";
		variables.freezeAll = false;
		cube.SetActive (true);
		qualityButton.SetActive (true);
		variables.triggerRender = true;
		variables.volumeReadyState = 1;
	}

	// LOAD BY ATLAS
	private int atlasesLoaded = 0;
	private float[] downloadProgress = new float[8];
	IEnumerator loadByAtlas(int atlasNumber){
		float atlasWidth = 1; 

		//infoText.text = "Downloading texture map " + (atlasNumber+1) + " of " + (int)numAtlases + ".";

		string atlasToLoad = pathToImages + imagePrefix + atlasNumber.ToString (numberingFormat) + ".png";
		Texture2D atlasSlice = new Texture2D (4, 4, TextureFormat.ARGB32, false);

		WWW www = new WWW (atlasToLoad);

		while (!www.isDone) {
			downloadProgress [atlasNumber] = www.progress;
			infoText.text = "Downloaded " + (sum (downloadProgress) /numAtlases).ToString("P1"); 
			yield return null;
		}

		yield return www;

		// Load image into atlas
		if (string.IsNullOrEmpty (www.error)) {
			www.LoadImageIntoTexture (atlasSlice);
		} else {
			byte[] fileData = File.ReadAllBytes (atlasToLoad);
			atlasSlice.LoadImage (fileData);
		}

		// Set atlas to material
		rayMarchMaterial.SetTexture("_Atlas"+atlasNumber, atlasSlice);


		atlasesLoaded++;
		// Calculate a few more useful variables
		if (atlasesLoaded == (int)numAtlases) {
			atlasWidth = atlasSlice.width;
			rayMarchMaterial.SetFloat("_atlasWidth", atlasWidth);
			rayMarchMaterial.SetFloat ("_atlasHeight", atlasSlice.height);

			float paddedImageWidth = (float)ceil2 ((uint)imageWidth);
			float paddedImageHeight = (float)ceil2 ((uint)imageHeight);

			float slicesPerRow = Mathf.Floor (atlasWidth / paddedImageWidth);
			float slicesPerAtlas = Mathf.Ceil (imageDepth / numAtlases);

			// Set other material properties
			rayMarchMaterial.SetFloat ("_imageDepth", imageDepth);
			rayMarchMaterial.SetFloat ("_imageWidth", paddedImageWidth);
			rayMarchMaterial.SetFloat ("_imageHeight", paddedImageHeight);
			rayMarchMaterial.SetFloat ("_slicesPerAtlas", slicesPerAtlas);
			rayMarchMaterial.SetFloat ("_slicesPerRow", slicesPerRow);


			Vector3 cubeSize = new Vector3 (imageWidth * voxelSize.x, imageHeight * voxelSize.y, imageDepth * voxelSize.z).normalized;
			cubeSize *= 3.5f * Mathf.Min (1.0f / cubeSize.x, Mathf.Min (1.0f / cubeSize.y, 1.0f / cubeSize.z));

			cube.transform.localScale = cubeSize;

			// Load the scene
			infoText.text = "Click to start";
			variables.freezeAll = false;
			cube.SetActive (true);
			qualityButton.SetActive (true);
			variables.triggerRender = true;
			variables.volumeReadyState = 1;
		}
	}

	void Update(){
		if (variables.volumeReadyState == 1){
			if (Input.anyKeyDown || Input.touchCount > 0) {
				if (changingQuality && Input.GetKey (KeyCode.Mouse0)) {
					infoBox.SetActive (false);
					// Then open the quality changer
					GameObject.Find("Arrow Left").GetComponent<leftArrowControl>().arrowClicked();
				} else {
					infoBox.SetActive (false);
				}
				// Off you go! 
				variables.volumeReadyState = 2;
			}
		}
	}

	public void setChangingQuality(bool mouseIn){
		changingQuality = mouseIn;
	}

	// Set variables for online mode
	private void setVariables(){
		Application.ExternalEval ("fpcanvas.SendMessage('Main Camera', 'parseFpbJSON', JSON.stringify(fpb));");

		atlasMode = variables.fpbJSON.getAtlasMode();
		pathToImages = variables.fpbJSON.pathToImages;
		if (pathToImages.Substring (pathToImages.Length - 1) != "/") {
			pathToImages = pathToImages + "/";
		}


		imagePrefix = variables.fpbJSON.imagePrefix;
		numberingFormat = variables.fpbJSON.numberingFormat;

		imageWidth = variables.fpbJSON.sliceWidth;
		imageHeight = variables.fpbJSON.sliceHeight;
		imageDepth = variables.fpbJSON.numberOfImages;
		voxelSize = variables.fpbJSON.voxelSize; // this one might not work so well... 
	}

	public void parseFpbJSON(string jsonString){
		variables.fpbJSON = JsonUtility.FromJson<FpbJSON> (jsonString);
	}

	//  Check if there is a bookmark in the URL
	public void setLoadFromBookmark(string boolean){
		variables.loadBookmarkFromURL = (boolean=="true") ? true : false;
	}

	// Some efficient power-of-two rounders:
	private uint ceil2(uint x) {
		x--;
		x |= (x >> 1);
		x |= (x >> 2);
		x |= (x >> 4);
		x |= (x >> 8);
		x |= (x >> 16);
		x |= (x >> 32);
		x |= (x >> 64);
		x |= (x >> 128);
		x |= (x >> 256);
		x |= (x >> 512);
		x |= (x >> 1024);
		x |= (x >> 2048);
		x |= (x >> 4096);
		x |= (x >> 8192);
		x |= (x >> 16384);
		x |= (x >> 32768);
		x |= (x >> 65536);
		x |= (x >> 131072);
		x |= (x >> 262144);
		return (x+1);
	}

	// Just a useful sum helper-function 
	private float sum(float[] arrayToSum){
		float output = 0.0f;
		for (int i = 0; i < arrayToSum.Length; i++) {
			output += arrayToSum [i];
		}
		return output;
	}

}
