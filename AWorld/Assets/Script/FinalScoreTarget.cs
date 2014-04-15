using UnityEngine;
using System.Collections;

public class FinalScoreTarget : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PlayScoreAnimation(){
//		Debug.Log("Message recieved");
		GetComponent<ParticleSystem>().Emit (100);
	}
}
