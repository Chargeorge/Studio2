using UnityEngine;
using System.Collections;

public class OptionsManager : MonoBehaviour {

	public GUIStyle titleStyle;
	public GUIStyle subtitleStyle;
	public GUIStyle highlightStyle;

	int height1;
	int height2;

	public GameObject cursor;

	bool backSelected = false;

	bool playersSelected = false;
	public GameObject playersDisplay;
	public Material twoPlayersMat;
	public Material fourPlayersMat;
	public static int numberOfPlayers;

	bool fogSelected = false;
	public GameObject fogDisplay;
	public Material fogOnMat;
	public Material fogOffMat;
	public static bool fogDisplayed;

	bool terrainSelected = false;
	public GameObject terrainDisplay;
	public Material terrainIntensity1;
	public Material terrainIntensity2;
	public Material terrainIntensity3;
	public static int terrainIntensity;

	bool sizeSelected = false;
	public GameObject sizeDisplay;
	public Material sizeNormalMat;
	public Material sizeSmallMat;
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

		//Set up the default values for the game
		numberOfPlayers = 2;
		fogDisplayed = true;
		terrainIntensity = 2;
		terrainSize = 2; //1 is small, 2 is normal, 3 is large;
		gameSpeed = 2; //idem

	
	}
	
	// Update is called once per frame
	void Update () {

		//LET'S FIRST DEFINE WHAT IS SELECTED WHEN
		if(cursor.transform.position.y == 0.9f){

			sizeSelected = false;
			speedSelected = false;
			backSelected = false;

			if(cursor.transform.position.x > 2.80f && cursor.transform.position.x < 5.85f){
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
			backSelected = false;

			if(cursor.transform.position.x > 4.85f && cursor.transform.position.x < 7.2f){
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
			backSelected = true;
		}


		//LET'S MAKE SHIT HAPPEN WHEN SOMETHING IS SELECTED AND THE BUTTON IS PRESSED
		if(playersSelected){
			if(Input.GetButtonDown("BuildPlayer1") && numberOfPlayers == 2){
				playersDisplay.renderer.material = fourPlayersMat;
				numberOfPlayers = 4;
			} else if(Input.GetButtonDown("BuildPlayer1") && numberOfPlayers == 4){
				playersDisplay.renderer.material = twoPlayersMat;
				numberOfPlayers = 2;
			}
		}

		if(fogSelected){
			if(Input.GetButtonDown("BuildPlayer1") && fogDisplayed){
				fogDisplay.renderer.material = fogOffMat;
				fogDisplayed = false;
			} else if(Input.GetButtonDown("BuildPlayer1") && !fogDisplayed){
				fogDisplay.renderer.material = fogOnMat;
				fogDisplayed = true;
			}
		}

		if(terrainSelected && Input.GetButtonDown("BuildPlayer1")){
			if(terrainIntensity == 1){ //change from small to medium
				terrainDisplay.renderer.material = terrainIntensity2;
				terrainIntensity = 2;
			} else if(terrainIntensity == 2){ //change from small to medium
				terrainDisplay.renderer.material = terrainIntensity3;
				terrainIntensity = 3;
			} else if(terrainIntensity == 3){ //change from small to medium
				terrainDisplay.renderer.material = terrainIntensity1;
				terrainIntensity = 1;
			}
		}

		if(speedSelected && Input.GetButtonDown("BuildPlayer1")){
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

		if(sizeSelected && Input.GetButtonDown("BuildPlayer1")){
			if(terrainSize == 1){
				sizeDisplay.renderer.material = speedNormalMat;
				terrainSize = 2;
			} else if(terrainSize == 2){
				sizeDisplay.renderer.material = speedDoubleMat;
				terrainSize = 3;
			} else if(terrainSize == 3){
				sizeDisplay.renderer.material = speedHalfMat;
				terrainSize = 1;
			}
		}

		if(backSelected && Input.GetButtonDown("BuildPlayer1")){
			Application.LoadLevel("PierreMenu");
		}

	}

	void OnGUI(){
		GUI.Label (new Rect(Screen.width/3, Screen.height/9, Screen.width/3, 50), "MODIFY", titleStyle);

		if(!playersSelected) GUI.Label (new Rect(Screen.width/8, height1, Screen.width/4, 50), "PLAYERS", subtitleStyle);
		if(playersSelected) GUI.Label (new Rect(Screen.width/8, height1, Screen.width/4, 50), "PLAYERS", highlightStyle);
		if(!fogSelected) GUI.Label (new Rect((Screen.width/8)*3, height1, Screen.width/4, 50), "FOG", subtitleStyle);
		if(fogSelected) GUI.Label (new Rect((Screen.width/8)*3, height1, Screen.width/4, 50), "FOG", highlightStyle);
		if(!terrainSelected) GUI.Label (new Rect((Screen.width/8)*5, height1, Screen.width/4, 50), "INTEGRITY", subtitleStyle);
		if(terrainSelected) GUI.Label (new Rect((Screen.width/8)*5, height1, Screen.width/4, 50), "INTEGRITY", highlightStyle);

		if(!speedSelected) GUI.Label (new Rect((Screen.width/4), height2, Screen.width/5, 50), "SPEED", subtitleStyle);
		if(speedSelected) GUI.Label (new Rect((Screen.width/4), height2, Screen.width/5, 50), "SPEED", highlightStyle);
		if(!sizeSelected) GUI.Label (new Rect((Screen.width/4)*2, height2, Screen.width/5, 50), "SIZE", subtitleStyle);
		if(sizeSelected) GUI.Label (new Rect((Screen.width/4)*2, height2, Screen.width/5, 50), "SIZE", highlightStyle);

		if(!backSelected) GUI.Label (new Rect((Screen.width/6)*4.8f, (Screen.height/6)*5, Screen.width/5, 50), "BACK", subtitleStyle);
		if(backSelected) GUI.Label (new Rect((Screen.width/6)*4.8f, (Screen.height/6)*5, Screen.width/5, 50), "BACK", highlightStyle);
	}
}
