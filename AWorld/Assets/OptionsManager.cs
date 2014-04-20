using UnityEngine;
using System.Collections;

public class OptionsManager : MonoBehaviour {

	public GUIStyle titleStyle;
	public GUIStyle subtitleStyle;
	public GUIStyle normalStyle;

	int height1;
	int height2;

	// Use this for initialization
	void Start () {
		height1 = Screen.height/3 - Screen.height/12;
		height2 = (Screen.height/3)*2 - Screen.height/12;

	
	}
	
	// Update is called once per frame
	void Update () {


	}

	void OnGUI(){
		GUI.Label (new Rect(Screen.width/3, Screen.height/9, Screen.width/3, 50), "MODIFY", titleStyle);

		GUI.Label (new Rect(Screen.width/8, height1, Screen.width/4, 50), "PLAYERS", subtitleStyle);
		GUI.Label (new Rect((Screen.width/8)*3, height1, Screen.width/4, 50), "FOG", subtitleStyle);
		GUI.Label (new Rect((Screen.width/8)*5, height1, Screen.width/4, 50), "INTEGRITY", subtitleStyle);

		GUI.Label (new Rect((Screen.width/4), height2, Screen.width/5, 50), "SPEED", subtitleStyle);
		GUI.Label (new Rect((Screen.width/4)*2, height2, Screen.width/5, 50), "SIZE", subtitleStyle);
	}
}
