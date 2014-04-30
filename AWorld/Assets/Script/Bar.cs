using UnityEngine;
using System.Collections;

public class Bar : MonoBehaviour {
	Settings sRef;
	GameManager gRef;

	public TeamInfo team;

	public Vector3 endScale;
	public Vector3 teamScale;

	public float endPosY;
	public Vector3 teamPos;
	public Transform bar;
	public Color32 barAlpha;
	public Transform capture;
	public Transform backBar;
	public Vector3 startPos;
	public GameObject scoreBitFinal;
	public float fixIt;

	// Use this for initialization
	void Start () {
		scoreBitFinal = transform.FindChild("ScoreBitFinalTarget").gameObject;
		sRef = Settings.SettingsInstance;
		gRef = GameManager.GameManagerInstance;

		//team = gRef.teams[0];

		endScale.x = 0.7f;
		endScale.y = sRef.scaleY; 

//		if (team.teamNumber == 1){
//			startPos = new Vector3(sizeRef.scorePos1.x, sizeRef.scorePos1.y, 0);
//		} else {
//			startPos = new Vector3(sizeRef.scorePos2.x, sizeRef.scorePos2.y, 0);
//		}

		teamPos = new Vector3(0 ,0,0);

		//this might be a problem.
		endPosY = sRef.scaleY/2;

		fixIt = sRef.bbLocalPosY - endPosY;
		
		//this.transform.position = startPos;

		teamScale = new Vector3 (1f,0,0);
		bar = this.transform.FindChild("Bar");
		capture = this.transform.FindChild("Capture");
		backBar = this.transform.FindChild("BarBack");
		teamScale.y = sRef.scaleY;
		backBar.transform.localScale = teamScale;
		teamPos.y = sRef.bbLocalPosY;
		backBar.transform.localPosition = teamPos; 
		backBar.renderer.materials[ 0 ].SetTextureScale( "_MainTex", new Vector2(0.7f,sRef.scaleY));
	
//s		barAlpha = team.teamColor;
//		barAlpha.a = 100;
//		bar.renderer.material.color = barAlpha;
	}
	
	// Update is called once per frame
	void Update () {



		float perScore = team.score / sRef.valPointsToWin;
	
		if(perScore <= 1){
			teamScale.y = endScale.y *perScore;
			teamPos.y = fixIt + endPosY *perScore;
		} else {
			teamPos.y = fixIt + endPosY;
		}
		scoreBitFinal.transform.position = new Vector3(scoreBitFinal.transform.position.x, teamPos.y+teamScale.y/2, -1);
		
		//teamScale1.y = 8;
		bar.localScale = teamScale;
		bar.localPosition = teamPos;

		bar.renderer.material.color = team.teamColor;
		capture.renderer.material.color = team.beaconColor;

		Color32 backBarColor = team.tileColor;
		//Color32 backBarColor = new Color32 (200, 200, 200, 170);
		backBarColor.a = 100;
		backBar.renderer.material.color = backBarColor;

	//	team = gRef.teams[1];
		
	//	perScore = team.score / sRef.valPointsToWin;
	//	teamScale2.y = endScale.y *perScore;
	}
	

}
