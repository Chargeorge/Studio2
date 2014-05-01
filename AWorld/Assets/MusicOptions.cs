using UnityEngine;
using System.Collections;

public class MusicOptions : MonoBehaviour {

	GameObject mainMusic;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		mainMusic = GameObject.Find("Music");

		if(mainMusic == null){
			DontDestroyOnLoad(this);
		}
		if(mainMusic != null){
			Destroy(this);
		}
	}
}
