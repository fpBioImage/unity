using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class resizer : UIBehaviour {

	public RectTransform canvasTransform;
	public GameObject fullScreenQuad;

	protected override void OnRectTransformDimensionsChange(){
		variables.hidePanels = (canvasTransform.sizeDelta.x / canvasTransform.sizeDelta.y < 1.1);

		#if UNITY_EDITOR
		fullScreenQuad.SendMessage ("updateQuality");
		#endif
	}

}
