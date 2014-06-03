using UnityEngine;
using System.Collections;
using InControl;
public class Pause : MonoBehaviour {

	GameManager gRef;

	public static bool paused = false;
	// Use this for initialization
	void Start () {
		gRef = GameManager.GameManagerInstance;

	}
	
	// Update is called once per frame
	void Update () {
		if (InputManager.ActiveDevice.MenuWasPressed || Input.GetKeyDown("p")){
			if (paused == false){
			//	Time.timeScale = 0;
				paused = true;

			} else {
			//	Time.timeScale = 1;
				paused = false;
			}
		}
	}
}
