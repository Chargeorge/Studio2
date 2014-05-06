using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	Settings sRef;
	GameManager gRef;
	public float scoreBarH = 30;
	public float scoreBarW = 75;
	Rect TeamRect1, ScoreRect1;

	public GUIStyle victoryStyle;
	public GUIStyle subStyle;
	public GUIStyle subStyleHighlight;
	public string victoryString;

	float subStyleX;
	float subStyleY;
	float subStyleWidth;
	float subStyleHeight;

	bool restart;
	bool menu;
	bool loadingNewScreen;
	
	Rect TeamRect2, ScoreRect2;
	// Use this for initialization
	void Start () {
		sRef = Settings.SettingsInstance;
		gRef = GameManager.GameManagerInstance;

		restart = true;
		menu = false;

		TeamRect1 = new Rect((Screen.width - scoreBarW)*(0)+ 2, 0, scoreBarW, Screen.height);
		ScoreRect1  = new Rect((Screen.width - scoreBarW)*(0)+ 2, Screen.height, scoreBarW,0);
		TeamRect2 = new Rect((Screen.width - scoreBarW)*(1)+ 2, 0, scoreBarW, Screen.height);
		ScoreRect2  = new Rect((Screen.width - scoreBarW)*(1)+ 2, Screen.height, scoreBarW,0);

		subStyleX = (Screen.width/10);
		subStyleY = (Screen.height/8)*6;
		subStyleHeight = Screen.height/4;
		subStyleWidth = Screen.width/6;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void OnGUI(){

		TeamInfo team = gRef.teams[0];

		float perScore = team.score / sRef.valPointsToWin;
		ScoreRect1.height =-Screen.height *perScore;
		
	//	GUI.DrawTexture(TeamRect1, gRef.scoreBgTexture, ScaleMode.StretchToFill, true, 1.0f);
	//	GUI.DrawTexture(ScoreRect1, team.scoreTexture, ScaleMode.StretchToFill, true, 1.0f);

		 team = gRef.teams[1];
		
		perScore = team.score / sRef.valPointsToWin;
		ScoreRect2.height =-Screen.height *perScore;
		
	//	GUI.DrawTexture(TeamRect2, gRef.scoreBgTexture, ScaleMode.StretchToFill, true, 1.0f);
	//	GUI.DrawTexture(ScoreRect2, team.scoreTexture, ScaleMode.StretchToFill, true, 1.0f);


		//	GUI.DrawTexture(new Rect(0,(Screen.height - scoreBarH)*(PlayerNumber-1), Screen.width, scoreBarH), gRef.scoreBgTexture, ScaleMode.StretchToFill, true, 1.0f);
		//	GUI.DrawTexture(new Rect(0,(Screen.height - scoreBarH)*(PlayerNumber-1), Screen.width * perScore, scoreBarH), scoreTexture, ScaleMode.StretchToFill, true, 1.0f);
		
		//		GUI.DrawTexture(new Rect((Screen.width - scoreBarW)*(PlayerNumber-1)+ 2, 0, scoreBarW, Screen.height), gRef.scoreBgTexture, ScaleMode.StretchToFill,true, 1.0f);
		//		GUI.DrawTexture(new Rect((Screen.width - scoreBarW)*(PlayerNumber-1)+ 2, Screen.height, scoreBarW, -Screen.height *perScore),scoreTexture, ScaleMode.StretchToFill, true, 1.0f);
		
		
		int boxWidth = 1600;
		int boxHeight = 900;
		TeamInfo winningTeam;
		switch (gRef.currentState){
		case GameState.gameWon:

			if(Input.GetAxis("HorizontalPlayer1") > 0f&& restart){
				menu = true;
				restart = false;
			}
			if(Input.GetAxis("HorizontalPlayer1") < 0f && menu){
				restart = true;
				menu = false;
			}
			if(Input.GetButton("BuildPlayer1") && restart){
			//	audio.Play();
			//	Invoke("replay", 1.5f);
			}
			if(Input.GetButton("BuildPlayer1") && menu){
			//	audio.Play();
			//	Invoke("mainMenu", 1.5f);
			}
			winningTeam =gRef.vIsForVendetta.completingTeam;
			/*GUI.BeginGroup(new Rect(Screen.width/2 - boxWidth/2, Screen.height/2 - boxHeight/2, boxWidth, boxHeight));*/

			GUI.DrawTexture(new Rect(0,0,boxWidth,boxHeight), winningTeam.winTexture, ScaleMode.StretchToFill, true, 1.0f);
			/*GUI.EndGroup();*/

			victoryStyle.normal.textColor = winningTeam.teamColor;
			//Color32 blackText = new Color32(0,0,0, 255);
			//victoryStyle.normal.textColor = blackText;
			GUI.Label (new Rect(Screen.width/3, Screen.height /2 - 50, Screen.width/3, Screen.height/15), victoryString, victoryStyle);
			
			if(restart) GUI.Label (new Rect(subStyleX*2, subStyleY, subStyleWidth, subStyleHeight), "Restart", subStyleHighlight);
			if(!restart) GUI.Label(new Rect(subStyleX*2, subStyleY, subStyleWidth, subStyleHeight), "Restart", subStyle);
			if(menu) GUI.Label (new Rect(subStyleX*6.3f, subStyleY, subStyleWidth, subStyleHeight), "Menu", subStyleHighlight);
			if(!menu) GUI.Label(new Rect(subStyleX*6.3f, subStyleY, subStyleWidth, subStyleHeight), "Menu", subStyle);

			break;
		
		case GameState.gameRestartable:

			if(Input.GetAxis("HorizontalPlayer1") > 0f && restart && !loadingNewScreen){
				menu = true;
				restart = false;
			}
			if(Input.GetAxis("HorizontalPlayer1") < 0f && menu && !loadingNewScreen){
				restart = true;
				menu = false;
			}
			if(Input.GetButton("BuildPlayer1") && restart){
				if (!audio.isPlaying) audio.Play();
				loadingNewScreen = true;
				Invoke("replay", 1.5f);
			}
			if(Input.GetButton("BuildPlayer1") && menu){
				if (!audio.isPlaying) audio.Play();
				loadingNewScreen = true;
				Invoke("mainMenu", 1.5f);
			}

			winningTeam =gRef.vIsForVendetta.completingTeam;
			/*GUI.BeginGroup(new Rect(Screen.width/2 - boxWidth/2, Screen.height/2 - boxHeight/2, boxWidth, boxHeight));*/
			GUI.DrawTexture(new Rect(0,0,boxWidth,boxHeight), winningTeam.winTexture, ScaleMode.StretchToFill, true, 1.0f);
			/*GUI.EndGroup();*/
			//blackText = new Color32(0,0,0, 255);
			//victoryStyle.normal.textColor = blackText;
			GUI.Label (new Rect(Screen.width/3, Screen.height/2 - 50, Screen.width/3, Screen.height/15), victoryString, victoryStyle);
			
			if(restart) GUI.Label (new Rect(subStyleX*2, subStyleY, subStyleWidth, subStyleHeight), "Restart", subStyleHighlight);
			if(!restart) GUI.Label(new Rect(subStyleX*2, subStyleY, subStyleWidth, subStyleHeight), "Restart", subStyle);
			if(menu) GUI.Label (new Rect(subStyleX*6.3f, subStyleY, subStyleWidth, subStyleHeight), "Menu", subStyleHighlight);
			if(!menu) GUI.Label(new Rect(subStyleX*6.3f, subStyleY, subStyleWidth, subStyleHeight), "Menu", subStyle);

			
			break;

		case GameState.playing:

			break;

		case GameState.paused:
			GUI.DrawTexture(new Rect(0,0,boxWidth,boxHeight), (Texture)Resources.Load("Sprites/victoryBackground3"), ScaleMode.StretchToFill, true, 1.0f);
			GUI.Label (new Rect(Screen.width/3, Screen.height/2, Screen.width/3, Screen.height/15), "Pause", victoryStyle);
			break;
		}

	}

	public void replay(){
		Debug.Log("Cool");
		Application.LoadLevel("SiggWorking");
	}

	public void mainMenu(){
		Application.LoadLevel("PierreMenu");
	}
}
