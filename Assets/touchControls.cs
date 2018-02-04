using UnityEngine;
using System.Collections;

public class touchControls : MonoBehaviour {

	public float pinchSpeed = 10f;
	public float translateSpeed = 10f;
	public float rotateSpeed = 25000f;
	public GameObject cameraToMove;
	public GameObject lookDirection;

	Vector2 touchOld = new Vector2(0.0f,0.0f);

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		// Touch control
		if (variables.freezeMouse) {



			if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) {
				Vector2 touchDeltaPosition = Input.GetTouch (0).deltaPosition;

				float horizontalRot = (touchDeltaPosition.x) * rotateSpeed;
				float verticalRot = -(touchDeltaPosition.y) * rotateSpeed;

				if (touchOld.x - touchDeltaPosition.x == 0.0f && Mathf.Abs(touchDeltaPosition.x) < 3.1f)
					horizontalRot = 0.0f;

				if (touchOld.y - touchDeltaPosition.y == 0.0f && Mathf.Abs(touchDeltaPosition.y) < 3.1f)
					verticalRot = 0.0f;

				Vector3 vertRotAxis = transform.InverseTransformDirection (lookDirection.transform.TransformDirection (Vector3.right)).normalized;

				transform.Rotate (vertRotAxis, verticalRot, Space.Self);
				transform.Rotate (Vector3.up, horizontalRot, Space.World);

				touchOld = touchDeltaPosition;
				Debug.Log (TouchPhase.Stationary);

			} else if (Input.touchCount == 2) {
				// Store both touches.
				Touch touchZero = Input.GetTouch (0);
				Touch touchOne = Input.GetTouch (1);

				// Find the position in the previous frame of each touch.
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

				// Find the magnitude of the vector (the distance) between the touches in each frame.
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

				// Find the difference in the distances between each frame.
				float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

				// scale by screen size
				float scaledMagDiff = deltaMagnitudeDiff / Screen.width;

				// move camera forward and back based on pinch
				cameraToMove.transform.position -= cameraToMove.transform.forward * scaledMagDiff * pinchSpeed;

				// Calculate average x/y movement of 2 fingers
				float xMove = (touchZero.deltaPosition.x + touchOne.deltaPosition.x) / Screen.width; 
				float yMove = (touchZero.deltaPosition.y + touchOne.deltaPosition.y) / Screen.width;

				cameraToMove.transform.position += cameraToMove.transform.right * xMove * translateSpeed;
				cameraToMove.transform.position += cameraToMove.transform.up * yMove * translateSpeed;

			}
		}
	
	}
}
