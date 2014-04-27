using UnityEngine;
using System.Collections;

public class Bar : MonoBehaviour {
	Settings sRef;
	GameManager gRef;
	public TeamInfo team;

	public Vector3 endScale;
	public Vector3 teamScale1;

	public Vector3 teamScale2;

	public float endPosY;
	public Vector3 teamPos1;
	public Transform bar;
	public Color32 barAlpha;
	public Transform capture;
	public Transform backBar;
	public GameObject teamBar1;
	public GameObject teamBar2;


	// Use this for initialization
	void Start () {
		sRef = Settings.SettingsInstance;
		gRef = GameManager.GameManagerInstance;
		//team = gRef.teams[0];

		endScale.x = 0.7f;
		endScale.y = 12.73845f; 
		teamScale1 = new Vector3 (0.7f,8,0);

		teamScale2 = new Vector3 (0.7f,0,0);

		endPosY = 6.4f;
		teamPos1 = new Vector3(0, 0, 0);
		bar = this.transform.FindChild("Bar");
		capture = this.transform.FindChild("Capture");
		backBar = this.transform.FindChild("BarBack");

	
//s		barAlpha = team.teamColor;
//		barAlpha.a = 100;
//		bar.renderer.material.color = barAlpha;
	}
	
	// Update is called once per frame
	void Update () {



		float perScore = team.score / sRef.valPointsToWin;


		teamScale1.y = endScale.y *perScore;
		teamPos1.y = 0.3f + endPosY *perScore;

		//teamScale1.y = 8;
		bar.localScale = teamScale1;
		bar.localPosition = teamPos1;

		bar.renderer.material.color = team.teamColor;
		capture.renderer.material.color = team.beaconColor;

		Color32 backBarColor = team.tileColor;
		//Color32 backBarColor = new Color32 (200, 200, 200, 170);
		backBarColor.a = 170;
		backBar.renderer.material.color = backBarColor;

	//	team = gRef.teams[1];
		
	//	perScore = team.score / sRef.valPointsToWin;
	//	teamScale2.y = endScale.y *perScore;
	}
	

}
