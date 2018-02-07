using UnityEngine;
using System.Collections;

/* 
 * This script controls the movement of the camera. Many of the 
 * controls are bound to various phsyical keys: a future verison
 * may be better to assign these to Input buttons. This would 
 * allow keys to be edited on a per-user basis. 
 * 
*/

#if UNITY_EDITOR 
	using UnityEditor;
#endif

public class CameraMovement : MonoBehaviour {

	// Public variables
	public float speed = 0.2f;

	private float x;
	private float y;
	private float z;

	private float rotationX;
	private float rotationY;
	private float rotationZ;
	private Quaternion startRotation;

	public float mouseSpeed = 1.67F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -90F;
	public float maximumY = 90F;

	public float scrollSensitivity = 5.0f;

	public GameObject rightPanel;
	public GameObject leftPanel;

	// Private variables
	private GameObject bindingBox;
	private float xTop;
	private float xBottom;
	private float yTop;
	private float yBottom;
	private float zTop;
	private float zBottom;

	private bool fastMode = true;

	private float startSpeed;
	private float startMouseSpeed;

	// Use this for initialization
	void Start () {
		// Get initial parameters
		startRotation = transform.localRotation;

		startSpeed = speed;
		startMouseSpeed = mouseSpeed;

		// Set up binding box
		bindingBox = GameObject.Find("Binding Box");
		xTop = 0.4f * bindingBox.transform.localScale.x;
		yTop = 0.4f * bindingBox.transform.localScale.y;
		zTop = 0.4f * bindingBox.transform.localScale.z;
		xBottom = -0.4f * bindingBox.transform.localScale.x;
		yBottom = -0.4f * bindingBox.transform.localScale.y;
		zBottom = -0.4f * bindingBox.transform.localScale.z;

		transform.LookAt (new Vector3 (0, 0, 0));
		resetRotation ();

	}
	
	// Update is called once per frame
	void Update () {
		if (!variables.freezeAll) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				fastMode = !fastMode;
			}

			if (!fastMode) {
				speed = startSpeed / 2.5F;
				mouseSpeed = startMouseSpeed / 3.0F;
			} else {
				speed = startSpeed;
				mouseSpeed = startMouseSpeed;
			}


			// Translation
			transform.position += Input.GetAxis ("V1") * Camera.main.transform.up * Time.deltaTime * speed;
			transform.position += Input.GetAxis ("H1") * Camera.main.transform.right * Time.deltaTime * speed;
			transform.position += Input.GetAxis ("Z1") * Camera.main.transform.forward * Time.deltaTime * speed;
			transform.position += Input.GetAxis ("Mouse ScrollWheel") * Camera.main.transform.forward * Time.deltaTime * scrollSensitivity;

			// Rotation
			if (!variables.freezeMouse && !Input.GetMouseButton(1)) {
				// This is the 'first person' mouse mode 
				rotationX += (Input.GetAxis ("Mouse X")) * mouseSpeed;
				rotationY += (Input.GetAxis ("Mouse Y")) * mouseSpeed;

				rotationY = ClampAngle (rotationY, minimumY, maximumY); // Stops camera doing backflips!

				Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
				Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);

				transform.localRotation = startRotation * xQuaternion * yQuaternion;
			} 

			// Camera movement with mouse
			if (Input.GetMouseButton (1)) {
				transform.position -= 2000.0f * Input.GetAxis("Mouse X") * Camera.main.transform.right * Time.deltaTime * mouseSpeed / Screen.width;
				transform.position -= 2000.0f * Input.GetAxis("Mouse Y") * Camera.main.transform.up * Time.deltaTime * mouseSpeed / Screen.height;
			}

			// Keys
			if (Input.GetKeyDown (KeyCode.F)) {
				if (variables.freezeMouse) {
					// lock pointer
					variables.freezeMouse = false;
				} else {
					// unlock pointer
					variables.freezeMouse = true;
				}
			}


			// Touch controls: double-tap hides binding box
			if (Input.touchCount == 1) { 
				Touch touch0 = Input.GetTouch (0);
				if (touch0.tapCount == 2) {
					variables.showBindingBox = !variables.showBindingBox;
				}
			}
		}
	}

	void LateUpdate(){
		// Stop camera exiting the binding box
		leftPanel.SetActive (variables.showBindingBox && !variables.hidePanels);
		rightPanel.SetActive (variables.showBindingBox && !variables.hidePanels);

		if (!variables.freezeAll) {
			float xNow = transform.position.x;
			float yNow = transform.position.y;
			float zNow = transform.position.z;

			if (xNow > xTop) {
				xNow = xTop;
			} if (yNow > yTop) {
				yNow = yTop;
			} if (zNow > zTop){
				zNow = zTop;
			} if (xNow < xBottom) {
				xNow = xBottom;
			} if (yNow < yBottom) {
				yNow = yBottom;
			} if (zNow < zBottom) {
				zNow = zBottom;
			}

			transform.position = new Vector3 (xNow, yNow, zNow);
		}
	}

	// Helper functions: 
	public void resetRotation(){
		rotationX = 0;
		rotationY = 0;
		startRotation = transform.localRotation;
	}

	private static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}

}