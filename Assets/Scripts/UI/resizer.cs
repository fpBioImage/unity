using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class resizer : UIBehaviour {

	public RectTransform canvasTransform;

	protected override void OnRectTransformDimensionsChange(){
		variables.hidePanels = (canvasTransform.sizeDelta.x / canvasTransform.sizeDelta.y < 1.1);
	}

}
