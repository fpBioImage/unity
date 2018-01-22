using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class hideSharingPanel : MonoBehaviour, IPointerClickHandler {

	public GameObject me;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPointerClick(PointerEventData eventData){
		me.SetActive (false);
	}
}
