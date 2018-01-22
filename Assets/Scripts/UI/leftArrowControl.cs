using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class leftArrowControl : MonoBehaviour {

	private bool isMovingLeft = false;
	private bool isMovingRight = false;

	public GameObject leftPanel;
	public Text arrowLeftText;
	public GameObject helpPanel;
	public GameObject infoPanel;

	public float speed = 1000.0f;

	private RectTransform rectTransform;
	private RectTransform helpTransform;
	private RectTransform infoTransform;

	// Use this for initialization
	void Start () {
		rectTransform = leftPanel.GetComponent<RectTransform> ();
		helpTransform = helpPanel.GetComponent<RectTransform> ();
		infoTransform = infoPanel.GetComponent<RectTransform> ();

		if (variables.leftPanelOpen) {
			openWithoutAnimation ();
		} else {
			closeWithoutAnimation ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (isMovingLeft) {
			rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x - speed * Time.deltaTime, rectTransform.anchoredPosition.y);
		}

		if (isMovingRight) {
			rectTransform.anchoredPosition = new Vector2 (rectTransform.anchoredPosition.x + speed * Time.deltaTime, rectTransform.anchoredPosition.y);
		}

		if (rectTransform.anchoredPosition.x < -155.0f){
			rectTransform.anchoredPosition = new Vector2(-155.0f, rectTransform.anchoredPosition.y);
			isMovingLeft = false;
			variables.leftPanelOpen = false;
			arrowLeftText.text = "▶";
			helpTransform.offsetMin = new Vector2 (30.0f, helpTransform.offsetMin.y);
			infoTransform.offsetMin = new Vector2 (30.0f, infoTransform.offsetMin.y);
		}

		if (rectTransform.anchoredPosition.x > 0.0f) {
			rectTransform.anchoredPosition = new Vector2(0.0f, rectTransform.anchoredPosition.y);
			isMovingRight = false;
			variables.leftPanelOpen = true;
			arrowLeftText.text = "◀";
			helpTransform.offsetMin = new Vector2 (175.0f, helpTransform.offsetMin.y);
			infoTransform.offsetMin = new Vector2 (175.0f, infoTransform.offsetMin.y);
		}

	}

	public void arrowClicked() {
		if (isMovingLeft || isMovingRight) {
			return;
		}

		if (variables.leftPanelOpen) {
			isMovingLeft = true;
		}

		if (!variables.leftPanelOpen) {
			isMovingRight = true;
		}

	}

	public void openWithoutAnimation(){
		rectTransform.anchoredPosition = new Vector2(0.0f, rectTransform.anchoredPosition.y);
		isMovingRight = false;
		variables.leftPanelOpen = true;
		arrowLeftText.text = "◀";
		helpTransform.offsetMin = new Vector2 (175.0f, helpTransform.offsetMin.y);
		infoTransform.offsetMin = new Vector2 (175.0f, infoTransform.offsetMin.y);
	}

	public void closeWithoutAnimation(){
		rectTransform.anchoredPosition = new Vector2(-155.0f, rectTransform.anchoredPosition.y);
		isMovingLeft = false;
		variables.leftPanelOpen = false;
		arrowLeftText.text = "▶";
		helpTransform.offsetMin = new Vector2 (30.0f, helpTransform.offsetMin.y);
		infoTransform.offsetMin = new Vector2 (30.0f, infoTransform.offsetMin.y);
	}


}
