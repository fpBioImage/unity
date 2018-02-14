using UnityEngine;
using System.Collections;

/*
 * This script contains global variables that are shared
 * between all scenes. It is never destroyed as long as
 * the program runs - that is, until the user closes the
 * webpage FP Bioimage is running in, or refreshes the 
 * page. 
 * 
 * I haven't been consistent with private and public 
 * variables. I began making all variables private, and
 * using get and set functions. However it became more
 * convenient to just declare variables as public and change
 * them directly, hence the disparity. 
 * 
 * 
*/

public class variables : MonoBehaviour {

	public static bool freezeMouse = true;
	public static bool freezeAll = false;
	public static bool jamesMode = false;
	public static bool vr = false;

	public static bool showBindingBox = true;
	public static bool sectionMode = false;
	public static bool hidePanels = true;

	public static bool loadBookmarkFromURL = false;
	public static bool bookmarkHover = true;

	public static bool offlineMode = true;
	public static int volumeReadyState = 0;

	public static FpbJSON fpbJSON = null;
	public static FpbBookmark fpbBookmark = null;

	public static bool triggerRender;
}
