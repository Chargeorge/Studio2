using UnityEngine;
using System.Collections;

public class FinalScoreTarget : MonoBehaviour {

	public void PlayScoreAnimation(int scoreBits){
//		Debug.Log("Message recieved");
		GetComponent<ParticleSystem>().Emit (scoreBits);
	}
}
