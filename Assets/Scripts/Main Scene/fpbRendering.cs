using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class fpbRendering : MonoBehaviour {
	// Input variables, can be set in the Unity editor. 
	[Header("Keyboard input")]
	[SerializeField]
	private float opacitySpeed = 0.8f;
	[SerializeField]
	private float thresholdSpeed = 0.07f;
	[SerializeField]
	private float intensitySpeed = 0.2f;

	[Header("Touch input")]
	[SerializeField]
	private float opacityTouchSpeed = 80f;
	[SerializeField]
	private float thresholdTouchSpeed = 7f;
	[SerializeField]
	private float intensityTouchSpeed = 20f;

	[Header("Slider input")]
	[SerializeField]
	private Slider opacitySlider;
	[SerializeField]
	private Slider thresholdSlider;
	[SerializeField]
	private Slider intensitySlider;
	[SerializeField]
	private Dropdown renderingMode; // should change this to rendering mode

	public float opacity = 4.0f;
	public float threshold = 0.2f;
	public float intensity = 1.0f;

	public GameObject volumetricCube;
	private Material _rayMarchMaterial;
	private bool cameraFrozen = true;
	private float updateTime;

	private int _opacityID;
	private int _thresholdID;
	private int _intensityID;
	private int _renderID;
	private int _clipPlane1ID;

	private int volumeLayer;

	public void setFreezeAll(bool freezeAll){
		variables.freezeAll = freezeAll;
	}
		
	private void Start()
	{
		// Get game object variables
		_rayMarchMaterial = volumetricCube.GetComponent<Renderer> ().material;
		volumeLayer = (1 << LayerMask.NameToLayer ("TransparentFX"));

		// Get Property IDs
		_opacityID = Shader.PropertyToID("_Opacity");
		_intensityID = Shader.PropertyToID ("_Intensity");
		_thresholdID = Shader.PropertyToID ("_DataMin");
		_renderID = Shader.PropertyToID ("_RenderMode");
		_clipPlane1ID = Shader.PropertyToID ("_ClipPlane");

		updateTime = Time.time;
	}
		

	// More input variables
	[Header("Game Objects")]
	[SerializeField]
	private Transform clipPlane;
	[SerializeField]
	private Transform backClipPlane;
	[SerializeField]
	private Transform cubeTarget;

	private void LateUpdate()
	{
		// Set material rendering properties
		if (cubeTarget != null && clipPlane != null && clipPlane.gameObject.activeSelf) {
			var p = new Plane (cubeTarget.InverseTransformDirection (clipPlane.forward), cubeTarget.InverseTransformPoint (clipPlane.position));
			Vector3 scaledPlane = new Vector3 (p.normal.x * cubeTarget.localScale.x, p.normal.y * cubeTarget.localScale.y, p.normal.z * cubeTarget.localScale.z).normalized;

			_rayMarchMaterial.SetVector (_clipPlane1ID, new Vector4 (scaledPlane.x, scaledPlane.y, scaledPlane.z, p.distance));

			/*if (variables.sectionMode) {
				_rayMarchMaterial.SetVector ("_ClipPlane2", new Vector4 (scaledBackPlane.x, scaledBackPlane.y, scaledBackPlane.z, p2.distance));
			} else {
				_rayMarchMaterial.SetVector ("_ClipPlane2", new Vector4 (0.0f, 0.0f, 0.0f, 50.0f));
			}*/
		} else {
			_rayMarchMaterial.SetVector (_clipPlane1ID, Vector4.zero);
		}
			
		_rayMarchMaterial.SetFloat (_opacityID, opacity); // Blending strength 
		_rayMarchMaterial.SetFloat (_thresholdID, threshold); // alpha cutoff value
		_rayMarchMaterial.SetFloat (_intensityID, intensity); // blends image a bit better
		_rayMarchMaterial.SetFloat (_renderID, (float)renderingMode.value);
	}

	private void Update(){
		if (!variables.freezeAll) {
			// Update rendering values
			opacity = opacitySlider.value * opacitySlider.value;
			opacity += Input.GetAxis("OpacityAxis") * opacitySpeed * Time.deltaTime * opacity;

			threshold = thresholdSlider.value;
			threshold += Input.GetAxis ("ThresholdAxis") * thresholdSpeed * Time.deltaTime;

			intensity = intensitySlider.value * intensitySlider.value;
			intensity += Input.GetAxis ("IntensityAxis") * intensitySpeed * Time.deltaTime * intensity;

			if (Input.GetKeyUp (KeyCode.N)) {
				variables.sectionMode = !variables.sectionMode;
			}

			if (Input.GetKeyUp (KeyCode.Z)) {
				variables.sectionMode = false;
			}

			// Touch controls 
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
				intensity += yMove * intensityTouchSpeed * intensity;

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

			// Update sliders based on the values from keyboard, touch, or mouse input
			opacitySlider.value = Mathf.Sqrt(opacity);
			thresholdSlider.value = threshold;
			intensitySlider.value = Mathf.Sqrt (intensity);


		}

		// Check if we need to render a new image to the full screen quad
		if ((Input.anyKey && !variables.freezeAll) || (!variables.freezeMouse && (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0)) ||
			variables.triggerRender || Input.GetAxis("Mouse ScrollWheel")!=0) {
			variables.triggerRender = false;
			if (cameraFrozen) {
				freezeCamera (false);
			}
			if (Input.GetAxis ("ClipAxisX") != 0 || Input.GetAxis ("ClipAxisY") != 0 || Input.GetAxis ("ClipAxisZ") != 0) {
				updateTime = Time.time + 1.7f;
			} else {
				updateTime = (Time.time > updateTime) ? Time.time : updateTime;
			}
		} else if (!cameraFrozen && Time.time - updateTime > 0.1f) {
			freezeCamera(true);
		}
	}

	private void freezeCamera(bool freezeCamera){
		if (freezeCamera) {
			// Freeze camera, so that we're not wasting rendering passes rendering the same image again and again! 
			GetComponent<Camera> ().clearFlags = CameraClearFlags.Nothing;
			GetComponent<Camera> ().cullingMask = 0;
		} else {
			// Unfreeze camera: start rendering again! 
			GetComponent<Camera> ().cullingMask = volumeLayer;
			GetComponent<Camera> ().clearFlags = CameraClearFlags.SolidColor;
			GetComponent<Camera> ().backgroundColor = Color.clear;
		}
		cameraFrozen = freezeCamera;
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
