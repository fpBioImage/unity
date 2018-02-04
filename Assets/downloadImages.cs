using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class downloadImages : MonoBehaviour {

	public bool volumeReady = false;
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

		if (!atlasMode) {
			print ("Loading by image slices");
			infoText.text = "Downloading image slices...";
			//StartCoroutine (loadBySlices ());
		} else {
			print ("Loading atlases directly");
			infoText.text = "Downloading texture maps...";
			StartCoroutine (loadByAtlas ());
		}
	}


	/*IEnumerator loadBySlices(){
		//// THIS ALL NEEDS UPDATING. UNSUPPORTED FOR NOW. 
		// First, check that variables.slices is null. If not, we don't have
		// to download the images again, and can skip straight to loading the scene. 
		if (variables.atlasArray[0] == null) {
			// load the first image, to determine sizes. 
			bool pngMode = true;
			Debug.Log("Loading slices individually");

			Texture2D texture0 = new Texture2D (4, 4);
			int ii = 0;
			if (!offlineMode) {
				string loadImage0 = variables.pathToImages + variables.imagePrefix + ii.ToString (variables.numberingFormat) + ".png";
				WWW ww0 = new WWW (loadImage0);
				yield return ww0;

				if (!string.IsNullOrEmpty(ww0.error)){
					pngMode = false;
					loadImage0 = variables.pathToImages + variables.imagePrefix + ii.ToString (variables.numberingFormat) + ".jpg";
					ww0 = new WWW (loadImage0);
					yield return ww0;
				}

				ww0.LoadImageIntoTexture (texture0);

			} else {
				string loadImage0 = variables.pathToImages + variables.imagePrefix + ii.ToString (variables.numberingFormat);
				texture0 = Resources.Load<Texture2D>(loadImage0);
			}

			int t0width = texture0.width;
			int t0height = texture0.height;

			// Make sure we're within size limits
			int texWidth = t0width > sizeLimit ? sizeLimit : t0width;
			int texHeight = t0height > sizeLimit ? sizeLimit : t0height;
			int numImages = variables.numberOfImages > sizeLimit ? sizeLimit : variables.numberOfImages;

			// Adjust voxel size to our size-limit scaling
			variables.voxelSize [0] *= (float)t0width / (float)texWidth;
			variables.voxelSize [1] *= (float)t0height / (float)texHeight;
			variables.voxelSize [2] *= (float)variables.numberOfImages / (float)numImages;

			// Calculate power-of-2 sizes for images
			int paddedSliceWidth = (int)ceil2((uint)texWidth);
			int paddedSliceHeight = (int)ceil2((uint)texHeight);
			int depthPadding = 4; // padding in depth helps stop strange rendering effects on some graphcis cards. 
			int paddedSliceDepth = numImages + depthPadding;

			int xOffset = Mathf.FloorToInt(((float)paddedSliceWidth-(float)texWidth)/2.0f);
			int yOffset = Mathf.FloorToInt(((float)paddedSliceHeight-(float)texHeight)/2.0f);

			// Set up 4 2D atlases with a clear black background, to fill with the PNGs. 
			int atlasWidth;
			int atlasHeight;

			float slicesPerAtlas = Mathf.Ceil (paddedSliceDepth / numAtlases);
			atlasWidth = (int) ceil2 ((uint)paddedSliceWidth);
			atlasHeight = (int) ceil2 ((uint)(paddedSliceHeight * slicesPerAtlas));

			while ((atlasHeight > 2*atlasWidth) && (atlasHeight > paddedSliceHeight)){
				atlasHeight /= 2;
				atlasWidth *= 2;
			}

			Color32 black = new Color32 (0, 0, 0, 0);

			for (int i = 0; i < (int)numAtlases; i++) {
				variables.atlasArray[i] = new Texture2D (atlasWidth, atlasHeight, TextureFormat.ARGB32, false);
			}

			Color32[] bigClearArray = variables.atlasArray[0].GetPixels32 ();

			for (int i = 0; i < bigClearArray.Length; i++)
				bigClearArray [i] = black;

			for (int i = 0; i < (int)numAtlases; i++) {
				variables.atlasArray[i].SetPixels32(bigClearArray);
			}

			// Set up some variable for atlas filling
			float xCoord;
			float yCoord;

			float slicesPerRow = Mathf.Floor((float)atlasWidth / (float)paddedSliceWidth);
			float texturesPerSlice = Mathf.Ceil ((float)paddedSliceDepth / numAtlases);

			// This loop does the actual downloading and filling of the atlases
			for (int i = 0; i < numImages; i++) {
				int j = i + Mathf.FloorToInt((float)depthPadding/2.0f);
				int k = Mathf.RoundToInt (i * (float)variables.numberOfImages / (float)numImages);

				int atlasNumber = (int) (((float)j) % numAtlases);
				float locationIndex = Mathf.Floor(((float)j)/numAtlases);

				Texture2D downloadedImage = new Texture2D (t0width, t0width);
				infoText.text = "Loading slice " + (i+1).ToString () + " of " + numImages + ".";

				if (!offlineMode) {
					string imageToLoad;
					if (pngMode) {
						imageToLoad = variables.pathToImages + variables.imagePrefix + k.ToString (variables.numberingFormat) + ".png";
					} else {
						imageToLoad = variables.pathToImages + variables.imagePrefix + k.ToString (variables.numberingFormat) + ".jpg";
					}
					WWW www = new WWW (imageToLoad);
					yield return www;
					www.LoadImageIntoTexture (downloadedImage);
				} else {
					string imageToLoad = variables.pathToImages + variables.imagePrefix + k.ToString (variables.numberingFormat);
					downloadedImage = Resources.Load<Texture2D>(imageToLoad);
				}

				if (t0width > sizeLimit || t0height > sizeLimit) {
					TextureScaler.scale (downloadedImage, texWidth, texHeight);
				}

				xCoord = (locationIndex % slicesPerRow) * paddedSliceWidth;
				xCoord += xOffset;
				yCoord = Mathf.Floor(locationIndex / slicesPerRow) * paddedSliceHeight;
				yCoord += yOffset;

				variables.atlasArray[atlasNumber].SetPixels ((int)xCoord, (int)yCoord, texWidth, texHeight, downloadedImage.GetPixels (0, 0, texWidth, texHeight));
			}
			for (int i = 0; i < (int)numAtlases; i++) {
				variables.atlasArray [i].Apply ();
			}

			// Set the global variables for the renderer
			variables.numSlices = paddedSliceDepth;
			variables.slicesPerRow = slicesPerRow;

			variables.texturesPerSlice = texturesPerSlice;

			variables.tPixelWidth = paddedSliceWidth;
			variables.tPixelHeight = paddedSliceHeight;
			variables.atlasWidth = atlasWidth;
			variables.atlasHeight = atlasHeight;
			variables.cubeSize = new Vector3 ((float)paddedSliceWidth * voxelSize.x, (float)paddedSliceHeight * voxelSize.y, (float)paddedSliceDepth * voxelSize.z);

			variables.cubeScale = 3.5f * Mathf.Max ((float)paddedSliceWidth / (float)texWidth, Mathf.Max ((float)paddedSliceHeight / (float)texHeight, (float)paddedSliceDepth / (float)numImages));
		}

		// Load the scene
		infoText.text = "Preparing volumetric renderer...";
		UnityEngine.SceneManagement.SceneManager.LoadScene ("main");
	}*/

	IEnumerator loadByAtlas(){
		float atlasWidth = 1; 

		for (int atlasNumber = 0; atlasNumber < numAtlases; atlasNumber++) {
			infoText.text = "Downloading texture map " + (atlasNumber+1) + " of " + (int)numAtlases + ".";

			string atlasToLoad = pathToImages + imagePrefix + atlasNumber.ToString (numberingFormat) + ".png";
			Texture2D atlasSlice = new Texture2D (4, 4, TextureFormat.ARGB32, false);

			WWW www = new WWW (atlasToLoad);
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

			if (atlasNumber == 0){
				atlasWidth = atlasSlice.width;
				rayMarchMaterial.SetFloat("_atlasWidth", atlasWidth);
				rayMarchMaterial.SetFloat ("_atlasHeight", atlasSlice.height);
			}

		}

		// Calculate a few more useful variables
		float paddedImageWidth = (float) ceil2((uint) imageWidth);
		float paddedImageHeight = (float) ceil2 ((uint) imageHeight);

		float slicesPerRow = Mathf.Floor(atlasWidth / paddedImageWidth);
		float slicesPerAtlas = Mathf.Ceil (imageDepth / numAtlases);

		// Set other material properties
		rayMarchMaterial.SetFloat ("_imageDepth", imageDepth);
		rayMarchMaterial.SetFloat ("_imageWidth", paddedImageWidth);
		rayMarchMaterial.SetFloat ("_imageHeight", paddedImageHeight);
		rayMarchMaterial.SetFloat ("_slicesPerAtlas", slicesPerAtlas);
		rayMarchMaterial.SetFloat ("_slicesPerRow", slicesPerRow);


		Vector3 cubeSize = new Vector3 (imageWidth * voxelSize.x, imageHeight * voxelSize.y, imageDepth * voxelSize.z).normalized;
		cubeSize *= 3.5f * Mathf.Min (1.0f/cubeSize.x, Mathf.Min (1.0f/cubeSize.y, 1.0f/cubeSize.z));

		cube.transform.localScale = cubeSize;

		// Load the scene
		infoText.text = "Click to start";
		cube.SetActive (true);
		qualityButton.SetActive (true);
		volumeReady = true;
	}

	void Update(){
		if (volumeReady){
			if (Input.anyKeyDown) {
				if (changingQuality && Input.GetKey (KeyCode.Mouse0)) {
					infoBox.SetActive (false);
					// AND THEN OPEN quality changer
					qualityBox.SetActive(true);
				} else {
					infoBox.SetActive (false);
				}
				// Off you go! 
				volumeReady = false;
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
		print ("Set Atlas Mode as " + atlasMode);
		pathToImages = variables.fpbJSON.pathToImages;
		if (pathToImages.Substring (pathToImages.Length - 1) != "/") {
			pathToImages = pathToImages + "/";
		}
		print ("Set path to images as " + pathToImages);
		

		imagePrefix = variables.fpbJSON.imagePrefix;
		numberingFormat = variables.fpbJSON.numberingFormat;

		imageWidth = variables.fpbJSON.sliceWidth;
		imageHeight = variables.fpbJSON.sliceHeight;
		imageDepth = variables.fpbJSON.numberOfImages;
		print ("Set image depth as " + imageDepth);
		voxelSize = variables.fpbJSON.voxelSize; // this one might not work so well... 
		print("Set voxel size as x:" + voxelSize.x + ", y:" + voxelSize.y + ", z:" + voxelSize.z);

		/*
		string evalInputs = "if(fpb.uniqueName==undefined){fpb.uniqueName='defaultName'};fpcanvas.SendMessage('Main Camera', 'setUniqueName', fpb.uniqueName);" +
			"if(fpb.numberOfImages==undefined){fpb.numberOfImages=0};fpcanvas.SendMessage('Main Camera', 'setNumberOfImages', fpb.numberOfImages);" +
			"if(fpb.imagePrefix==undefined){fpb.imagePrefix='not-found'};fpcanvas.SendMessage('Main Camera', 'setImagePrefix', fpb.imagePrefix);" +
			"if(fpb.numberingFormat==undefined){fpb.numberingFormat='0000'};fpcanvas.SendMessage('Main Camera', 'setNumberingFormat', fpb.numberingFormat);" +
			"if(fpb.pathToImages==undefined){fpb.pathToImages='not-found'};fpcanvas.SendMessage('Main Camera', 'setPathToImages', fpb.pathToImages);" +
			"if(fpb.voxelSize.x==undefined){fpb.voxelSize.x=1};fpcanvas.SendMessage('Main Camera', 'setVoxelSizeX', fpb.voxelSize.x);" +
			"if(fpb.voxelSize.y==undefined){fpb.voxelSize.y=1};fpcanvas.SendMessage('Main Camera', 'setVoxelSizeY', fpb.voxelSize.y);" +
			"if(fpb.voxelSize.z==undefined){fpb.voxelSize.z=1};fpcanvas.SendMessage('Main Camera', 'setVoxelSizeZ', fpb.voxelSize.z);" + 
			"if(fpb.atlasMode==undefined){fpb.atlasMode='false'};fpcanvas.SendMessage('Main Camera', 'setAtlasMode', fpb.atlasMode);" + 
			"if(fpb.sliceWidth==undefined){fpb.sliceWidth=100};fpcanvas.SendMessage('Main Camera', 'setSliceWidth', fpb.sliceWidth);" +
			"if(fpb.sliceHeight==undefined){fpb.sliceHeight=100};fpcanvas.SendMessage('Main Camera', 'setSliceHeight', fpb.sliceHeight);" + 
			"if(fpb.imageAlpha==undefined){fpb.imageAlpha='false'};fpcanvas.SendMessage('Main Camera', 'setImageAlpha', fpb.imageAlpha);";

		Application.ExternalEval (evalInputs);

		// This works out if we need to load a bookmark from the URL:
		string evalURL = "if(window.location.href.indexOf('b=') > -1){" +
			"fpcanvas.SendMessage('Main Camera', 'setLoadFromBookmark', 'true');}";

		Application.ExternalEval (evalURL);

		if (variables.loadBookmarkFromURL) {
			Application.ExternalEval("function getParameterByName(name, url) {" +
				"if (!url) url = window.location.href;" +
				"name = name.replace(/[\\[\\]]/g, '\\\\$&');" +
				"var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)')," +
				"results = regex.exec(url);" +
				"if (!results) return null;" +
				"if (!results[2]) return '';" +
				"return decodeURIComponent(results[2].replace(/\\+/g, ' ')); }" +
				"fpcanvas.SendMessage('Main Camera', 'setURLbookmarkString', getParameterByName('b'));" );
		}

		// The following variables are optional inputs
		string evalRendering = "if(fpb.opacity){fpcanvas.SendMessage('Main Camera', 'setOpacity', fpb.opacity)};" +
			"if(fpb.threshold){fpcanvas.SendMessage('Main Camera', 'setThreshold', fpb.threshold);}" +
			"if(fpb.intensity){fpcanvas.SendMessage('Main Camera', 'setIntensity', fpb.intensity)};";

		Application.ExternalEval (evalRendering);
		*/

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

}
