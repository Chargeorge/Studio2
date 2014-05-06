using UnityEngine;
using System.Collections;

public class OptionsManager : MonoBehaviour {

	public GUIStyle titleStyle;
	public GUIStyle subtitleStyle;
	public GUIStyle highlightStyle;
	public GUIStyle smallSubtitleStyle;
	public GUIStyle smallHighlightStyle;

	int height1;
	int height2;
	int height3;

	public AudioClip select;
	public AudioClip launch;
	public bool loadingNewScreen;
	AudioSource music;

	public GameObject musicObject;
	bool turnOffMusic;

	public GameObject cursor;

	public static bool backSelected;
	public static bool startSelected;
	public static bool resetSelected;

	public static bool playersSelected;
	public GameObject playersDisplay;
	public Material twoPlayersMat;
	public Material fourPlayersMat;
	public static int numberOfPlayers;

	public static bool fogSelected;
	public GameObject fogDisplay;
	public Material fogOnMat;
	public Material fogOffMat;
	public static int fogDisplayed; //0 is off, 1 is on

	public static bool waterSelected;
	public GameObject terrainDisplay;
	public Material terrainIntensityNoWater;
	public Material terrainIntensitySwamp;
	public Material terrainIntensityFlooded;
	public static int terrainIntensity;

	public static bool sizeSelected;
	public GameObject sizeDisplay;
	public Material sizeSmallMat;
	public Material sizeNormalMat;
	public Material sizeLargeMat;
	public static int terrainSize;

	public static bool speedSelected;
	public GameObject speedDisplay;
	public Material speedNormalMat;
	public Material speedDoubleMat;
	public Material speedHalfMat;
	public static int gameSpeed;

	public static bool tutorialSelected;
	public GameObject tutorialDisplay;
	public Material tutorialOnMat;
	public Material tutorialOffMat;
	public static int tutorialToggle;

	float firstColumn = 1f;
	float secondColumn = 3f;
	float thirdColumn = 5f;
	float firstColumnBottom = 0.2f;
	float secondColumnBottom = 3.75f;
	float thirdColumnBottom = 7.5f;

	// Use this for initialization
	void Start () {
		turnOffMusic = false;

		musicObject = GameObject.FindWithTag("Music");

		if(musicObject == null){
			musicObject = GameObject.FindWithTag("Finish");
			musicObject.audio.Play();
		}

		music = musicObject.GetComponent<AudioSource>();

		height1 = Screen.height/3 - Screen.height/12;
		height2 = (Screen.height/3)*2 - Screen.height/12;
		height3 = (Screen.height/7)*6;

		//Set up the default values for the game
		numberOfPlayers = (PlayerPrefs.GetInt(PreferencesOptions.numberOfPlayers.ToString()) == 2) ? 2 : 4;
		playersDisplay.renderer.material = (PlayerPrefs.GetInt(PreferencesOptions.numberOfPlayers.ToString()) == 2) ? twoPlayersMat : fourPlayersMat;
		fogDisplayed = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? 1 : 0;
		fogDisplay.renderer.material = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? fogOnMat : fogOffMat;
		fogDisplay.transform.localScale = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? new Vector3 (2f, 1f, 1f) : new Vector3(1.5f, 1.5f, 1f);
		terrainIntensity = PlayerPrefs.GetInt (PreferencesOptions.terrainIntensity.ToString ());
		tutorialToggle = (PlayerPrefs.GetInt (PreferencesOptions.tutorial.ToString()) == 1) ? 1 : 0;
		tutorialDisplay.renderer.material = (PlayerPrefs.GetInt(PreferencesOptions.tutorial.ToString()) == 1) ? tutorialOnMat : tutorialOffMat;

		switch (terrainIntensity) {
			case 1:
				terrainDisplay.renderer.material = terrainIntensityNoWater;
				break;
			case 2:
				terrainDisplay.renderer.material = terrainIntensitySwamp;
				break;
			case 3:
				terrainDisplay.renderer.material = terrainIntensityFlooded;
				break;
			default:	//In case of errors
				Debug.LogWarning ("Terrain intensity was a weird value in PlayerPrefs");
				PlayerPrefs.SetInt(PreferencesOptions.terrainIntensity.ToString(), 2);
				terrainIntensity = 2;
				terrainDisplay.renderer.material = terrainIntensitySwamp;
				break;
		}
		terrainSize = PlayerPrefs.GetInt (PreferencesOptions.terrainSize.ToString ()); //1 is small, 2 is normal, 3 is large;
		switch (terrainSize) {
			case 1:
				sizeDisplay.renderer.material = sizeSmallMat;
				break;
			case 2:
				sizeDisplay.renderer.material = sizeNormalMat;
				break;
			case 3:
				sizeDisplay.renderer.material = sizeLargeMat;
				break;
			default:	//In case of errors
				Debug.LogWarning ("Terrain size was a weird value in PlayerPrefs");
				PlayerPrefs.SetInt(PreferencesOptions.terrainSize.ToString(), 2);
				terrainSize = 2;
				sizeDisplay.renderer.material = sizeNormalMat;
				break;
		}
		gameSpeed = PlayerPrefs.GetInt (PreferencesOptions.gameSpeed.ToString ()); //idem
		switch (gameSpeed) {
			case 1:
				speedDisplay.renderer.material = speedHalfMat;
				break;
			case 2:
				speedDisplay.renderer.material = speedNormalMat;
				break;
			case 3:
				speedDisplay.renderer.material = speedDoubleMat;
				break;
			default:	//In case of errors
				Debug.LogWarning ("Game speed was a weird value in PlayerPrefs");
				PlayerPrefs.SetInt(PreferencesOptions.gameSpeed.ToString(), 2);
				gameSpeed = 2;
				speedDisplay.renderer.material = speedNormalMat;
				break;
		}

		playersSelected = true;
		fogSelected = false;
		waterSelected = false;
		speedSelected = false;
		sizeSelected = false;
		tutorialSelected = false;
		backSelected = false;
		resetSelected = false;
		startSelected = false;
		loadingNewScreen = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		PlayerPrefs.SetInt(PreferencesOptions.numberOfPlayers.ToString(), numberOfPlayers);
		PlayerPrefs.SetInt(PreferencesOptions.fogOn.ToString(), fogDisplayed);
		PlayerPrefs.SetInt(PreferencesOptions.terrainIntensity.ToString(), terrainIntensity);
		PlayerPrefs.SetInt(PreferencesOptions.terrainSize.ToString(), terrainSize);
		PlayerPrefs.SetInt(PreferencesOptions.gameSpeed.ToString(), gameSpeed);
		PlayerPrefs.SetInt(PreferencesOptions.tutorial.ToString(), tutorialToggle);

//		Debug.Log("intensity :" + terrainIntensity);

		//LET'S FIRST DEFINE WHAT IS SELECTED WHEN
		/**
		// Handled in cursor now
		if(cursor.transform.position.y == 0.9f){

			sizeSelected = false;
			speedSelected = false;
			tutorialSelected = false;

			startSelected = false;
			backSelected = false;
			resetSelected = false;

			if(cursor.transform.position.x >= 1.80f && cursor.transform.position.x < 5.85f){
				playersSelected = true;
				fogSelected = false;
				waterSelected = false;

			} else if(cursor.transform.position.x > 5.95f && cursor.transform.position.x < 9.0f){
				playersSelected = false;
				fogSelected = true;
				waterSelected = false;
			} else if(cursor.transform.position.x > 9.2f && cursor.transform.position.x < 12.0f){
				playersSelected = false;
				fogSelected = false;
				waterSelected = true;
			}

		} else if(cursor.transform.position.y == -2.3f){

			playersSelected = false;
			fogSelected = false;
			waterSelected = false;

			startSelected = false;
			backSelected = false;
			resetSelected = false;

			if(cursor.transform.position.x >= 1.80f && cursor.transform.position.x < 5.85f){
				speedSelected = true;
				sizeSelected = false;
				tutorialSelected = false;
				
			} else if(cursor.transform.position.x > 5.95f && cursor.transform.position.x < 9.0f){
				speedSelected = false;
				sizeSelected = true;
				tutorialSelected = false;
			} else if(cursor.transform.position.x > 9.2f && cursor.transform.position.x < 12.0f){
				speedSelected = false;
				sizeSelected = false;
				tutorialSelected = true;
			}
		} else if(cursor.transform.position.y == -3.5f){
			
			playersSelected = false;
			fogSelected = false;
			waterSelected = false;

			sizeSelected = false;
			speedSelected = false;
			tutorialSelected = false;
			
			if(cursor.transform.position.x >= 1.0f && cursor.transform.position.x < 5.85f){
				backSelected = true;
				resetSelected = false;
				startSelected = false;
				
			} else if(cursor.transform.position.x > 5.95f && cursor.transform.position.x < 9.0f){
				backSelected = false;
				resetSelected = true;
				startSelected = false;
			} else if(cursor.transform.position.x > 9.2f && cursor.transform.position.x < 15.0f){
				backSelected = false;
				resetSelected = false;
				startSelected = true;
			}
		}
		*/

		//LET'S MAKE SHIT HAPPEN WHEN SOMETHING IS SELECTED AND THE BUTTON IS PRESSED
		if(Input.GetButtonDown("BuildPlayer1")){

			audio.Stop ();
			audio.PlayOneShot(select, 0.8f);

			if(playersSelected){
				if(numberOfPlayers == 2){
					playersDisplay.renderer.material = fourPlayersMat;
					numberOfPlayers = 4;
					} else if(numberOfPlayers == 4){
						playersDisplay.renderer.material = twoPlayersMat;
						numberOfPlayers = 2;
					}
				}
		
				if(fogSelected){
					if(fogDisplayed == 1){
						fogDisplay.renderer.material = fogOffMat;
						fogDisplay.transform.localScale = new Vector3 (1.5f, 1.5f, fogDisplay.transform.localScale.z);
						fogDisplayed = 0;
					} else if(fogDisplayed == 0){
						fogDisplay.renderer.material = fogOnMat;
						fogDisplay.transform.localScale = new Vector3 (2f, 1f, fogDisplay.transform.localScale.z);
						fogDisplayed = 1;
					}
				}
		
				if(waterSelected){
					if(terrainIntensity == 1){ //change from small to medium
						terrainDisplay.renderer.material = terrainIntensitySwamp;
						terrainIntensity = 2;
					} else if(terrainIntensity == 2){ //change from small to medium
						terrainDisplay.renderer.material = terrainIntensityFlooded;
						terrainIntensity = 3;
					} else if(terrainIntensity == 3){ //change from small to medium
						terrainDisplay.renderer.material = terrainIntensityNoWater;
						terrainIntensity = 1;
					}
				}
		
				if(speedSelected){
					if(gameSpeed == 1){
						speedDisplay.renderer.material = speedNormalMat;
						gameSpeed = 2;
					} else if(gameSpeed == 2){
						speedDisplay.renderer.material = speedDoubleMat;
						gameSpeed = 3;
					} else if(gameSpeed == 3){
						speedDisplay.renderer.material = speedHalfMat;
						gameSpeed = 1;
					}
				}
		
				if(sizeSelected){
					if(terrainSize == 1){
						sizeDisplay.renderer.material = sizeNormalMat;
						terrainSize = 2;
					} else if(terrainSize == 2){
						sizeDisplay.renderer.material = sizeLargeMat;
						terrainSize = 3;
					} else if(terrainSize == 3){
						sizeDisplay.renderer.material = sizeSmallMat;
						terrainSize = 1;
					}
				}
	
				if(tutorialSelected){
					if(tutorialToggle == 1){
						tutorialDisplay.renderer.material = tutorialOffMat;
						tutorialToggle = 0;
					} else if(tutorialToggle == 0){
						tutorialDisplay.renderer.material = tutorialOnMat;
						tutorialToggle = 1;
					}
				}
			
			if (resetSelected){
				playersDisplay.renderer.material = twoPlayersMat;
				numberOfPlayers = 2;
				fogDisplay.renderer.material = fogOnMat;
				fogDisplay.transform.localScale = new Vector3 (2f, 1f, fogDisplay.transform.localScale.z);
				fogDisplayed = 1;
				terrainDisplay.renderer.material = terrainIntensitySwamp;
				terrainIntensity = 2;
				speedDisplay.renderer.material = speedNormalMat;
				gameSpeed = 2;
				sizeDisplay.renderer.material = sizeNormalMat;
				terrainSize = 2;
				tutorialDisplay.renderer.material = tutorialOnMat;
				tutorialToggle = 1;
			}
		}
		
		if(startSelected && Input.GetButtonDown("BuildPlayer1") && !loadingNewScreen){
			turnOffMusic = true;
			audio.PlayOneShot(launch, 0.9f);
			loadingNewScreen = true;
			Invoke ("launchGame", 1.5f);
		}
		
		if(backSelected && Input.GetButton("BuildPlayer1") && !loadingNewScreen){
			audio.PlayOneShot(select, 0.8f);
			loadingNewScreen = true;
			Invoke ("goToMainMenu", 1.0f);
		}

		if(turnOffMusic){
			music.volume -= 0.04f;
		}

	}

	public void launchGame(){
		
		PlayerPrefs.SetInt(PreferencesOptions.numberOfPlayers.ToString(), numberOfPlayers);
		PlayerPrefs.SetInt(PreferencesOptions.fogOn.ToString(), fogDisplayed);
		PlayerPrefs.SetInt(PreferencesOptions.terrainIntensity.ToString(), terrainIntensity);
		PlayerPrefs.SetInt(PreferencesOptions.terrainSize.ToString(), terrainSize);
		PlayerPrefs.SetInt(PreferencesOptions.gameSpeed.ToString(), gameSpeed);

		Application.LoadLevel("SiggWorking");
	}
	
	public void goToMainMenu(){
	
		PlayerPrefs.SetInt(PreferencesOptions.numberOfPlayers.ToString(), numberOfPlayers);
		PlayerPrefs.SetInt(PreferencesOptions.fogOn.ToString(), fogDisplayed);
		PlayerPrefs.SetInt(PreferencesOptions.terrainIntensity.ToString(), terrainIntensity);
		PlayerPrefs.SetInt(PreferencesOptions.terrainSize.ToString(), terrainSize);
		PlayerPrefs.SetInt(PreferencesOptions.gameSpeed.ToString(), gameSpeed);

		Application.LoadLevel("PierreMenu");
	}

	void OnGUI(){
		GUI.Label (new Rect(Screen.width/3, Screen.height/9, Screen.width/3, 50), "MODIFY", titleStyle);

		if(!playersSelected) GUI.Label (new Rect((Screen.width/8)*firstColumn, height1, Screen.width/4, 50), "PLAYERS", subtitleStyle);
		if(playersSelected) GUI.Label (new Rect((Screen.width/8)*firstColumn, height1, Screen.width/4, 50), "PLAYERS", highlightStyle);
		if(!fogSelected) GUI.Label (new Rect((Screen.width/8)*secondColumn, height1, Screen.width/4, 50), "FOG", subtitleStyle);
		if(fogSelected) GUI.Label (new Rect((Screen.width/8)*secondColumn, height1, Screen.width/4, 50), "FOG", highlightStyle);
		if(!waterSelected) GUI.Label (new Rect((Screen.width/8)*thirdColumn, height1, Screen.width/4, 50), "WATER", subtitleStyle);
		if(waterSelected) GUI.Label (new Rect((Screen.width/8)*thirdColumn, height1, Screen.width/4, 50), "WATER", highlightStyle);

		if(!speedSelected) GUI.Label (new Rect((Screen.width/8)*firstColumn, height2, Screen.width/4, 50), "SPEED", subtitleStyle);
		if(speedSelected) GUI.Label (new Rect((Screen.width/8)*firstColumn, height2, Screen.width/4, 50), "SPEED", highlightStyle);
		if(!sizeSelected) GUI.Label (new Rect((Screen.width/8)*secondColumn, height2, Screen.width/4, 50), "SIZE", subtitleStyle);
		if(sizeSelected) GUI.Label (new Rect((Screen.width/8)*secondColumn, height2, Screen.width/4, 50), "SIZE", highlightStyle);
		if(!tutorialSelected) GUI.Label (new Rect((Screen.width/8)*thirdColumn, height2, Screen.width/4, 50), "TUTORIAL", subtitleStyle);
		if(tutorialSelected) GUI.Label (new Rect((Screen.width/8)*thirdColumn, height2, Screen.width/4, 50), "TUTORIAL", highlightStyle);

		if(!backSelected) GUI.Label (new Rect((Screen.width/10)*firstColumnBottom, height3, Screen.width/4, 50), "BACK", smallSubtitleStyle);
		if(backSelected) GUI.Label (new Rect((Screen.width/10)*firstColumnBottom, height3, Screen.width/4, 50), "BACK", smallHighlightStyle);
		if(!resetSelected) GUI.Label (new Rect((Screen.width/10)*secondColumnBottom, height3, Screen.width/4, 50), "RESET", smallSubtitleStyle);
		if(resetSelected) GUI.Label (new Rect((Screen.width/10)*secondColumnBottom, height3, Screen.width/4, 50), "RESET", smallHighlightStyle);		
		if(!startSelected) GUI.Label (new Rect((Screen.width/10)*thirdColumnBottom, height3, Screen.width/4, 50), "START", smallSubtitleStyle);
		if(startSelected) GUI.Label (new Rect((Screen.width/10)*thirdColumnBottom, height3, Screen.width/4, 50), "START", smallHighlightStyle);
	}
}
