using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

/*
 * This script runs in the 'loading' scene. It downloads the
 * PNG image stack and puts each slice into the correct 
 * location in the atlas texture. 
 * Note that using a 2D atlas was found
 * to be faster and more memory efficient than a Texture3D.
 * It also means that FPBioimage only requires webGL 1.0, and
 * not the experimental webGL 2.0.
 * You may notice everything is padded to the next highest power
 * of 2. This was to counteract rendering errors seen on some
 * graphics cards when non-power-of-2 texture sizes were used.
*/

public class TextureLoader : MonoBehaviour {

	public Text infoText;
	private int sizeLimit = 500;
	private float numAtlases = 8.0f;

	// Use this for initialization
	void Start () {
		if (!variables.loadAtlasDirectly) {
			infoText.text = "Downloading image slices...";
			StartCoroutine (loadBySlices ());
		} else {
			infoText.text = "Downloading texture maps...";
			StartCoroutine (loadByAtlas ());
		}
	}

	IEnumerator loadBySlices(){
		// First, check that variables.slices is null. If not, we don't have
		// to download the images again, and can skip straight to loading the scene. 
		if (variables.atlasArray[0] == null) {
			// load the first image, to determine sizes. 
			bool pngMode = true;
			Debug.Log("Loading slices individually");

			Texture2D texture0 = new Texture2D (4, 4);
			int ii = 0;
			if (!variables.offlineMode) {
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

				if (!variables.offlineMode) {
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
			variables.cubeSize = new Vector3 ((float)paddedSliceWidth * variables.voxelSize[0], (float)paddedSliceHeight * variables.voxelSize[1], (float)paddedSliceDepth * variables.voxelSize[2]);

			variables.cubeScale = 3.5f * Mathf.Max ((float)paddedSliceWidth / (float)texWidth, Mathf.Max ((float)paddedSliceHeight / (float)texHeight, (float)paddedSliceDepth / (float)numImages));
		}

		// Load the scene
		infoText.text = "Preparing volumetric renderer...";
		UnityEngine.SceneManagement.SceneManager.LoadScene ("main");
	}

	IEnumerator loadByAtlas(){
		Debug.Log("Loading atlas directly");

		for (int atlasNumber = 0; atlasNumber < numAtlases; atlasNumber++) {
			bool success = false;
			string atlasToLoad = variables.pathToImages + variables.imagePrefix + atlasNumber.ToString (variables.numberingFormat) + ".png";
			variables.atlasArray [atlasNumber] = new Texture2D (4, 4, TextureFormat.ARGB32, false);

			WWW www = new WWW (atlasToLoad);
			yield return www;

			// Load image into atlas
			if (string.IsNullOrEmpty (www.error)) {
				www.LoadImageIntoTexture (variables.atlasArray [atlasNumber]);
			} else {
				// This is likely the route taken for offline Unity testing
				// And it doesn't work offline at the moment! Whoops!
				byte[] fileData = File.ReadAllBytes (atlasToLoad);
				variables.atlasArray [atlasNumber].LoadImage (fileData);
			}
		
			infoText.text = "Downloaded texture map " + (atlasNumber+1) + " of " + (int)numAtlases + ".";
		}

		// Scaling seems to be broken now too.. not sure where it's gone wrong.. 
		variables.numSlices = variables.numberOfImages + 4; 
		variables.tPixelWidth = (float)ceil2((uint)variables.sliceWidth);
		variables.tPixelHeight = (float)ceil2((uint)variables.sliceHeight);
		variables.atlasWidth = variables.atlasArray[0].width;
		variables.atlasHeight = variables.atlasArray[0].height;

		variables.cubeSize = new Vector3 (variables.tPixelWidth * variables.voxelSize[0], variables.tPixelHeight * variables.voxelSize[1], variables.numSlices * variables.voxelSize[2]);
		variables.cubeScale = 3.5f * Mathf.Max (variables.tPixelWidth / variables.sliceWidth, Mathf.Max (variables.tPixelHeight / variables.sliceHeight, variables.numSlices / variables.numberOfImages));

		float slicesPerRow = Mathf.Floor(variables.atlasWidth / variables.tPixelWidth);
		float texturesPerSlice = Mathf.Ceil (variables.numSlices / numAtlases);

		// Set the global variables for the renderer
		variables.slicesPerRow = slicesPerRow;
		variables.texturesPerSlice = texturesPerSlice;

		// Load the scene
		infoText.text = "Preparing volumetric renderer...";
		UnityEngine.SceneManagement.SceneManager.LoadScene ("main");

	}

	void Awake() {
	}
	
	// Update is called once per frame
	void Update () {
		// Nothing to do: updating is handled by coroutine
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