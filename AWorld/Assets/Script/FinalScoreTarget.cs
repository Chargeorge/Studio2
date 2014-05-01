using UnityEngine;
using System.Collections;

public class FinalScoreTarget : MonoBehaviour {

	public void PlayScoreAnimation(){
//		Debug.Log("Message recieved");
		GetComponent<ParticleSystem>().Emit (10);
	}
}
