using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	Settings sRef;
	GameManager gRef;
	public float scoreBarH = 30;
	public float scoreBarW = 75;
	Rect TeamRect1, ScoreRect1;

	
	
	Rect TeamRect2, ScoreRect2;
	// Use this for initialization
	void Start () {
		sRef = Settings.SettingsInstance;
		gRef = GameManager.GameManagerInstance;


		TeamRect1 = new Rect((Screen.width - scoreBarW)*(0)+ 2, 0, scoreBarW, Screen.height);
		ScoreRect1  = new Rect((Screen.width - scoreBarW)*(0)+ 2, Screen.height, scoreBarW,0);
		TeamRect2 = new Rect((Screen.width - scoreBarW)*(1)+ 2, 0, scoreBarW, Screen.height);
		ScoreRect2  = new Rect((Screen.width - scoreBarW)*(1)+ 2, Screen.height, scoreBarW,0);

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
			winningTeam =gRef.vIsForVendetta.completingTeam;
			GUI.BeginGroup(new Rect(Screen.width/2 - boxWidth/2, Screen.height/2 - boxHeight/2, boxWidth, boxHeight));
			GUI.DrawTexture(new Rect(0,0,boxWidth,boxHeight), winningTeam.winTexture, ScaleMode.StretchToFill, true, 1.0f);
			GUI.EndGroup();

			break;
		
		case GameState.gameRestartable:
			winningTeam =gRef.vIsForVendetta.completingTeam;
			GUI.BeginGroup(new Rect(Screen.width/2 - boxWidth/2, Screen.height/2 - boxHeight/2, boxWidth, boxHeight));
			GUI.DrawTexture(new Rect(0,0,boxWidth,boxHeight), winningTeam.winTexture, ScaleMode.StretchToFill, true, 1.0f);
			GUI.EndGroup();
			
			break;

		case GameState.playing:
			break;
		}

	}
}
