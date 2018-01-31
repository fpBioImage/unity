using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class fpbRendering : MonoBehaviour {
	// Input variables, can be set in the Unity editor. 
	[SerializeField]
	[Header("Render in a lower resolution to increase performance.")]
	private int downscale = 2;
	[SerializeField]
	private LayerMask volumeLayer;

	[Header("Colouring")]
	[SerializeField]
	private float opacitySpeed = 0.8f;
	[SerializeField]
	private float thresholdSpeed = 0.07f;
	[SerializeField]
	private float intensitySpeed = 0.2f;

	[SerializeField][Range(0, 1)]
	public float opacity = 1.0f;
	[SerializeField][Range(0, 1)]
	public float threshold = 0.1f;
	[SerializeField][Range(0, 5.0f)]
	public float intensity = 0.75f;

	[Header("Touch input")]
	[SerializeField]
	private float opacityTouchSpeed = 80f;
	[SerializeField]
	private float thresholdTouchSpeed = 7f;
	[SerializeField]
	private float intensityTouchSpeed = 20f;

	[Header("GUI sliders")]
	[SerializeField]
	private Slider opacitySlider;
	[SerializeField]
	private Slider thresholdSlider;
	[SerializeField]
	private Slider intensitySlider;
	[SerializeField]
	private Dropdown renderingMode; // should change this to rendering mode

	private Material _rayMarchMaterial;
	private bool updateRender = true;
	private float updateTime;

	private bool takeScreenShot = false;

	private int rendering = 1;
	private float zSteps;

	[Header("Res input")]
	[SerializeField]
	private Text wRes;
	[SerializeField]
	private Text hRes;

	private bool hiRes;

	public void takeShot(){
		hiRes = false;
		takeScreenShot = true;
	}

	public void hiResShot(){
		hiRes = true;
		takeScreenShot = true;
	}

	public void setFreezeAll(bool freezeAll){
		variables.setFreezeAll (freezeAll);
	}

	private void Update(){
		// Pre-update
		if (Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl) ||
			Input.GetKeyDown (KeyCode.LeftAlt) || Input.GetKeyDown (KeyCode.RightAlt) ||
			Input.GetKeyDown (KeyCode.LeftApple) || Input.GetKeyDown (KeyCode.RightApple) ||
			Input.GetKeyDown (KeyCode.LeftCommand) || Input.GetKeyDown (KeyCode.RightCommand) ||
			Input.GetKeyDown (KeyCode.LeftWindows) || Input.GetKeyDown (KeyCode.RightWindows) ||
			Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
			variables.setFreezeAll (true);
		}
		if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl) ||
			Input.GetKeyUp (KeyCode.LeftAlt) || Input.GetKeyUp (KeyCode.RightAlt) ||
			Input.GetKeyUp (KeyCode.LeftApple) || Input.GetKeyUp (KeyCode.RightApple) ||
			Input.GetKeyUp (KeyCode.LeftCommand) || Input.GetKeyUp (KeyCode.RightCommand) ||
			Input.GetKeyUp (KeyCode.LeftWindows) || Input.GetKeyUp (KeyCode.RightWindows) ||
			Input.GetKeyUp (KeyCode.LeftShift) || Input.GetKeyUp (KeyCode.RightShift)) {
			variables.setFreezeAll (false);
		} 	

		if (!variables.getFreezeAll ()) {
			opacity = opacitySlider.value * opacitySlider.value;
			opacity += Input.GetAxis("OpacityAxis") * opacitySpeed * Time.deltaTime * opacity;
			//opacity = clamp(opacity);
			opacitySlider.value = Mathf.Sqrt(opacity);

			threshold = thresholdSlider.value;
			threshold += Input.GetAxis ("ThresholdAxis") * thresholdSpeed * Time.deltaTime;
			//threshold = clamp(threshold);
			thresholdSlider.value = threshold;

			// would like to make the intensity slider logarithmic. Maybe quadratic is easiest. 
			intensity = intensitySlider.value * intensitySlider.value;
			intensity += Input.GetAxis ("IntensityAxis") * intensitySpeed * Time.deltaTime * intensity;
			//intensity = clamp(intensity, 0.0f, 5.0f);
			intensitySlider.value = Mathf.Sqrt (intensity);

			if (Input.GetKeyUp (KeyCode.N)) {
				variables.sectionMode = !variables.sectionMode;
			}

			if (Input.GetKeyUp (KeyCode.Z)) {
				variables.sectionMode = false;
			}

			if (Input.touchCount == 3) {
				// Store all touches
				Touch touch0 = Input.GetTouch (0);
				Touch touch1 = Input.GetTouch (1);
				Touch touch2 = Input.GetTouch (2);

				// Calculate average x/y movement for intensity and opacity axes (respectively)
				float xMove = (touch0.deltaPosition.x + touch1.deltaPosition.x + touch2.deltaPosition.x) / Screen.width; 
				float yMove = (touch0.deltaPosition.y + touch1.deltaPosition.y + touch2.deltaPosition.y) / Screen.width; 

				// Change opacity and intensity
				opacity += xMove * opacityTouchSpeed * opacity;
				//opacity = clamp (opacity, 0.01f, 1.0f);
				intensity += yMove * intensityTouchSpeed * intensity;
				//intensity = clamp (intensity, 0.01f, 5.0f);

				// Find the position in the previous frame of each touch.
				Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
				Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
				Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag01 = (touch0PrevPos - touch1PrevPos).magnitude;
				float touchDeltaMag01 = (touch0.position - touch1.position).magnitude;
				float deltaMagnitudeDiff01 = prevTouchDeltaMag01 - touchDeltaMag01;

				float prevTouchDeltaMag02 = (touch0PrevPos - touch2PrevPos).magnitude;
				float touchDeltaMag02 = (touch0.position - touch2.position).magnitude;
				float deltaMagnitudeDiff02 = prevTouchDeltaMag02 - touchDeltaMag02;

				float prevTouchDeltaMag12 = (touch1PrevPos - touch2PrevPos).magnitude;
				float touchDeltaMag12 = (touch1.position - touch2.position).magnitude;
				float deltaMagnitudeDiff12 = prevTouchDeltaMag12 - touchDeltaMag12;

				// Get the maximum pinch of the 3 fingers
				float maxDeltaMagDiff = Mathf.Max (deltaMagnitudeDiff01, Mathf.Max (deltaMagnitudeDiff02, deltaMagnitudeDiff12))/Screen.width;

				// Change threshold based on 3-finger pinch
				threshold += maxDeltaMagDiff * thresholdTouchSpeed;
				threshold = clamp (threshold);
			}

			// Screen shots
			if (Input.GetKeyUp (KeyCode.V)) {
				takeShot ();
			}

			if (Input.GetKeyUp (KeyCode.C)) {
				hiResShot ();
			}

		}

		if (takeScreenShot) {
			float oldZ = zSteps;
			int oldD = downscale;

			int camWidth;
			int camHeight;

			if (hiRes) {
				zSteps = 512.0f;
				downscale = 1;

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
			Application.ExternalCall ("download", "data:image/png;base64," + System.Convert.ToBase64String (bytes), "Screenshot " + System.DateTime.Now.ToString ("yyyy-MM-dd") + " at " + System.DateTime.Now.ToString ("HH.mm.ss") + ".png");

			#if UNITY_EDITOR
			File.WriteAllBytes(Application.dataPath + "/SavedScreen.png", bytes);
			#endif

			zSteps = oldZ; downscale = oldD;
			takeScreenShot = false;
		}

		// Continue updating renderer for 1s after any keypress
		if (Input.GetAxis ("ClipAxisX")!=0 || Input.GetAxis ("ClipAxisY")!=0 || Input.GetAxis ("ClipAxisZ")!=0) {
			updateTime = Time.time + 3;
		} else if (Input.anyKey || (!variables.getFreezeMouse() && (Input.GetAxis("Mouse X")!=0 || Input.GetAxis("Mouse Y")!=0) )) {
			updateTime = Time.time;
		}
		updateRender = Time.time - updateTime < 1;
	}

	private void Start()
	{
		// set default rendering parameters. If necessary.
		_rayMarchMaterial = GetComponent<Renderer> ().material;

		if (variables.getViewMemory () == "") {
			if (!variables.offlineMode) {
				GetComponent<bookmarker> ().setDefaultView ();
				opacity = variables.defaultOpacity;
				intensity = variables.defaultIntensity;
				threshold = variables.defaultThreshold;
			} else {
				opacity = variables.defaultOpacity;
				intensity = variables.defaultIntensity;
				threshold = variables.defaultThreshold;
			}
		} else {
			GetComponent<bookmarker> ().loadBookmarkFromString (variables.getViewMemory ());
		}
			
		opacitySlider.value = Mathf.Sqrt(opacity);
		thresholdSlider.value = threshold;
		intensitySlider.value = Mathf.Sqrt(intensity);

		variables.sectionMode = false;

		updateTime = Time.time;
		GenerateVolumeTexture();
	}

	public void updateRenderingMode(int newRendering){
		print ("new rendering value is " + newRendering);
		_rayMarchMaterial.SetFloat(_renderID, (float)newRendering);
	}

	// More input variables
	[Header("Game Objects")]
	[SerializeField]
	private Transform clipPlane;
	[SerializeField]
	private Transform backClipPlane;
	[SerializeField]
	private Transform cubeTarget;

	//private void OnRenderImage(RenderTexture source, RenderTexture destination)
	private void LateUpdate()
	{
		if (updateRender) {
			//var width = source.width / downscale;
			//var height = source.height / downscale;

			if (cubeTarget != null && clipPlane != null && clipPlane.gameObject.activeSelf) {
				var p = new Plane (
					cubeTarget.InverseTransformDirection (clipPlane.forward),
					cubeTarget.InverseTransformPoint (clipPlane.position));
				Vector3 scaledPlane = new Vector3 (p.normal.x * cubeTarget.localScale.x, p.normal.y * cubeTarget.localScale.y, p.normal.z * cubeTarget.localScale.z).normalized;

				var p2 = new Plane (
					cubeTarget.InverseTransformDirection (backClipPlane.forward),
					cubeTarget.InverseTransformPoint (backClipPlane.position));
				Vector3 scaledBackPlane = new Vector3 (p2.normal.x * cubeTarget.localScale.x, p2.normal.y * cubeTarget.localScale.y, p2.normal.z * cubeTarget.localScale.z).normalized;


				_rayMarchMaterial.SetVector ("_ClipPlane", new Vector4 (scaledPlane.x, scaledPlane.y, scaledPlane.z, p.distance));

				if (variables.sectionMode) {
					_rayMarchMaterial.SetVector ("_ClipPlane2", new Vector4 (scaledBackPlane.x, scaledBackPlane.y, scaledBackPlane.z, p2.distance));
				} else {
					_rayMarchMaterial.SetVector ("_ClipPlane2", new Vector4 (0.0f, 0.0f, 0.0f, 50.0f));
				}
			} else {
				_rayMarchMaterial.SetVector ("_ClipPlane", Vector4.zero);
			}
				

			_rayMarchMaterial.SetFloat (_opacityID, opacity); // Blending strength 
			_rayMarchMaterial.SetFloat (_thresholdID, threshold); // alpha cutoff value
			_rayMarchMaterial.SetFloat (_intensityID, intensity); // blends image a bit better

		}
	}

	private int _opacityID;
	private int _thresholdID;
	private int _intensityID;
	private int _renderID;

	private void GenerateVolumeTexture()
	{
		// Get Property IDs
		_opacityID = Shader.PropertyToID("_Opacity");
		_intensityID = Shader.PropertyToID ("_Intensity");
		_thresholdID = Shader.PropertyToID ("_DataMin");
		_renderID = Shader.PropertyToID ("_RenderMode");
	}

	// Helper functions
	public float clamp (float input){
		input = clamp (input, 0.0f, 1.0f);
		return input;
	}

	public float clamp(float input, float limLow, float limHigh){
		if (input > limHigh)
			input = limHigh;
		if (input < limLow)
			input = limLow;
		return input;
	}


}
