using UnityEngine;
using System.Collections;

public class Chart : MonoBehaviour {

	public float chartTimer = 10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void scoreChart(){
		chartTimer -= Time.deltaTime; // I need timer which from a particular time goes to zero
		
		if (chartTimer == 0){
			
		} 
	}
}
