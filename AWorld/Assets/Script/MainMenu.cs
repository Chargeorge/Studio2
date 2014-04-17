using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public GUIStyle headerStyle;
	public GUIStyle creditsStyle;
	public GUIStyle neutralStyle;
	public GUIStyle highlightStyle;
	bool startSelected;
	bool optionsSelected;
	bool quitSelected;

	// Use this for initialization
	void Start () {
		startSelected = true;
		optionsSelected = false;
		quitSelected = false;	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		GUI.Label (new Rect(Screen.width/3, Screen.height/10, Screen.width/3, Screen.height/5), "GENESES", headerStyle); //DRAW THE TITLE
		GUI.Label (new Rect(Screen.width/3, Screen.height/4, Screen.width/3, Screen.height/5),
		           "a game by Pierre Depaz, Char George,\nSig Gunnarsson & Josh Raab", creditsStyle); //DRAW THE CREDITS

		if(startSelected) GUI.Label (new Rect(Screen.width/2.5f, Screen.height*0.55f, Screen.width/5, Screen.height/8), "BEGIN", highlightStyle);
		if(!startSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.55f, Screen.width/3, Screen.height/5), "BEGIN", neutralStyle);
		if(optionsSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.67f, Screen.width/3, Screen.height/5), "MODIFY", highlightStyle);
		if(!optionsSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.67f, Screen.width/3, Screen.height/5), "MODIFY", neutralStyle);
		if(quitSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.8f, Screen.width/3, Screen.height/5), "ABANDON", highlightStyle);
		if(!quitSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.8f, Screen.width/3, Screen.height/5), "ABANDON", neutralStyle);
	}
}
