using UnityEngine;
using System.Collections;

public class DynamicMusic : MonoBehaviour {

	public TeamInfo team;

	public AudioSource layer1Lo;
	public AudioSource layer2LoMid;
	public AudioSource layer3MidHi;
	public AudioSource layer4Hi;

	public GameObject GameManager;
	public GameManager GameManagerInstance;

	float scorePlayer1;
	float scorePlayer2;



	// Use this for initialization
	void Start () {

		//scorePlayer1 = GameManager.GameManagerInstance.teams[0];
		//scorePlayer2 = GameManager.GameManagerInstance.teams[1];


	}
	
	// Update is called once per frame
	void Update () {



		//either play all the time and lerp the volume in and out
		//or just play/stop it



		if(scorePlayer1 > 60 || scorePlayer2 > 60){
			//play Layer1
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
