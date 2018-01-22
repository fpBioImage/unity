using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * This script fades between scenes, making a less jarring
 * transition from the loading screen into the viewer.
 * 
*/

public class screenFader : MonoBehaviour {

	public float fadeSpeed = 3.0f;
	private bool sceneStarting = true;

	// Use this for initialization
	void Start () {
		sceneStarting = true;

		Color currentColor = GetComponent<Image> ().color;
		GetComponent<Image>().color = Color.Lerp(currentColor, Color.clear, fadeSpeed * Time.deltaTime);

	}
	
	// Update is called once per frame
	void Update () {
		if(sceneStarting)
			StartScene();
	}

	void StartScene ()
	{
		// Fade the texture to clear.
		FadeToClear();

		// If the texture is almost clear...
		if(GetComponent<Image>().color.a <= 0.05f)
		{
			// ... set the colour to clear and disable the GUITexture.
			GetComponent<Image>().color = Color.clear;

			// The scene is no longer starting.
			sceneStarting = false;
		}
	}

	void FadeToClear(){
		Color currentColor = GetComponent<Image> ().color;
		GetComponent<Image>().color = Color.Lerp(currentColor, Color.clear, fadeSpeed * Time.deltaTime);
	}
}


