using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* 
 * This script helps with the hovering of the mouse when
 * the bookmarking display is open, highlighting the 
 * "share bookmark" button on hover.
 * 
 * The blue is not a very nice colour, and should probably
 * be updated - so that share bookmark moves from an off-white
 * to a strong white upon hover. 
 *
*/

public class shareBookmarkClick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	private Color blue = new Color(0.70f, 0.9f, 0.98f);
	private Color white = Color.white;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPointerEnter(PointerEventData eventData){
		variables.bookmarkHover = true;
		GetComponent<Text> ().color = blue;
	}

	public void OnPointerExit(PointerEventData eventData){
		variables.bookmarkHover = false;
		GetComponent<Text> ().color = white;
	}

	public void OnPointerClick(PointerEventData eventData){
		// create short link. 
		variables.bookmarkHover = false;
		GetComponent<Text> ().color = white;
		//Camera.main.GetComponent<bookmarker> ().createBookmarkURL ();

	}
}
