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
	public float displayedScore = 0.0f;
	public float scoreLerpRate = 0.1f;
	
	public AudioSource[] audioSources;

	// Use this for initialization
	void Start () {
		scoreBitFinal = transform.FindChild("ScoreBitFinalTarget").gameObject;
		transform.FindChild ("NoBuildLayer").renderer.material.color = team.tileColor;
		transform.FindChild ("Quad").renderer.material.color = team.tileColor;
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

		audioSources = gameObject.GetComponents<AudioSource>();
		for(int i = 0; i < audioSources.Length; i++){
			audioSources[i].volume = sRef.altarScoreVolume;
		if (team.teamNumber == 1) {
			audioSources[i].clip = Resources.Load("SFX/Altar_Score_Perc_Lo") as AudioClip;
			/*audioSources[1].clip = Resources.Load("SFX/Altar_Score_PentaScale_Lo_2") as AudioClip;
			audioSources[2].clip = Resources.Load("SFX/Altar_Score_PentaScale_Lo_3") as AudioClip;
			audioSources[3].clip = Resources.Load("SFX/Altar_Score_PentaScale_Lo_4") as AudioClip;
			audioSources[4].clip = Resources.Load("SFX/Altar_Score_PentaScale_Lo_5") as AudioClip;*/
		} else if(team.teamNumber == 2){
			audioSources[i].clip = Resources.Load("SFX/Altar_Score_Perc_Hi") as AudioClip;
			/*audioSources[1].clip = Resources.Load("SFX/Altar_Score_PentaScale_Hi_2") as AudioClip;
			audioSources[2].clip = Resources.Load("SFX/Altar_Score_PentaScale_Hi_3") as AudioClip;
			audioSources[3].clip = Resources.Load("SFX/Altar_Score_PentaScale_Hi_4") as AudioClip;
			audioSources[4].clip = Resources.Load("SFX/Altar_Score_PentaScale_Hi_5") as AudioClip;*/
		} else {
			//Debug.LogWarning ("Didn't load score sounds in Bar");
		}

		}
		
		/*audioSources[0].pitch = 1f;
		audioSources[1].pitch = 0.8f;
		audioSources[2].pitch = 1.2f;
		audioSources[3].pitch = 0.6f;
		audioSources[4].pitch = 1.4f;*/
		
	}
	
	// Update is called once per frame
	void Update () {
		float perScore = team.score / sRef.valPointsToWin;
		if (Mathf.Abs (displayedScore - perScore) < 0.001f) { 
			displayedScore = perScore;
		}	
		else {
			displayedScore = Mathf.Lerp (displayedScore, perScore, scoreLerpRate);
		}
				
		if(displayedScore <= 1){
			teamScale.y = endScale.y *displayedScore;
			teamPos.y = fixIt + endPosY *displayedScore;
		} else {
			teamPos.y = fixIt + endPosY;
			teamScale.y = endScale.y;
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
	
	public void SetScoreSound (AudioClip clip) {
		foreach (AudioSource a in audioSources) {
			a.clip = clip;
		}
	}
	
	public void PlayScoreSound () {
	
		bool foundEmptySource = false;
		for (int i = 0; i < audioSources.Length && !foundEmptySource; i++) {
			if (!audioSources[i].isPlaying) {
				audioSources[i].pitch = 1f + (Random.Range (-0.3f, 0.3f));
				audioSources[i].Play ();
				foundEmptySource = true;
			}	
		}
	
	/**	if (!audioSources[0].isPlaying) {
			audioSources[0].Play ();
		}
	*/
	}
	
}
