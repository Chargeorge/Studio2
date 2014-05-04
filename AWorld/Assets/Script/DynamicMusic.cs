using UnityEngine;
using System.Collections;

public class DynamicMusic : MonoBehaviour {

	TeamInfo team;

	Settings sRef;

	public AudioSource soundtrack;
	public AudioSource layer1Lo;
	public AudioSource layer2LoMid;
	public AudioSource layer3MidHi;
	public AudioSource layer4Hi;

	float _s1;
	float _s2;
	float scorePlayer1;
	float scorePlayer2;

	public float layerVolume;
	public float layerVolumeClimax;
	public float lerpRate;
	public float lerpRateFast;

	// Use this for initialization
	void Start () {
		sRef = Settings.SettingsInstance;

		layerVolume = 0.4f;
		layerVolumeClimax = 1.0f;
		lerpRate = 0.1f;
		lerpRateFast = 0.3f;

		layer1Lo.volume = 0.0f;
		layer2LoMid.volume = 0.0f;
		layer3MidHi.volume = 0.0f;
		layer4Hi.volume = 0.0f;

		layer1Lo.Play();
		layer2LoMid.Play ();
		layer3MidHi.Play();
		layer4Hi.Play();

	}
	
	// Update is called once per frame
	void Update () {

		if(GameManager.GameManagerInstance.teams[0].score != null) _s1 = GameManager.GameManagerInstance.teams[0].score;
		if(GameManager.GameManagerInstance.teams[1].score != null) _s2 = GameManager.GameManagerInstance.teams[1].score;

		scorePlayer1 = _s1 / sRef.valPointsToWin;
		scorePlayer2 = _s2 / sRef.valPointsToWin;

//		Debug.Log("player1 score :"+scorePlayer1);

		if(scorePlayer1 > .66f || scorePlayer2 > .66f){ //first layer is when one player gets closer to score
			audioLerp(layer1Lo, layerVolume, lerpRate);
		}

		if(scorePlayer1 > .8f || scorePlayer2 > .8f){ //when one player is almost there
			audioLerp(layer1Lo, 0.0f, lerpRateFast);
			audioLerp(layer2LoMid, layerVolume, lerpRate);
		}

		if(scorePlayer1 > .9f || scorePlayer2 > .9f){ //this is when two players are really tied
			audioLerp(layer1Lo, 0.0f, lerpRateFast); //turn off the first layer
			audioLerp(layer2LoMid, 0.0f, lerpRateFast); //turn off the second layer
			audioLerp(layer3MidHi, layerVolumeClimax, lerpRateFast);
			audioLerp(soundtrack, layerVolumeClimax, lerpRateFast);
		}

		if(scorePlayer1 == 1 || scorePlayer2 == 2){
			audioLerp(layer1Lo, 0.0f, lerpRateFast);
			audioLerp(layer2LoMid, 0.0f, lerpRateFast);
			audioLerp(layer3MidHi, 0.0f, lerpRateFast);
		}
	
	}

	public void audioLerp (AudioSource source, float target, float rate) {
		if (Mathf.Abs (source.volume - target) <= 0.01f) {
			source.volume = target;
		}
		else {
			source.volume = Mathf.Lerp (source.volume, target, rate);
		}
	}
}
