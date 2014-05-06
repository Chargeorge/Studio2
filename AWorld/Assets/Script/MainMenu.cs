using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public GUIStyle headerStyle;
	public GUIStyle creditsStyle;
	public GUIStyle neutralStyle;
	public GUIStyle highlightStyle;
	public GUIStyle highlightStyleO;
	public GUIStyle highlightStyleQ;
	public static bool startSelected;
	public static bool optionsSelected;
	public static bool quitSelected;
	public float deadZone;
	public AudioClip select;
	public AudioClip launch;
	bool joystickActive = true;
	public bool loadingNewScreen;
	public bool quitting;

	public GameObject musicObject;
	AudioSource music;

	float startHeight = 0.55f;
	float optionsHeight = 0.67f;
	float quitHeight = 0.8f;

	bool turnOffMusic;



	public GameObject cursor;

	// Use this for initialization
	void Start () {
		startSelected = true;
		optionsSelected = false;
		quitSelected = false;
		loadingNewScreen = false;
		quitting = false;

		music = musicObject.GetComponent<AudioSource>();

		turnOffMusic = false;
	}
	
	// Update is called once per frame
	void Update () {

		GameObject _musicObject  = GameObject.FindGameObjectWithTag("Music");
		//Debug.Log(_musicObject);
		if(_musicObject == null){
			Instantiate(musicObject);
			musicObject.audio.Play();
		}

		/**
		// Handled in cursor now
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
		}*/

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

		if(Input.GetButtonDown("BuildPlayer1") && !loadingNewScreen){
			if(startSelected){
				//audioLerp(music, 0.0f, 0.2f);
				turnOffMusic = true;
				audio.PlayOneShot(launch, 0.9f);
				loadingNewScreen = true;
				Invoke("launchGame", 1.5f);
			}
			if(optionsSelected){
				audio.PlayOneShot(select, .9f);
				loadingNewScreen = true;
				Invoke ("launchOptions", 1.0f);
			}
			if(quitSelected){
				turnOffMusic = true;
				audioLerp(music, 0.0f, 0.2f);
				audio.PlayOneShot (select, .9f);
				if (!Application.isEditor) {
					quitting = true;
					Invoke ("quitApp", 1.0f);
				}
			}
		}

		if(turnOffMusic){
			music.volume -= 0.04f;
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

	public void audioLerp (AudioSource source, float target, float rate) {
		if (Mathf.Abs (source.volume - target) <= 0.001f) {
			source.volume = target;
		}
		else {
			source.volume = Mathf.Lerp (source.volume, target, rate);
		}
	}

	void OnGUI(){
		GUI.Label (new Rect(Screen.width/3, Screen.height/10, Screen.width/3, Screen.height/5), "GENESES", headerStyle); //DRAW THE TITLE
		GUI.Label (new Rect(Screen.width/3, Screen.height/4, Screen.width/3, Screen.height/5),
		           "a game by Pierre Depaz, Char George,\nSig Gunnarsson & Josh Raab", creditsStyle); //DRAW THE CREDITS

		if(startSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*startHeight, Screen.width/3, Screen.height/5), "BEGIN", highlightStyle);
		if(!startSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*startHeight, Screen.width/3, Screen.height/5), "BEGIN", neutralStyle);
		if(optionsSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*optionsHeight, Screen.width/3, Screen.height/5), "MODIFY", highlightStyleO);
		if(!optionsSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*optionsHeight, Screen.width/3, Screen.height/5), "MODIFY", neutralStyle);
		if(quitSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*quitHeight, Screen.width/3, Screen.height/5), "ABANDON", highlightStyleQ);
		if(!quitSelected) GUI.Label (new Rect(Screen.width/3, Screen.height*quitHeight, Screen.width/3, Screen.height/5), "ABANDON", neutralStyle);
	}
}
