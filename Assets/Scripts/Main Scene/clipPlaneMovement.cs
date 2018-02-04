using UnityEngine;
using UnityEngine.UI;
using System.Collections;


/*
 * This script controls movement of the clip plane. The clip
 * plane is usually invisible, although active, until it is
 * moved by the user when it becomes visible for a few seconds. 
 * It begins by moving relative to the camera; however it can
 * be frozen to the model. 
 * 
 * Fading in and out is controlled by a state space machine
 * with 3 states. 
 * 
*/
public class clipPlaneMovement : MonoBehaviour {

	public GameObject sceneCamera;
	public GameObject model;

	public float speed = 1.0f;
	public float rotSpeed = 1.0f;

	public float fadeInTime = 1.0f;
	public float fadeOutTime = 1.0f;
	public float showTime = 2.0f;

	public GameObject guiButton;

	private Vector3 startPosition;
	private Quaternion startRotation;
	private float rotationX;
	private float rotationY;

	private float nearClipPlane;

	private bool freezePlane;

	private int state = 0;
	private float timer;

	// Use this for initialization
	void Start () {
		GetComponent<Renderer> ().material.color = Color.clear;
		nearClipPlane = Camera.main.GetComponent<Camera> ().nearClipPlane;
		transform.parent = sceneCamera.transform;
		startRotation = transform.localRotation;
		startPosition = transform.localPosition;
		freezePlane = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!variables.freezeAll) {
			// Check if the plane is attached to the camera or the model
			if (transform.parent == sceneCamera)
				freezePlane = false;
			else if (transform.parent == model.transform)
				freezePlane = true;

			// Only allow the plane to move if it is not frozen to the model
			if (!freezePlane) {
				if (Input.GetAxis ("ClipAxisX") != 0 || Input.GetAxis ("ClipAxisY") != 0 || Input.GetAxis ("ClipAxisZ") != 0) {
					state = 1;
				}

				transform.localPosition -= Input.GetAxis ("ClipAxisZ") * new Vector3(0,0,1) * speed * Time.deltaTime;

				rotationX -= Input.GetAxis ("ClipAxisX") * rotSpeed;
				rotationY += Input.GetAxis ("ClipAxisY") * rotSpeed;

				if (!variables.jamesMode) {
					rotationX = ClampAngle (rotationX, -135f, 135f);
					rotationY = ClampAngle (rotationY, -135f, 135f);
				}

				Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
				Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.right);

				transform.localRotation = startRotation * xQuaternion * yQuaternion;
			}

			// Freeze the plane to the model
			if (Input.GetKeyUp (KeyCode.X)) {
				freezePlaneToModel ();
			}

			// Reset the plane, unfreezing it from the model
			if (Input.GetKeyUp (KeyCode.Z)) {
				resetPlane ();
			}
		}

		if (state == 1) FadeIn ();
		if (state == 2)	countDown ();
		if (state == 3)	FadeOut ();

	}

	public void freezePlaneToModel(){
		if (!freezePlane){
			freezePlane = true;
			transform.SetParent (model.transform);
			guiButton.GetComponent<Button> ().interactable = false;
		}
	}

	public void resetPlane(){
		freezePlane = false;
		transform.SetParent (sceneCamera.transform);
		rotationX = 0.0f;
		rotationY = 0.0f;
		transform.localRotation = startRotation;
		transform.localPosition = startPosition;
		guiButton.GetComponent<Button> ().interactable = true;
	}
		

	void LateUpdate(){
	// Don't let the clipping plane come closer to the camera than the camera's nearClipPlane - otherwise nasty artefacts appear in the model 
		if (!freezePlane && transform.parent==sceneCamera.transform && transform.localPosition.z < nearClipPlane) {
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, nearClipPlane);
		}
	}

	// Helper functions: 
	void FadeOut(){
		Color currentColor = GetComponent<Renderer> ().material.color;
		GetComponent<Renderer> ().material.color = Color.Lerp(currentColor, Color.clear, Time.deltaTime/fadeOutTime);
		if (GetComponent<Renderer> ().material.color == Color.clear) state = 0;
	}
		
	void FadeIn(){
		Color currentColor = GetComponent<Renderer> ().material.color;
		GetComponent<Renderer> ().material.color = Color.Lerp (currentColor, Color.white, Time.deltaTime/fadeInTime);
		if (GetComponent<Renderer> ().material.color == Color.white) {
			state = 2;
			timer = showTime;
		}
	}
		
	void countDown(){
		timer = timer - Time.deltaTime;
		if (timer < 0.0f) state = 3;// should be 3. Use 2 for testing (no fade out)
	}


	private static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F) angle += 360F;
		if (angle > 360F) angle -= 360F;
		if (min < -360F) min += 360F;
		if (min > 360F) min -= 360F;
		if (max < -360F) max += 360F;
		if (max > 360F) max -= 360F;
		return Mathf.Clamp (angle, min, max);
	}
}
