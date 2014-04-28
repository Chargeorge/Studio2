using UnityEngine;
using System.Collections;

public class OptionsManager : MonoBehaviour {

	public GUIStyle titleStyle;
	public GUIStyle subtitleStyle;
	public GUIStyle highlightStyle;

	int height1;
	int height2;

	public AudioClip select;
	public AudioClip launch;
	public bool screenChanging;

	public GameObject soundtrack;
	public GameObject cursor;

	bool backSelected = false;
	bool startSelected = false;

	bool playersSelected = false;
	public GameObject playersDisplay;
	public Material twoPlayersMat;
	public Material fourPlayersMat;
	public static int numberOfPlayers;

	bool fogSelected = false;
	public GameObject fogDisplay;
	public Material fogOnMat;
	public Material fogOffMat;
	public static int fogDisplayed; //0 is off, 1 is on

	bool terrainSelected = false;
	public GameObject terrainDisplay;
	public Material terrainIntensityNoWater;
	public Material terrainIntensitySwamp;
	public Material terrainIntensityFlooded;
	public static int terrainIntensity;

	bool sizeSelected = false;
	public GameObject sizeDisplay;
	public Material sizeSmallMat;
	public Material sizeNormalMat;
	public Material sizeLargeMat;
	public static int terrainSize;

	bool speedSelected = false;
	public GameObject speedDisplay;
	public Material speedNormalMat;
	public Material speedDoubleMat;
	public Material speedHalfMat;
	public static int gameSpeed;

	// Use this for initialization
	void Start () {
		height1 = Screen.height/3 - Screen.height/12;
		height2 = (Screen.height/3)*2 - Screen.height/12;

		soundtrack = GameObject.Find("Soundtrack");

		//Set up the default values for the game
		numberOfPlayers = (PlayerPrefs.GetInt(PreferencesOptions.numberOfPlayers.ToString()) == 2) ? 2 : 4;
		playersDisplay.renderer.material = (PlayerPrefs.GetInt(PreferencesOptions.numberOfPlayers.ToString()) == 2) ? twoPlayersMat : fourPlayersMat;
		fogDisplayed = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? 1 : 0;
		fogDisplay.renderer.material = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? fogOnMat : fogOffMat;
		fogDisplay.transform.localScale = (PlayerPrefs.GetInt(PreferencesOptions.fogOn.ToString()) == 1) ? new Vector3 (2f, 1f, 1f) : new Vector3(1.5f, 1.5f, 1f);
		terrainIntensity = PlayerPrefs.GetInt (PreferencesOptions.terrainIntensity.ToString ());
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

		screenChanging = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		PlayerPrefs.SetInt(PreferencesOptions.numberOfPlayers.ToString(), numberOfPlayers);
		PlayerPrefs.SetInt(PreferencesOptions.fogOn.ToString(), fogDisplayed);
		PlayerPrefs.SetInt(PreferencesOptions.terrainIntensity.ToString(), terrainIntensity);
		PlayerPrefs.SetInt(PreferencesOptions.terrainSize.ToString(), terrainSize);
		PlayerPrefs.SetInt(PreferencesOptions.gameSpeed.ToString(), gameSpeed);

//		Debug.Log("intensity :" + terrainIntensity);

		//LET'S FIRST DEFINE WHAT IS SELECTED WHEN
		if(cursor.transform.position.y == 0.9f){

			sizeSelected = false;
			speedSelected = false;
			startSelected = false;
			backSelected = false;

			if(cursor.transform.position.x >= 1.80f && cursor.transform.position.x < 5.85f){
				playersSelected = true;
				fogSelected = false;
				terrainSelected = false;

			} else if(cursor.transform.position.x > 5.95f && cursor.transform.position.x < 9.0f){
				playersSelected = false;
				fogSelected = true;
				terrainSelected = false;
			} else if(cursor.transform.position.x > 9.2f && cursor.transform.position.x < 12.0f){
				playersSelected = false;
				fogSelected = false;
				terrainSelected = true;
			}

		} else if(cursor.transform.position.y == -2.6f){

			playersSelected = false;
			fogSelected = false;
			terrainSelected = false;
			startSelected = false;
			backSelected = false;

			if(cursor.transform.position.x >= 4.85f && cursor.transform.position.x < 7.2f){
				sizeSelected = false;
				speedSelected = true;
			} else if(cursor.transform.position.x > 7.2f && cursor.transform.position.x < 9.7f){
				sizeSelected = true;
				speedSelected = false;
			}
		} else if(cursor.transform.position.y == -3.42f){
			
			playersSelected = false;
			fogSelected = false;
			terrainSelected = false;
			sizeSelected = false;
			speedSelected = false;
			
			if (cursor.transform.position.x >= 12.7f) {
				startSelected = true;
				backSelected = false;
				
			} else if (cursor.transform.position.x <= 6.0f) {
				backSelected = true;
				startSelected = false;
			}
		}


		//LET'S MAKE SHIT HAPPEN WHEN SOMETHING IS SELECTED AND THE BUTTON IS PRESSED
		if(Input.GetButtonDown("BuildPlayer1") && cursor.transform.position.y > -3.0f){

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
	
			if(terrainSelected){
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
		}

		if(startSelected && Input.GetButtonDown("BuildPlayer1") && !screenChanging){
			audio.PlayOneShot(launch, 0.9f);
			screenChanging = true;
			soundtrack.audio.volume = Mathf.Lerp(soundtrack.audio.volume, 0, Time.deltaTime);
			Invoke ("launchGame", 1.5f);
		}
		
		if(backSelected && Input.GetButton("BuildPlayer1") && !screenChanging){
			audio.PlayOneShot(select, 0.8f);
			screenChanging = true;
			Invoke ("goToMainMenu", 1.0f);
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

		if(!playersSelected) GUI.Label (new Rect(Screen.width/8, height1, Screen.width/4, 50), "PLAYERS", subtitleStyle);
		if(playersSelected) GUI.Label (new Rect(Screen.width/8, height1, Screen.width/4, 50), "PLAYERS", highlightStyle);
		if(!fogSelected) GUI.Label (new Rect((Screen.width/8)*3.025f, height1, Screen.width/4, 50), "FOG", subtitleStyle);
		if(fogSelected) GUI.Label (new Rect((Screen.width/8)*3.025f, height1, Screen.width/4, 50), "FOG", highlightStyle);
		if(!terrainSelected) GUI.Label (new Rect((Screen.width/8)*5.1f, height1, Screen.width/4, 50), "WORLD", subtitleStyle);
		if(terrainSelected) GUI.Label (new Rect((Screen.width/8)*5.1f, height1, Screen.width/4, 50), "WORLD", highlightStyle);

		if(!speedSelected) GUI.Label (new Rect((Screen.width/4), height2, Screen.width/5, 50), "SPEED", subtitleStyle);
		if(speedSelected) GUI.Label (new Rect((Screen.width/4), height2, Screen.width/5, 50), "SPEED", highlightStyle);
		if(!sizeSelected) GUI.Label (new Rect((Screen.width/4)*2, height2, Screen.width/5, 50), "SIZE", subtitleStyle);
		if(sizeSelected) GUI.Label (new Rect((Screen.width/4)*2, height2, Screen.width/5, 50), "SIZE", highlightStyle);

		if(!startSelected) GUI.Label (new Rect((Screen.width/6)*4.8f, (Screen.height/6)*5, Screen.width/5, 50), "START", subtitleStyle);
		if(startSelected) GUI.Label (new Rect((Screen.width/6)*4.8f, (Screen.height/6)*5, Screen.width/5, 50), "START", highlightStyle);
		
		if(!backSelected) GUI.Label (new Rect((Screen.width/6)*0.0f, (Screen.height/6)*5, Screen.width/5, 50), "BACK", subtitleStyle);
		if(backSelected) GUI.Label (new Rect((Screen.width/6)*0.0f, (Screen.height/6)*5, Screen.width/5, 50), "BACK", highlightStyle);
	}
}
