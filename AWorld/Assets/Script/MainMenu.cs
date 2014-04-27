using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public GUIStyle headerStyle;
	public GUIStyle creditsStyle;
	public GUIStyle neutralStyle;
	public GUIStyle highlightStyle;
	public GUIStyle highlightStyleO;
	public GUIStyle highlightStyleQ;
	bool startSelected;
	bool optionsSelected;
	bool quitSelected;
	public float deadZone;
	public AudioClip select;
	public AudioClip launch;
	bool joystickActive = true;
	public bool screenChanging;

	public GameObject cursor;

	// Use this for initialization
	void Start () {
		startSelected = false;
		optionsSelected = false;
		quitSelected = false;
		screenChanging = false;
	}
	
	// Update is called once per frame
	void Update () {

		if(cursor.transform.position.y < -0.4 && cursor.transform.position.y > -1.4){
			startSelected = true;
			optionsSelected = false;
			quitSelected = false;
		} else if(cursor.transform.position.y < -1.4 && cursor.transform.position.y > -2.4){
			startSelected = false;
			optionsSelected = true;
			quitSelected = false;
		} else if(cursor.transform.position.y < -2.4 && cursor.transform.position.y > -3.1f){
			startSelected = false;
			optionsSelected = false;
			quitSelected = true;
		} else{
			startSelected = false;
			optionsSelected = false;
			quitSelected = false;
		}

		/*if(joystickActive){
		if(startSelected){
			if(Input.GetAxisRaw("VerticalPlayer1") < -deadZone){
				startSelected = false;
				optionsSelected = true;
				joystickActive = false;
				StartCoroutine("InputDelay");
			} else if(Input.GetAxisRaw("VerticalPlayer1") > deadZone){
				startSelected = false;
				quitSelected = true;

			}
		}

		if(optionsSelected){
			if(Input.GetAxisRaw("VerticalPlayer1") < -deadZone){
				optionsSelected = false;
				quitSelected = true;
			} else if(Input.GetAxisRaw("VerticalPlayer1") > deadZone){
					joystickActive = false;

				optionsSelected = false;
				startSelected = true;
				
				StartCoroutine("InputDelay");
			}
		}

		if(quitSelected){
			if(Input.GetAxisRaw("VerticalPlayer1") < -deadZone){
				quitSelected = false;
				startSelected = true;
			} else if(Input.GetAxisRaw("VerticalPlayer1") > deadZone){
				quitSelected = false;
				optionsSelected = true;
			}
		}
		}*/

		if(Input.GetButtonDown("BuildPlayer1") && !screenChanging){
			if(startSelected){
				audio.PlayOneShot(launch, 0.9f);
				screenChanging = true;
				Invoke("launchGame", 1.5f);
			}
			if(optionsSelected){
				audio.PlayOneShot(select, 1.0f);
				screenChanging = true;
				Invoke ("launchOptions", 1.0f);
			}
			if(quitSelected){
				audio.PlayOneShot (select, 1.0f);
				if (!Application.isEditor) {
					screenChanging = true;
					Invoke ("quitApp", 1.0f);
				}
			}
		}
	}

	public void launchOptions(){
		Application.LoadLevel("PierreOptions");
	}

	public void launchGame(){
		Application.LoadLevel("SiggWorking");
	}
	
	public void quitApp(){
		Application.Quit();
	}

	void OnGUI(){
		GUI.Label (new Rect(Screen.width/3, Screen.height/10, Screen.width/3, Screen.height/5), "GENESES", headerStyle); //DRAW THE TITLE
		GUI.Label (new Rect(Screen.width/3, Screen.height/4, Screen.width/3, Screen.height/5),
		           "a game by Pierre Depaz, Char George,\nSig Gunnarsson & Josh Raab", creditsStyle); //DRAW THE CREDITS

		if(startSelected) GUI.Label (new Rect(Screen.width/2.5f, Screen.height*0.55f, Screen.width/5, Screen.height/8), "BEGIN", highlightStyle);
		if(!startSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.55f, Screen.width/3, Screen.height/5), "BEGIN", neutralStyle);
		if(optionsSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.67f, Screen.width/3, Screen.height/5), "MODIFY", highlightStyleO);
		if(!optionsSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.67f, Screen.width/3, Screen.height/5), "MODIFY", neutralStyle);
		if(quitSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.8f, Screen.width/3, Screen.height/5), "ABANDON", highlightStyleQ);
		if(!quitSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*0.8f, Screen.width/3, Screen.height/5), "ABANDON", neutralStyle);
	}
}
