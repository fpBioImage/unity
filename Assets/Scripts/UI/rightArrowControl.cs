using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class rightArrowControl : MonoBehaviour {

	private bool isMovingLeft = false;
	private bool isMovingRight = false;
	private bool rightPanelOpen = false;

	public GameObject rightPanel;
	public Text arrowRightText;
	public GameObject helpPanel;
	public GameObject infoPanel;

	public float speed = 1000.0f;

	private RectTransform rectTransform;
	private RectTransform helpTransform;
	private RectTransform infoTransform;

	// Use this for initialization
	void Start () {
		rectTransform = rightPanel.GetComponent<RectTransform> ();
		helpTransform = helpPanel.GetComponent<RectTransform> ();
		infoTransform = infoPanel.GetComponent<RectTransform> ();
		closeWithoutAnimation ();
	}
	
	// Update is called once per frame
	void Update () {
		if (isMovingLeft) {
			rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x - speed * Time.deltaTime, rectTransform.anchoredPosition.y);
		}

		if (isMovingRight) {
			rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x + speed * Time.deltaTime, rectTransform.anchoredPosition.y);
		}

		if (rectTransform.anchoredPosition.x < 0.0f){
			rectTransform.anchoredPosition = new Vector2(0.0f, rectTransform.anchoredPosition.y);
			isMovingLeft = false;
			rightPanelOpen = true;
			arrowRightText.text = "▶";
			helpTransform.offsetMax = new Vector2 (-175.0f, helpTransform.offsetMax.y);
			infoTransform.offsetMax = new Vector2 (-175.0f, infoTransform.offsetMax.y);
		}

		if (rectTransform.anchoredPosition.x > 155.0f) {
			rectTransform.anchoredPosition = new Vector2(155.0f, rectTransform.anchoredPosition.y);
			isMovingRight = false;
			rightPanelOpen = false;
			arrowRightText.text = "◀";
			helpTransform.offsetMax = new Vector2 (-30.0f, helpTransform.offsetMax.y);
			infoTransform.offsetMax = new Vector2 (-30.0f, infoTransform.offsetMax.y);
		}

	}

	public void arrowClicked() {
		if (isMovingLeft || isMovingRight) {
			return;
		}

		if (rightPanelOpen) {
			isMovingRight = true;
		}

		if (!rightPanelOpen) {
			isMovingLeft = true;
		}

	}

	private void openWithoutAnimation(){
		rectTransform.anchoredPosition = new Vector2(0.0f, rectTransform.anchoredPosition.y);
		isMovingLeft = false;
		rightPanelOpen = true;
		arrowRightText.text = "▶";
		helpTransform.offsetMax = new Vector2 (-175.0f, helpTransform.offsetMax.y);
		infoTransform.offsetMax = new Vector2 (-175.0f, infoTransform.offsetMax.y);
	}

	private void closeWithoutAnimation(){
		rectTransform.anchoredPosition = new Vector2(155.0f, rectTransform.anchoredPosition.y);
		isMovingRight = false;
		rightPanelOpen = false;
		arrowRightText.text = "◀";
		helpTransform.offsetMax = new Vector2 (-30.0f, helpTransform.offsetMax.y);
		infoTransform.offsetMax = new Vector2 (-30.0f, infoTransform.offsetMax.y);
	}

}
