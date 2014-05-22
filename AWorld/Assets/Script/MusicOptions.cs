using UnityEngine;
using System.Collections;

public class MusicOptions : MonoBehaviour {

	GameObject[] musicObjects;

	// Use this for initialization
	void Start () {
	
		musicObjects = GameObject.FindGameObjectsWithTag("MenuMusic");
		
		if (musicObjects.Length == 1) {
			DontDestroyOnLoad(this);
			audio.Play ();
		}
		
		else {	
			foreach (GameObject o in musicObjects) {
				if (!o.audio.isPlaying) {
					GameObject.Destroy (o);
				}
			}
		}
		
		//This doesn't work for some reason, so I just added something to destroy these at the top of GameManager's Start function
		if(Application.loadedLevelName == "SiggWorking"){
			Destroy(this);
		}	
	}
	
	// Update is called once per frame
	void Update () {

	}
}
