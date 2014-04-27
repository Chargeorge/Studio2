using UnityEngine;
using System.Collections;

public class Bar : MonoBehaviour {
	Settings sRef;
	GameManager gRef;
	TeamInfo team;

	public Vector3 endScale;
	public Vector3 teamScale1;

	public Vector3 teamScale2;

	public float endPosY;
	public Vector3 teamPos1;
	public Transform bar;
	public Color32 barAlpha;


	// Use this for initialization
	void Start () {
		sRef = Settings.SettingsInstance;
		gRef = GameManager.GameManagerInstance;
	//	team = gRef.teams[0];

		endScale.x = 0.7f;
		endScale.y = 12.73845f; 
		teamScale1 = new Vector3 (0.7f,0,0);

		teamScale2 = new Vector3 (0.7f,0,0);

		endPosY = 6.4f;
		teamPos1 = new Vector3(0, 0, 0);

		bar.renderer.material.color = team.teamColor;
		barAlpha = team.teamColor;
//		barAlpha.a = 100;
//		bar.renderer.material.color = barAlpha;
	}
	
	// Update is called once per frame
	void Update () {

		team = gRef.teams[0];

		bar = this.transform.FindChild("Bar");

		float perScore = team.score / sRef.valPointsToWin;

		teamScale1.y = endScale.y *perScore;
		teamPos1.y = 0.3f + endPosY *perScore;

		bar.localScale = teamScale1;
		bar.localPosition = teamPos1;
		
	//	team = gRef.teams[1];
		
	//	perScore = team.score / sRef.valPointsToWin;
	//	teamScale2.y = endScale.y *perScore;
	}
	

}
