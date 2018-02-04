using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class fpbRendering : MonoBehaviour {
	// Input variables, can be set in the Unity editor. 
	[SerializeField]
	private LayerMask volumeLayer;

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

	private Material _rayMarchMaterial;
	private bool updateRender = true;
	private float updateTime;

	private int _opacityID;
	private int _thresholdID;
	private int _intensityID;
	private int _renderID;
	private int _clipPlane1ID;

	public void setFreezeAll(bool freezeAll){
		variables.freezeAll = freezeAll;
	}


	private void Start()
	{
		// set default rendering parameters. If necessary.
		_rayMarchMaterial = GetComponent<Renderer> ().material;

		// Get Property IDs
		_opacityID = Shader.PropertyToID("_Opacity");
		_intensityID = Shader.PropertyToID ("_Intensity");
		_thresholdID = Shader.PropertyToID ("_DataMin");
		_renderID = Shader.PropertyToID ("_RenderMode");
		_clipPlane1ID = Shader.PropertyToID ("_ClipPlane");

		// Set default rendering options
			// first, try to get parameters from bookmark url.
			// then, try to get parameters from bookmark 0? 
			// next, try to get parameters from javascript string
			// if we still haven't got parameters, use the defaults.
			
		/*
		opacitySlider.value = opacity;
		intensitySlider.value = intensity;
		thresholdSlider.value = threshold;*/ // These can all be set from bookmarking tab at Start() time. 

		updateTime = Time.time;
	}

	public void updateRenderingMode(int newRendering){
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

	private void LateUpdate()
	{
		// This is where all the actual rendering is done
		if (updateRender) {
			if (cubeTarget != null && clipPlane != null && clipPlane.gameObject.activeSelf) {
				var p = new Plane (cubeTarget.InverseTransformDirection(clipPlane.forward), cubeTarget.InverseTransformPoint(clipPlane.position));
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

		}
	}

	private void Update(){
		// Pre-update
		if (Input.GetKeyDown (KeyCode.LeftControl) || Input.GetKeyDown (KeyCode.RightControl) ||
			Input.GetKeyDown (KeyCode.LeftAlt) || Input.GetKeyDown (KeyCode.RightAlt) ||
			Input.GetKeyDown (KeyCode.LeftApple) || Input.GetKeyDown (KeyCode.RightApple) ||
			Input.GetKeyDown (KeyCode.LeftCommand) || Input.GetKeyDown (KeyCode.RightCommand) ||
			Input.GetKeyDown (KeyCode.LeftWindows) || Input.GetKeyDown (KeyCode.RightWindows) ||
			Input.GetKeyDown (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
			variables.freezeAll = true;
		}
		if (Input.GetKeyUp (KeyCode.LeftControl) || Input.GetKeyUp (KeyCode.RightControl) ||
			Input.GetKeyUp (KeyCode.LeftAlt) || Input.GetKeyUp (KeyCode.RightAlt) ||
			Input.GetKeyUp (KeyCode.LeftApple) || Input.GetKeyUp (KeyCode.RightApple) ||
			Input.GetKeyUp (KeyCode.LeftCommand) || Input.GetKeyUp (KeyCode.RightCommand) ||
			Input.GetKeyUp (KeyCode.LeftWindows) || Input.GetKeyUp (KeyCode.RightWindows) ||
			Input.GetKeyUp (KeyCode.LeftShift) || Input.GetKeyUp (KeyCode.RightShift)) {
			variables.freezeAll = false;
		} 	

		if (!variables.freezeAll) {
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

		}

		// Continue updating renderer for 1s after any keypress
		if (Input.GetAxis ("ClipAxisX")!=0 || Input.GetAxis ("ClipAxisY")!=0 || Input.GetAxis ("ClipAxisZ")!=0) {
			updateTime = Time.time + 3;
		} else if (Input.anyKey || (!variables.freezeMouse && (Input.GetAxis("Mouse X")!=0 || Input.GetAxis("Mouse Y")!=0) )) {
			updateTime = Time.time;
		}
		updateRender = Time.time - updateTime < 1;
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
