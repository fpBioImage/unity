using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class starWarsControl : MonoBehaviour {

	private float deadzone = 0.05f;
	private float speed = 0.3f;
	private Vector3 velocity = Vector3.zero;

	private float zRotation = 0.0f;
	private float xRotation = 0.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// Movement
		Vector3 targetPosition = new Vector3 (0.0f, -0.4f, 0.8f);
		if (Input.GetAxis ("Z1") > deadzone) {
			targetPosition.z = 1.0f;
		} else if (Input.GetAxis ("Z1") < -deadzone) {
			targetPosition.z = 0.6f;
		}
		transform.localPosition = Vector3.SmoothDamp (transform.localPosition, targetPosition, ref velocity, speed);

		// Rotation
		float targetZAngle = 0.0f;
		float targetXAngle = 0.0f;

		if (Input.GetAxis ("H1") > deadzone) {
			targetZAngle = -40.0f;
		} else if (Input.GetAxis ("H1") < -deadzone) {
			targetZAngle = 40.0f;
		}

		if (Input.GetAxis ("V1") > deadzone) {
			targetXAngle = -40.0f;
		} else if (Input.GetAxis ("V1") < -deadzone) {
			targetXAngle = 20.0f;
		}

		float zAngle = Mathf.SmoothDampAngle (transform.eulerAngles.z, targetZAngle, ref zRotation, speed);
		float xAngle = Mathf.SmoothDampAngle (transform.eulerAngles.x, targetXAngle, ref xRotation, speed);
		transform.eulerAngles = new Vector3 (xAngle, 0.0f, zAngle);

	}
}
