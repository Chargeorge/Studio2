using UnityEngine;
using System.Collections;

public class Pause : MonoBehaviour {


	bool pause = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.K)){
			if (pause == false){
				Time.timeScale = 0;
				pause = true;
			} else {
				Time.timeScale = 1;
				pause = false;
			}
		}
	}
}
