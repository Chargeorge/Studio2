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

	public float threshold1;
	public float threshold2;
	public float threshold3;

	public float layerVolume;
	public float layerVolumeClimax;
	public float lerpRate;
	public float lerpRateFast;

	bool gamewon;

	// Use this for initialization
	void Start () {
		sRef = Settings.SettingsInstance;
		gamewon = false;

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

		if(scorePlayer1 > threshold1 || scorePlayer2 > threshold1){ //first layer is when one player gets closer to score
			audioLerp(layer1Lo, layerVolume, lerpRate);
		}

		if(scorePlayer1 > threshold2 || scorePlayer2 > threshold2){ //when one player is almost there
			audioLerp(layer1Lo, 0.0f, lerpRateFast);
			audioLerp(layer2LoMid, layerVolume, lerpRate);
		}

		if(scorePlayer1 > threshold3 || scorePlayer2 > threshold3 && !gamewon){ //this is when two players are really tied
			audioLerp(layer1Lo, 0.0f, lerpRateFast); //turn off the first layer
			audioLerp(layer2LoMid, 0.0f, lerpRateFast); //turn off the second layer
			audioLerp(layer3MidHi, layerVolumeClimax, lerpRateFast);
			audioLerp(soundtrack, layerVolumeClimax, lerpRate);
		}

		if(scorePlayer1 > 1 || scorePlayer2 > 1){
			gamewon = true;
			audioLerp(layer1Lo, 0.0f, lerpRateFast);
			audioLerp(layer2LoMid, 0.0f, lerpRateFast);
			audioLerp(layer3MidHi, 0.0f, lerpRateFast);
		}
	
	}

	public void audioLerp (AudioSource source, float target, float rate) {
		if (Mathf.Abs (source.volume - target) <= 0.001f) {
			source.volume = target;
		}
		else {
			source.volume = Mathf.Lerp (source.volume, target, rate);
		}
	}
}
